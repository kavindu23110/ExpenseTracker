import { Bar } from "@/components/ui/Bar";
import { money } from "@/lib/format";
import type { Dashboard } from "@/lib/types";

/** Spending per category and how auto-categorise works. */
export function CategoryBreakdown({ data }: { data: Dashboard }) {
  return (
    <aside aria-label="Spending by category" className="flex flex-col gap-[18px] rounded-2xl border border-line bg-white p-6">
      <h2 className="text-xl font-extrabold">By category</h2>
      {data.breakdown.map((b) => (
        <div key={b.name} className="flex flex-col gap-1.5">
          <div className="flex justify-between gap-2 text-sm">
            <span className="font-bold">{b.name}</span>
            <span className="font-mono text-ink-2">{money(b.amount)} · {b.share}%</span>
          </div>
          <Bar percent={b.percent} />
        </div>
      ))}
      <div className="flex flex-col gap-1.5 border-t border-line-soft pt-4">
        <h3 className="text-sm font-extrabold">How auto-categorise works</h3>
        <p className="text-sm leading-[1.55] text-muted">Shop names and item lines are matched against English and Sinhala keywords for each category — <span lang="si">සහල්</span> (rice) or <span lang="si">ෆාමසිය</span> (pharmacy), for example. When you change a suggestion, that shop is remembered for next time.</p>
      </div>
    </aside>
  );
}
