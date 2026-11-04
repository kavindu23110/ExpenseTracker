import { ChevronDown } from "lucide-react";
import { Fragment } from "react";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { money, shortDate } from "@/lib/format";
import type { Dashboard } from "@/lib/types";
import { BillDetails } from "./BillDetails";

const TH = "px-2 pt-2 pb-2.5 font-bold border-b border-line";
const TD = "px-2 py-2.5 border-b border-line-soft";

/** The bills table with its category chips; each bill expands in place. */
export function RecentBills({ data, category, onCategory, openId, onToggle }: {
  data: Dashboard;
  category: string;
  onCategory: (c: string) => void;
  openId: number | null;
  onToggle: (id: number) => void;
}) {
  const chips = [{ value: "", name: "All" }, ...data.categories.map((c) => ({ value: c, name: c }))];
  return (
    <Card label="Recent bills" className="gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-col gap-0.5">
          <h2 className="text-xl font-extrabold">Recent bills</h2>
          <p className="text-[13px] text-muted">Select a bill to expand it, and again to collapse it</p>
        </div>
        <div role="group" aria-label="Filter by category" className="flex flex-wrap gap-1.5">
          {chips.map((c) => (
            <button key={c.name} type="button" aria-pressed={category === c.value} onClick={() => onCategory(c.value)} className={`min-h-11 rounded-[10px] px-3 text-[13px] ${category === c.value ? "bg-ink font-bold text-white" : "bg-soft font-semibold text-ink-2"}`}>{c.name}</button>
          ))}
        </div>
      </div>
      <div className="flex flex-col sm:hidden">
        {data.bills.map((b) => (
          <div key={b.id} className="border-b border-line-soft last:border-0">
            <button type="button" onClick={() => onToggle(b.id)} aria-expanded={b.id === openId} aria-label={`${b.id === openId ? "Collapse" : "Expand"} bill from ${b.merchant}`} className="flex min-h-14 w-full items-center gap-2 py-3 text-left">
              <ChevronDown size={18} aria-hidden className={`flex-none text-muted ${b.id === openId ? "rotate-180" : ""}`} />
              <span className="flex min-w-0 flex-1 flex-col gap-1">
                <span className="flex items-baseline justify-between gap-3">
                  <strong className="min-w-0">{b.merchant}</strong>
                  <span className="flex-none font-mono text-sm font-medium">{money(b.amount)}</span>
                </span>
                <span className="text-[13px] text-muted">{b.category} · {b.itemCount} {b.itemCount === 1 ? "item" : "items"} · {shortDate(b.date)}</span>
                {(b.lang === "si" || b.scanned) && <span className="flex flex-wrap gap-1.5">{b.lang === "si" && <Badge tone="si" lang="si">සිංහල</Badge>}{b.scanned && <Badge tone="scan">FROM BILL</Badge>}</span>}
              </span>
            </button>
            {b.id === openId && <div className="-mx-2 pb-3"><BillDetails id={b.id} categories={data.allCategories} onClose={() => onToggle(b.id)} /></div>}
          </div>
        ))}
      </div>
      <div className="hidden overflow-x-auto sm:block">
        <table className="w-full min-w-[600px] border-collapse text-[15px]">
          <thead>
            <tr className="text-left text-xs tracking-[0.06em] text-muted uppercase">
              <th scope="col" className={`${TH} pl-0`}>Shop</th>
              <th scope="col" className={TH}>Category</th>
              <th scope="col" className={TH}>Items</th>
              <th scope="col" className={TH}>Date</th>
              <th scope="col" className={`${TH} pr-0 text-right`}>Amount</th>
            </tr>
          </thead>
          <tbody>
            {data.bills.map((b) => (
              <Fragment key={b.id}>
              <tr className={b.id === openId ? "bg-[#f0f7f6]" : ""}>
                <td className={`${TD} pl-0`}>
                  <div className="flex flex-wrap items-center gap-2">
                    <button type="button" onClick={() => onToggle(b.id)} aria-expanded={b.id === openId} aria-label={`${b.id === openId ? "Collapse" : "Expand"} bill from ${b.merchant}`} className="inline-flex min-h-11 items-center gap-1.5 px-1 text-left font-bold hover:underline"><ChevronDown size={18} aria-hidden className={`flex-none text-muted ${b.id === openId ? "rotate-180" : ""}`} />{b.merchant}</button>
                    {b.lang === "si" && <Badge tone="si" lang="si">සිංහල</Badge>}
                    {b.scanned && <Badge tone="scan">FROM BILL</Badge>}
                  </div>
                </td>
                <td className={TD}><span className="inline-block rounded-md bg-soft px-2 py-1 text-[13px] font-semibold text-ink-2">{b.category}</span></td>
                <td className={`${TD} text-muted`}>{b.itemCount} {b.itemCount === 1 ? "item" : "items"}</td>
                <td className={`${TD} text-muted`}>{shortDate(b.date)}</td>
                <td className={`${TD} pr-0 text-right font-mono font-medium`}>{money(b.amount)}</td>
              </tr>
              {b.id === openId && (
                <tr>
                  <td colSpan={5} className="border-b border-line-soft py-3">
                    <div className="sticky left-0 max-w-[calc(100vw-82px)]">
                      <BillDetails id={b.id} categories={data.allCategories} onClose={() => onToggle(b.id)} />
                    </div>
                  </td>
                </tr>
              )}
              </Fragment>
            ))}
          </tbody>
        </table>
      </div>
      {data.bills.length === 0 && <p className="text-[15px] text-muted">{data.allCount === 0 ? "No bills yet. Scan your first bill to get started." : "No bills match these filters."}</p>}
    </Card>
  );
}
