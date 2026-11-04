import { money } from "@/lib/format";
import type { Dashboard } from "@/lib/types";

function Stat({ label, value, plain, wide }: { label: string; value: React.ReactNode; plain?: boolean; wide?: boolean }) {
  return (
    <div className={`flex min-w-0 flex-col gap-2 rounded-2xl border border-line bg-white p-4 sm:col-span-1 sm:p-5 ${wide ? "col-span-2" : ""}`}>
      <p className="text-[13px] font-semibold text-muted">{label}</p>
      <p className={plain ? "text-xl font-extrabold break-words sm:text-2xl" : "font-mono text-[28px] font-semibold"}>{value}</p>
    </div>
  );
}

/** The four headline numbers. */
export function SummaryCards({ data }: { data: Dashboard }) {
  const { summary } = data;
  return (
    <section aria-label="Summary" className="grid grid-cols-2 gap-3 sm:grid-cols-[repeat(auto-fit,minmax(200px,1fr))] sm:gap-4">
      <Stat label={`Total spent · ${data.periodLabel}`} value={money(summary.total)} wide />
      <Stat label="Bills logged" value={summary.count} />
      <Stat label="Bills in Sinhala" value={summary.sinhalaCount} />
      <Stat label="Biggest category" value={summary.topCategory ?? "—"} plain wide />
    </section>
  );
}
