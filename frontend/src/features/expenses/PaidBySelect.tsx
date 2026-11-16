import { useQuery } from "@tanstack/react-query";
import { useEffect } from "react";
import { householdApi } from "@/features/household/householdApi";

/** Picks the member who paid; a new bill starts with whoever is signed in. */
export function PaidBySelect({ id, value, onChange, defaultToMe = false }: { id: string; value: string; onChange: (name: string) => void; defaultToMe?: boolean }) {
  const { data: members } = useQuery({ queryKey: ["members"], queryFn: householdApi.members });
  const { data: session } = useQuery({ queryKey: ["session"], queryFn: householdApi.session });
  const me = session?.user?.name;
  useEffect(() => {
    if (defaultToMe && !value && me) onChange(me);
  }, [defaultToMe, value, me, onChange]);
  return (
    <div className="flex min-w-0 flex-col gap-1.5">
      <label htmlFor={id} className="text-[13px] font-bold text-ink-2">Paid by</label>
      <select id={id} value={value} onChange={(e) => onChange(e.target.value)} className="min-h-11 min-w-0 rounded-[10px] border border-field bg-white px-3 text-[15px] text-ink">
        <option value="">Not recorded</option>
        {members?.map((m) => <option key={m.id} value={m.name}>{m.name}</option>)}
      </select>
    </div>
  );
}
