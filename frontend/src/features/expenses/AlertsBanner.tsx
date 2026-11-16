import { useQuery } from "@tanstack/react-query";
import { householdApi } from "@/features/household/householdApi";

const ICON = { budget: "Budget", bill: "Bill due", expiry: "Expiring" } as const;

/** Budgets running out, bills due and food about to expire. */
export function AlertsBanner() {
  const { data } = useQuery({ queryKey: ["alerts"], queryFn: householdApi.alerts });
  if (!data || data.length === 0) return null;
  return (
    <section aria-label="Reminders" className="flex flex-col gap-1.5 rounded-[10px] bg-warn-soft px-4 py-3 text-sm text-warn">
      <h2 className="font-bold">Needs attention</h2>
      <ul className="flex flex-col gap-1">
        {data.map((a, i) => <li key={i}><strong>{ICON[a.kind] ?? "Note"}:</strong> {a.text}</li>)}
      </ul>
    </section>
  );
}
