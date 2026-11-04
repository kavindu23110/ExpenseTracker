import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { qtyLabel } from "@/lib/format";
import { householdApi } from "./householdApi";
import { BUTTON, HEADING, INPUT, LABEL, PRIMARY } from "./ui";
import { useSaveAction } from "./useSaveAction";

/** What is at home; grocery bills fill the pantry automatically. */
export function PantryPage() {
  const { data, isError } = useQuery({ queryKey: ["pantry"], queryFn: householdApi.pantry });
  const [item, setItem] = useState({ name: "", qty: "1", unit: "" });
  const addItem = useSaveAction(() => householdApi.addPantry(item.name, Number(item.qty), item.unit), () => setItem({ name: "", qty: "1", unit: "" }));
  const change = useSaveAction((run: () => Promise<unknown>) => run());
  const error = addItem.error || change.error;
  if (isError) return <p role="alert" className="text-warn">This page could not be loaded.</p>;
  if (!data) return <p role="status" className="text-muted">Loading…</p>;
  return (
    <>
      <div className="flex flex-col gap-1">
        <h1 className="text-[32px] font-extrabold tracking-[-0.02em]">Pantry</h1>
        <p className="text-[15px] text-muted">Newly saved grocery bills add their items here. When something is used up, it goes on your To buy page.</p>
      </div>
      {error && <p role="alert" className="rounded-[10px] bg-warn-soft px-4 py-3 text-sm font-semibold text-warn">{error}</p>}
      <div className="grid gap-6">
        <Card label="Pantry" className="gap-3">
          <h2 className={HEADING}>At home</h2>
          {data.pantry.length === 0 && <p className="text-[15px] text-muted">Nothing yet. Save a grocery bill or add an item.</p>}
          {data.pantry.map((p) => (
            <div key={p.id} className="flex flex-wrap items-center justify-between gap-2 border-b border-line-soft pb-3">
              <span className="flex min-w-0 flex-wrap items-center gap-2 text-sm"><strong>{p.name}</strong><span className="font-mono text-muted">{qtyLabel(p.qty, p.unit)}</span>{p.isOut && <Badge tone="bad">Out</Badge>}</span>
              <span className="flex flex-wrap gap-1.5">
                <button type="button" aria-label={`One more ${p.name}`} disabled={change.busy} onClick={() => change.run(() => householdApi.adjustPantry(p.id, 1))} className={BUTTON}>+1</button>
                <button type="button" aria-label={`One less ${p.name}`} disabled={change.busy || p.isOut} onClick={() => change.run(() => householdApi.adjustPantry(p.id, -1))} className={BUTTON}>−1</button>
                <button type="button" aria-label={`Used up ${p.name}`} disabled={change.busy || p.isOut} onClick={() => change.run(() => householdApi.useUpPantry(p.id))} className={BUTTON}>Used up</button>
                <button type="button" aria-label={`Delete ${p.name}`} disabled={change.busy} onClick={() => { if (window.confirm(`Delete ${p.name} from the pantry?`)) change.run(() => householdApi.removePantry(p.id)); }} className={BUTTON}>Delete</button>
              </span>
            </div>
          ))}
          <form className="flex flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); addItem.run(undefined); }}>
            <label className={LABEL}>Item<input required value={item.name} onChange={(e) => setItem({ ...item, name: e.target.value })} className={INPUT} /></label>
            <label className={LABEL}>Qty<input required type="number" min="0" step="any" value={item.qty} onChange={(e) => setItem({ ...item, qty: e.target.value })} className={`${INPUT} w-20`} /></label>
            <label className={LABEL}>Unit<input value={item.unit} placeholder="kg" onChange={(e) => setItem({ ...item, unit: e.target.value })} className={`${INPUT} w-20`} /></label>
            <button type="submit" disabled={addItem.busy} className={PRIMARY}>Add item</button>
          </form>
        </Card>
      </div>
    </>
  );
}
