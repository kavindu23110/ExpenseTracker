import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { Button } from "@/components/ui/Button";
import { SelectField, TextField } from "@/components/ui/Field";
import { api } from "@/lib/api";
import { money, money2 } from "@/lib/format";

const EMPTY_LINE = { name: "", qty: "1", unit: "", price: "" };

const localToday = () => {
  const now = new Date();
  return new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
};

const lineTotal = (l: typeof EMPTY_LINE) => (parseFloat(l.qty) || 0) * (parseFloat(l.price) || 0);

/** Files a bill typed in by hand; the total follows the lines unless the user enters their own. */
export function ManualBillForm({ categories, onCancel, onSaved }: {
  categories: string[];
  onCancel: () => void;
  onSaved: (id: number, message: string) => void;
}) {
  const [merchant, setMerchant] = useState("");
  const [date, setDate] = useState(localToday);
  const [category, setCategory] = useState(categories[0] ?? "Other");
  const [lines, setLines] = useState([EMPTY_LINE]);
  const [total, setTotal] = useState("");
  const computed = lines.reduce((sum, l) => sum + lineTotal(l), 0);
  const amount = total.trim() ? parseFloat(total) || 0 : computed;
  const setLine = (index: number, patch: Partial<typeof EMPTY_LINE>) => setLines(lines.map((l, i) => (i === index ? { ...l, ...patch } : l)));

  const save = useMutation({
    mutationFn: () => {
      const items = lines.filter((l) => l.name.trim()).map((l) => ({ name: l.name.trim(), qty: parseFloat(l.qty), price: parseFloat(l.price), unit: l.unit.trim() || null }));
      return api.saveBill({ lang: "en", merchant, date, amount, category, suggestedCategory: category, manual: true, items, charges: [] }, null);
    },
    onSuccess: ({ id }) => onSaved(id, `Saved ${money(amount)} at ${merchant} under ${category}.`),
  });

  return (
    <form className="flex flex-col gap-4" onSubmit={(e) => { e.preventDefault(); save.mutate(); }}>
      <div className="flex flex-col gap-1">
        <h2 className="text-xl font-extrabold">Enter a bill by hand</h2>
        <p className="text-[15px] text-muted">Add a line for each item. For a bill with no items, like electricity, use one line.</p>
      </div>
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
        <TextField id="m-shop" label="Shop" required value={merchant} onChange={(e) => setMerchant(e.target.value)} />
        <TextField id="m-date" label="Date" type="date" required value={date} onChange={(e) => setDate(e.target.value)} />
        <SelectField id="m-category" label="Category" value={category} onChange={setCategory} options={categories.map((c) => ({ value: c, label: c }))} />
      </div>

      {lines.map((l, i) => (
        <fieldset key={i} className="grid grid-cols-3 items-end gap-3 rounded-[10px] border border-line-soft bg-[#f8fafb] p-3 sm:grid-cols-[1fr_90px_90px_130px_auto]">
          <legend className="sr-only">Line {i + 1}</legend>
          <TextField id={`m-name-${i}`} label="Item" required value={l.name} onChange={(e) => setLine(i, { name: e.target.value })} className="col-span-3 sm:col-span-1" />
          <TextField id={`m-qty-${i}`} label="Qty" type="number" min="0.001" step="any" required mono value={l.qty} onChange={(e) => setLine(i, { qty: e.target.value })} />
          <TextField id={`m-unit-${i}`} label="Unit" placeholder="kg" value={l.unit} onChange={(e) => setLine(i, { unit: e.target.value })} />
          <TextField id={`m-price-${i}`} label="Price (Rs)" type="number" min="0" step="any" required mono value={l.price} onChange={(e) => setLine(i, { price: e.target.value })} />
          {lines.length > 1 && <button type="button" aria-label={`Remove line ${i + 1}`} onClick={() => setLines(lines.filter((_, n) => n !== i))} className="col-span-3 min-h-11 rounded-[10px] border border-field bg-white px-3.5 text-sm font-semibold sm:col-span-1">Remove</button>}
        </fieldset>
      ))}
      <button type="button" onClick={() => setLines([...lines, EMPTY_LINE])} className="min-h-11 self-start rounded-[10px] border border-field bg-white px-4 text-sm font-semibold">Add another item</button>

      <div className="flex flex-wrap items-end justify-between gap-3 border-t border-line pt-3">
        <TextField id="m-total" label="Total on the bill (optional)" type="number" min="0" step="any" mono placeholder={computed.toFixed(2)} value={total} onChange={(e) => setTotal(e.target.value)} />
        <p className="text-lg font-extrabold">Total <span className="font-mono">{money2(amount)}</span></p>
      </div>
      {save.isError && <p role="alert" className="rounded-[10px] bg-warn-soft px-4 py-3 text-sm font-semibold text-warn">{save.error.message}</p>}
      <div className="flex flex-wrap gap-3">
        <Button type="submit" disabled={save.isPending}>{save.isPending ? "Saving…" : "Save bill"}</Button>
        <Button variant="secondary" onClick={onCancel} disabled={save.isPending}>Cancel</Button>
      </div>
    </form>
  );
}
