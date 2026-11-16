import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { householdApi } from "./householdApi";
import { BUTTON, HEADING, INPUT, LABEL, PRIMARY } from "./ui";
import { useSaveAction } from "./useSaveAction";

function QtyEditor({ name, qty, unit, busy, onSave }: { name: string; qty: number; unit: string | null; busy: boolean; onSave: (qty: number) => void }) {
  const [value, setValue] = useState(String(qty));
  const changed = value.trim() !== "" && Number(value) !== qty;
  return (
    <form className="flex items-center gap-1.5" onSubmit={(e) => { e.preventDefault(); if (changed) onSave(Number(value)); }}>
      <input required type="number" min="0" step="any" aria-label={`Quantity of ${name}`} value={value} onChange={(e) => setValue(e.target.value)} className={`${INPUT} w-24 font-mono`} />
      {unit && <span className="font-mono text-sm text-muted">{unit}</span>}
      {changed && <button type="submit" disabled={busy} aria-label={`Save quantity of ${name}`} className={`${PRIMARY} basis-auto!`}>Save</button>}
    </form>
  );
}

type PantryRow = { id: number; name: string; expiry: string | null; location: string | null; lowAt: number | null };

function DetailsEditor({ item, busy, onSave }: { item: PantryRow; busy: boolean; onSave: (expiry: string, location: string, lowAt: number | null) => void }) {
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState({ expiry: item.expiry ?? "", location: item.location ?? "", lowAt: item.lowAt == null ? "" : String(item.lowAt) });
  if (!open) return <button type="button" aria-label={`Details for ${item.name}`} onClick={() => setOpen(true)} className={BUTTON}>Details</button>;
  return (
    <form className="flex w-full flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); onSave(form.expiry, form.location, form.lowAt === "" ? null : Number(form.lowAt)); setOpen(false); }}>
      <label className={LABEL}>Expires<input type="date" value={form.expiry} onChange={(e) => setForm({ ...form, expiry: e.target.value })} className={INPUT} /></label>
      <label className={LABEL}>Kept in<input list="pantry-places" maxLength={40} placeholder="Fridge" value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} className={`${INPUT} w-32`} /></label>
      <label className={LABEL}>Buy again at<input type="number" min="0" step="any" placeholder="1" value={form.lowAt} onChange={(e) => setForm({ ...form, lowAt: e.target.value })} className={`${INPUT} w-24`} /></label>
      <button type="submit" disabled={busy} className={PRIMARY}>Save</button>
      <button type="button" onClick={() => setOpen(false)} className={BUTTON}>Cancel</button>
    </form>
  );
}

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
        <p className="text-[15px] text-muted">Newly saved bills add items from the categories you choose below. When something is used up, it goes on your To buy page.</p>
      </div>
      {error && <p role="alert" className="rounded-[10px] bg-warn-soft px-4 py-3 text-sm font-semibold text-warn">{error}</p>}
      <div className="grid gap-6">
        <Card label="Pantry categories" className="gap-3">
          <h2 className={HEADING}>Categories that fill the pantry</h2>
          <p className="text-sm text-muted">Items in these categories go into the pantry when you save a bill. An item&apos;s own category counts first, then the bill&apos;s.</p>
          <div className="flex flex-wrap gap-2">
            {data.categories.map((c) => {
              const on = data.pantryCategories.includes(c);
              return (
                <button key={c} type="button" aria-pressed={on} disabled={change.busy}
                  onClick={() => change.run(() => householdApi.setPantryCategories(on ? data.pantryCategories.filter((x) => x !== c) : [...data.pantryCategories, c]))}
                  className={`min-h-11 rounded-full px-3.5 text-sm ${on ? "border-2 border-accent bg-accent font-bold text-white" : "border border-field bg-white font-semibold text-ink"}`}>{c}</button>
              );
            })}
          </div>
        </Card>
        <Card label="Pantry" className="gap-3">
          <h2 className={HEADING}>At home</h2>
          <datalist id="pantry-places"><option value="Pantry" /><option value="Fridge" /><option value="Freezer" /></datalist>
          {data.pantry.length === 0 && <p className="text-[15px] text-muted">Nothing yet. Save a grocery bill or add an item.</p>}
          {data.pantry.map((p) => (
            <div key={p.id} className="flex flex-wrap items-center justify-between gap-2 border-b border-line-soft pb-3">
              <span className="flex min-w-0 flex-wrap items-center gap-2 text-sm"><strong>{p.name}</strong>{p.isOut && <Badge tone="bad">Out</Badge>}{p.isLow && <Badge tone="bad">Low</Badge>}{p.expiresSoon && <Badge tone="bad">Expires {p.expiry}</Badge>}{!p.expiresSoon && p.expiry && !p.isOut && <span className="text-muted">Expires {p.expiry}</span>}{p.location && <Badge tone="neutral">{p.location}</Badge>}</span>
              <QtyEditor key={p.qty} name={p.name} qty={p.qty} unit={p.unit} busy={change.busy} onSave={(qty) => change.run(() => householdApi.setPantryQty(p.id, qty))} />
              <span className="flex flex-wrap gap-1.5">
                <button type="button" aria-label={`One more ${p.name}`} disabled={change.busy} onClick={() => change.run(() => householdApi.adjustPantry(p.id, 1))} className={BUTTON}>+1</button>
                <button type="button" aria-label={`One less ${p.name}`} disabled={change.busy || p.isOut} onClick={() => change.run(() => householdApi.adjustPantry(p.id, -1))} className={BUTTON}>−1</button>
                <button type="button" aria-label={`Used up ${p.name}`} disabled={change.busy || p.isOut} onClick={() => change.run(() => householdApi.useUpPantry(p.id))} className={BUTTON}>Used up</button>
                <DetailsEditor item={p} busy={change.busy} onSave={(expiry, location, lowAt) => change.run(() => householdApi.setPantryDetails(p.id, expiry, location, lowAt))} />
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
