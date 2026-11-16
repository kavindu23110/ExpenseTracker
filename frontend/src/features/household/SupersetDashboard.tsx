import { embedDashboard } from "@superset-ui/embedded-sdk";
import { useQuery } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { Card } from "@/components/ui/Card";
import { householdApi } from "./householdApi";
import { HEADING } from "./ui";

/** The household's Apache Superset dashboard, shown inside the page for whoever is signed in. */
export function SupersetDashboard() {
  const { data } = useQuery({ queryKey: ["superset"], queryFn: householdApi.superset, staleTime: Infinity });
  const mount = useRef<HTMLDivElement>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!data?.enabled || !data.publicUrl || !data.dashboardId || !mount.current) return;
    const el = mount.current;
    let cancelled = false;
    let unmount: (() => void) | undefined;
    embedDashboard({
      id: data.dashboardId,
      supersetDomain: data.publicUrl,
      mountPoint: el,
      fetchGuestToken: async () => (await householdApi.supersetToken()).token,
      dashboardUiConfig: { hideTitle: true, hideChartControls: false, filters: { expanded: false } },
    })
      .then((d) => {
        if (cancelled) d.unmount();
        else unmount = d.unmount;
      })
      .catch(() => setError("The charts could not be loaded. Check that WARP is connected and try again."));
    return () => {
      cancelled = true;
      unmount?.();
      el.replaceChildren();
    };
  }, [data]);

  if (!data?.enabled) return null;
  return (
    <Card label="More charts" className="gap-3">
      <div className="flex flex-col gap-0.5">
        <h2 className={HEADING}>More charts</h2>
        <p className="text-[13px] text-muted">From the household&apos;s Superset dashboard. Use the filters inside it to change the period.</p>
      </div>
      {error && <p role="alert" className="text-sm font-semibold text-warn">{error}</p>}
      <div ref={mount} className="h-[720px] w-full overflow-hidden rounded-[10px] border border-line-soft [&>iframe]:h-full [&>iframe]:w-full [&>iframe]:border-0" />
    </Card>
  );
}
