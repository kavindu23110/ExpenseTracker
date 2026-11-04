import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Card } from "@/components/ui/Card";
import { money2, monthYear, qtyLabel } from "@/lib/format";
import { householdApi } from "./householdApi";
import { ItemCombobox } from "./ItemCombobox";
import type { Pantry } from "./types";
import { BUTTON, HEADING, INPUT, LABEL, PRIMARY } from "./ui";
import { useSaveAction } from "./useSaveAction";

/** Edits how much of a line to buy; a blank quantity clears the amount. */
function AmountForm({ line, busy, onSave }: { line: Pantry["shopping"][number]; busy: boolean; onSave: (qty: number | null, unit: string) => void }) {
  const saved = { qty: line.qty?.toString() ?? "", unit: line.unit ?? "" };
  const [amount, setAmount] = useState(saved);
  const changed = amount.qty !== saved.qty || amount.unit !== saved.unit;
  return (
    <form className="flex w-full flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); onSave(amount.qty === "" ? null : Number(amount.qty), amount.unit); }}>
      <label className={LABEL}>Qty<input type="number" min="0" step="any" aria-label={`Quantity of ${line.name}`} value={amount.qty} onChange={(e) => setAmount({ ...amount, qty: e.target.value })} className={`${INPUT} w-24`} /></label>
      <label className={LABEL}>Unit<input maxLength={20} placeholder="kg" aria-label={`Unit of ${line.name}`} value={amount.unit} onChange={(e) => setAmount({ ...amount, unit: e.target.value })} className={`${INPUT} w-24`} /></label>
      {changed && <button type="submit" disabled={busy} className={BUTTON}>Save</button>}
    </form>
  );
}

