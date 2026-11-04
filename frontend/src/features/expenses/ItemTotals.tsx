import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Card } from "@/components/ui/Card";
import { api } from "@/lib/api";
import { money2, monthYear, qtyLabel } from "@/lib/format";
import type { Dashboard } from "@/lib/types";
import { ItemMergeWindow } from "./ItemMergeWindow";

const TH = "px-2 pt-2 pb-2.5 font-bold border-b border-line";
const TD = "px-2 py-2.5 border-b border-line-soft";
const ANSWER = "min-h-11 rounded-[10px] border border-field bg-white px-4 text-sm font-semibold disabled:opacity-50";

/** Total quantity and spend on each item in a chosen month, asking when two names may match. */
export function ItemTotals({ data }: { data: Dashboard }) {
  const queryClient = useQueryClient();
  const [month, setMonth] = useState("");
  const [merging, setMerging] = useState(false);
  const shown = data.itemMonths.find((m) => m.month === month) ?? data.itemMonths[0];
  const decide = useMutation({
    mutationFn: ({ keys, same }: { keys: string[]; same: boolean }) => api.decideItems(keys, same),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
  });
  return (
    <Card label="Spending by item" className="gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-col gap-0.5">
          <h2 className="text-xl font-extrabold">Spending by item</h2>
          <p className="text-[13px] text-muted">All bills in the month, whatever the filters above. Charges are not included.</p>
        </div>
        {shown && (
          <div className="flex flex-wrap items-center gap-3">
          <button type="button" onClick={() => setMerging(true)} className={ANSWER}>Merge items</button>
          <label className="flex items-center gap-2 text-[13px] font-bold text-ink-2">
            Month
            <select value={shown.month} onChange={(e) => setMonth(e.target.value)} className="min-h-11 rounded-[10px] border border-line bg-white px-3 text-sm font-medium text-ink">
              {data.itemMonths.map((m) => <option key={m.month} value={m.month}>{monthYear(`${m.month}-01`)}</option>)}
            </select>
          </label>
          </div>
        )}
      </div>
      {merging && <ItemMergeWindow months={data.itemMonths} onClose={() => setMerging(false)} />}

      {data.itemQuestions.map((q) => (
        <div key={`${q.keyA}|${q.keyB}`} className="flex flex-wrap items-center justify-between gap-3 rounded-[10px] bg-soft px-4 py-3">
          <p className="text-sm">Are <strong>{q.nameA}</strong> and <strong>{q.nameB}</strong> the same item?</p>
          <div className="flex gap-2">
            <button type="button" disabled={decide.isPending} onClick={() => decide.mutate({ keys: [q.keyA, q.keyB], same: true })} className={ANSWER}>Same item</button>
            <button type="button" disabled={decide.isPending} onClick={() => decide.mutate({ keys: [q.keyA, q.keyB], same: false })} className={ANSWER}>Different</button>
          </div>
        </div>
      ))}
      {decide.isError && <p role="alert" className="text-sm font-semibold text-warn">That answer could not be saved. Please try again.</p>}

      {!shown ? <p className="text-[15px] text-muted">No items yet. Item totals appear once you have saved bills.</p> : (
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="text-left text-[11px] tracking-[0.06em] text-muted uppercase">
                <th scope="col" className={`${TH} pl-0`}>Item</th>
                <th scope="col" className={`${TH} text-right`}>Total qty</th>
                <th scope="col" className={`${TH} pr-0 text-right`}>Total price</th>
              </tr>
            </thead>
            <tbody>
              {shown.items.map((i) => (
                <tr key={`${i.keys.join("|")}|${i.unit ?? ""}`}>
                  <td className={`${TD} pl-0`}>
                    <div className="flex flex-col items-start gap-px">
                      <span lang={i.nameSi ? "si" : "en"} className="font-semibold">{i.nameSi ?? i.name}</span>
                      {i.nameSi && <span lang="en" className="text-xs text-muted">{i.name}</span>}
                      {i.keys.length > 1 && (
                        <span className="text-xs text-muted">
                          Merged: {i.names.join(", ")} · <button type="button" disabled={decide.isPending} onClick={() => decide.mutate({ keys: i.keys, same: false })} className="min-h-6 font-semibold text-accent-ink underline">Separate</button>
                        </span>
                      )}
                    </div>
                  </td>
                  <td className={`${TD} text-right font-mono whitespace-nowrap text-ink-2`}>{qtyLabel(i.qty, i.unit)}</td>
                  <td className={`${TD} pr-0 text-right font-mono font-semibold whitespace-nowrap`}>{money2(i.amount)}</td>
                </tr>
              ))}
            </tbody>
            <tfoot>
              <tr>
                <th scope="row" colSpan={2} className="px-0 pt-3 text-left font-extrabold">All items</th>
                <td className="px-0 pt-3 text-right font-mono font-extrabold whitespace-nowrap">{money2(shown.total)}</td>
              </tr>
            </tfoot>
          </table>
        </div>
      )}
    </Card>
  );
}
