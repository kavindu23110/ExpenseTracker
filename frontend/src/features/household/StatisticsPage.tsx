import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Bar } from "@/components/ui/Bar";
import { Card } from "@/components/ui/Card";
import { money, monthYear } from "@/lib/format";
import { householdApi } from "./householdApi";
import type { StatisticsRow } from "./types";
import { HEADING } from "./ui";

const monthLabel = (month: string) => new Date(`${month}-01T00:00:00`).toLocaleDateString("en-US", { month: "short" });

function Tile({ label, value, note }: { label: string; value: string; note?: string }) {
  return (
    <div className="flex min-w-0 flex-col gap-1 rounded-2xl border border-line bg-white p-4 sm:p-5">
      <p className="text-[13px] font-semibold text-muted">{label}</p>
      <p className="font-mono text-xl font-semibold break-words sm:text-2xl">{value}</p>
      {note && <p className="text-xs text-muted">{note}</p>}
    </div>
  );
}

function Breakdown({ title, rows, empty }: { title: string; rows: StatisticsRow[]; empty: string }) {
  const top = rows[0]?.amount || 1;
  return (
    <Card label={title} className="gap-3">
      <h2 className={HEADING}>{title}</h2>
      {rows.length === 0 && <p className="text-[15px] text-muted">{empty}</p>}
      {rows.map((r) => (
        <div key={`${r.nameSi ?? r.name}`} className="flex flex-col gap-1">
          <div className="flex justify-between gap-3 text-sm">
            <span lang={r.nameSi ? "si" : "en"} className="min-w-0 font-semibold">{r.nameSi ?? r.name}</span>
            <span className="flex-none font-mono">{money(r.amount)}</span>
          </div>
          <Bar percent={(r.amount / top) * 100} />
        </div>
      ))}
    </Card>
  );
}

/** Spending month by month; choosing a month shows where its money went. */
export function StatisticsPage() {
  const { data, isError } = useQuery({ queryKey: ["statistics"], queryFn: householdApi.statistics });
  const [chosen, setChosen] = useState<string | null>(null);
  if (isError) return <p role="alert" className="text-warn">This page could not be loaded.</p>;
  if (!data) return <p role="status" className="text-muted">Loading…</p>;
  const index = Math.max(data.months.findIndex((m) => m.month === chosen), chosen ? 0 : data.months.length - 1);
  const month = data.months[index];
  const previous = data.months[index - 1];
  const highest = Math.max(...data.months.map((m) => m.total), 1);
  const difference = previous ? month.total - previous.total : 0;
  const name = monthYear(`${month.month}-01`);
  return (
    <>
      <div className="flex flex-col gap-1">
        <h1 className="text-[32px] font-extrabold tracking-[-0.02em]">Statistics</h1>
        <p className="text-[15px] text-muted">Where your money went, from your saved bills. Choose a month to see its details.</p>
      </div>
      <div className="grid grid-cols-2 gap-3 sm:gap-4 lg:grid-cols-4">
        <Tile label={`Spent in ${name}`} value={money(month.total)} />
        <Tile label="Compared with the month before" value={previous ? `${difference > 0 ? "+" : difference < 0 ? "−" : ""}${money(Math.abs(difference))}` : "—"} note={previous ? monthYear(`${previous.month}-01`) : undefined} />
        <Tile label="Monthly average" value={data.averagePerMonth == null ? "—" : money(data.averagePerMonth)} note="Finished months only" />
        <Tile label="Bills" value={String(month.billCount)} note={month.billCount ? `${money(Math.round(month.total / month.billCount))} a bill on average` : undefined} />
      </div>
      <Card label="Spending by month" className="gap-3">
        <h2 className={HEADING}>Spending by month</h2>
        <div role="group" aria-label="Choose a month" className="flex h-44 items-stretch gap-0.5 sm:gap-2">
          {data.months.map((m, i) => (
            <button key={m.month} type="button" aria-pressed={i === index} aria-label={`${monthYear(`${m.month}-01`)}: ${money(m.total)}`} title={`${monthYear(`${m.month}-01`)}: ${money(m.total)}`} onClick={() => setChosen(m.month)} className="flex min-w-0 flex-1 flex-col justify-end gap-1.5">
              <span className="flex min-h-0 flex-1 flex-col justify-end border-b border-line">
                {m.total > 0 && <span className={`mx-auto block min-h-1 w-full max-w-10 rounded-t ${i === index ? "bg-accent" : "bg-[#9fd3cb]"}`} style={{ height: `${(m.total / highest) * 100}%` }} />}
              </span>
              <span className={`text-[11px] sm:text-xs ${i === index ? "font-bold text-ink" : "text-muted"}`}>{monthLabel(m.month)}</span>
            </button>
          ))}
        </div>
      </Card>
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <Breakdown title="By category" rows={month.categories} empty={`No bills in ${name}.`} />
        <Breakdown title="By shop" rows={month.shops} empty={`No bills in ${name}.`} />
        <Breakdown title="Top items" rows={month.items} empty={`No bills in ${name}.`} />
      </div>
    </>
  );
}