/** The shopping list: tick an item once it is bought. */
export function ToBuyPage() {
  const { data, isError } = useQuery({ queryKey: ["pantry"], queryFn: householdApi.pantry });
  const { data: suggestions } = useQuery({ queryKey: ["to-buy-suggestions"], queryFn: householdApi.toBuySuggestions });
  const { data: known } = useQuery({ queryKey: ["known-items"], queryFn: householdApi.knownItems });
  const { data: lastMonth } = useQuery({ queryKey: ["last-month-groceries"], queryFn: householdApi.lastMonthGroceries });
  const [line, setLine] = useState("");
  const add = useSaveAction(() => householdApi.addShopping(line), () => setLine(""));
  const change = useSaveAction((run: () => Promise<unknown>) => run());
  const error = add.error || change.error;
  if (isError) return <p role="alert" className="text-warn">This page could not be loaded.</p>;
  if (!data) return <p role="status" className="text-muted">Loading…</p>;
  const toBuy = data.shopping.filter((s) => !s.done);
  const bought = data.shopping.filter((s) => s.done);
  const row = (s: Pantry["shopping"][number]) => (
    <div key={s.id} className="flex flex-wrap items-center justify-between gap-2 border-b border-line-soft pb-3">
      <label className="flex min-h-11 min-w-0 flex-[1_1_180px] items-center gap-2.5 text-sm">
        <input type="checkbox" checked={s.done} disabled={change.busy} onChange={(e) => { const done = e.target.checked; change.run(() => householdApi.setShoppingDone(s.id, done)); }} className="size-5 flex-none" />
        <span className="flex min-w-0 flex-col">
          <span className={s.done ? "text-muted line-through" : "font-semibold"}>{s.name}</span>
          {s.done && s.qty != null && <span className="font-mono text-xs text-muted">{qtyLabel(s.qty, s.unit)}</span>}
        </span>
      </label>
      <span className="flex flex-none gap-1.5">
        <button type="button" aria-label={s.done ? `Buy ${s.name} again` : `Mark ${s.name} as bought`} disabled={change.busy} onClick={() => change.run(() => householdApi.setShoppingDone(s.id, !s.done))} className={s.done ? BUTTON : PRIMARY}>{s.done ? "Buy again" : "Bought"}</button>
        <button type="button" aria-label={`Remove ${s.name} from the list`} disabled={change.busy} onClick={() => { if (window.confirm(`Remove ${s.name} from the list?`)) change.run(() => householdApi.removeShopping(s.id)); }} className={BUTTON}>Remove</button>
      </span>
      {!s.done && <AmountForm key={`${s.qty}|${s.unit}`} line={s} busy={change.busy} onSave={(qty, unit) => change.run(() => householdApi.setShoppingAmount(s.id, qty, unit))} />}
    </div>
  );
  return (
    <>
      <div className="flex flex-col gap-1">
        <h1 className="text-[32px] font-extrabold tracking-[-0.02em]">To buy</h1>
        <p className="text-[15px] text-muted">Tick an item when you have bought it. At the start of each month, last month&apos;s groceries are added here automatically. Items you mark as used up in the pantry appear here, and newly saved grocery bills tick off what they include.</p>
      </div>
      {error && <p role="alert" className="rounded-[10px] bg-warn-soft px-4 py-3 text-sm font-semibold text-warn">{error}</p>}
      <Card label="Suggestions from last month" className="gap-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div className="flex flex-col gap-0.5">
            <h2 className={HEADING}>Bought last month</h2>
            <p className="text-[13px] text-muted">Groceries from last month&apos;s bills that you haven&apos;t bought this month.</p>
          </div>
          {suggestions && suggestions.length > 1 && (
            <button type="button" disabled={change.busy} onClick={() => change.run(async () => { for (const s of suggestions) await householdApi.addShopping(s.nameSi ?? s.name); })} className={BUTTON}>Add all {suggestions.length}</button>
          )}
        </div>
        {lastMonth && <p className="flex flex-wrap justify-between gap-2 rounded-[10px] bg-soft px-4 py-3 text-sm font-semibold"><span>Groceries in {monthYear(`${lastMonth.month}-01`)}</span><span className="font-mono">{money2(lastMonth.total)}</span></p>}
        {suggestions?.length === 0 && <p className="text-[15px] text-muted">No suggestions right now.</p>}
        {suggestions?.map((s) => (
          <div key={`${s.nameSi ?? s.name}|${s.unit ?? ""}`} className="flex items-center justify-between gap-2 border-b border-line-soft pb-3 text-sm">
            <span className="flex min-w-0 flex-col">
              <strong lang={s.nameSi ? "si" : "en"}>{s.nameSi ?? s.name}</strong>
              <span className="text-xs text-muted">{s.nameSi && `${s.name} · `}Last month: <span className="font-mono">{qtyLabel(s.qty, s.unit)}</span></span>
            </span>
            <button type="button" aria-label={`Add ${s.name} to the list`} disabled={change.busy} onClick={() => change.run(() => householdApi.addShopping(s.nameSi ?? s.name))} className={BUTTON}>Add</button>
          </div>
        ))}
      </Card>
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card label="To buy list" className="gap-3">
          <h2 className={HEADING}>Still to buy ({toBuy.length})</h2>
          {toBuy.length === 0 && <p className="text-[15px] text-muted">Nothing to buy.</p>}
          {toBuy.map(row)}
          <form className="flex flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); add.run(undefined); }}>
            <ItemCombobox label="Add item" value={line} items={known ?? []} onChange={setLine} onPick={(name) => { setLine(""); change.run(() => householdApi.addShopping(name)); }} />
            <button type="submit" disabled={add.busy} className={PRIMARY}>Add</button>
          </form>
        </Card>
        <Card label="Bought" className="gap-3">
          <h2 className={HEADING}>Bought ({bought.length})</h2>
          {bought.length === 0 && <p className="text-[15px] text-muted">Items you tick move here. Untick one to buy it again.</p>}
          {bought.map(row)}
        </Card>
      </div>
    </>
  );
}
