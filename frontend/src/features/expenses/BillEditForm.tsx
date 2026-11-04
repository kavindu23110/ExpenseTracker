import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Button } from "@/components/ui/Button";
import { TextField } from "@/components/ui/Field";
import { api } from "@/lib/api";
import { money2 } from "@/lib/format";
import type { BillDetail, BillItem } from "@/lib/types";

type Line = BillItem & { qtyText: string; priceText: string };

const NEW_LINE: Line = { name: "", nameSi: null, qty: 1, price: 0, unit: null, markedPrice: null, marketKey: null, amount: 0, qtyText: "1", priceText: "" };

/** Corrects a saved bill's shop, date, items and total; a hand-entered bill's total always follows its lines. */
export function BillEditForm({ bill, onDone }: { bill: BillDetail; onDone: () => void }) {
  const queryClient = useQueryClient();
  const [merchant, setMerchant] = useState(bill.merchant);
  const [date, setDate] = useState(bill.date);
  const [total, setTotal] = useState(String(bill.amount));
  const [lines, setLines] = useState<Line[]>(bill.items.map((i) => ({ ...i, qtyText: String(i.qty), priceText: String(i.price) })));
  const charges = bill.charges.reduce((sum, c) => sum + c.amount, 0);
  const computed = Math.round((lines.reduce((sum, l) => sum + (parseFloat(l.qtyText) || 0) * (parseFloat(l.priceText) || 0), 0) + charges) * 100) / 100;
  const setLine = (index: number, patch: Partial<Line>) => setLines(lines.map((l, i) => (i === index ? { ...l, ...patch } : l)));

  const save = useMutation({
    mutationFn: () => api.editBill(bill.id, {
      merchant,
      date,
      amount: bill.scanned ? parseFloat(total) : computed,
      items: lines.map((l) => ({ name: l.name, nameSi: l.nameSi, qty: parseFloat(l.qtyText), price: parseFloat(l.priceText), unit: l.unit?.trim() || null, markedPrice: l.markedPrice, marketKey: l.marketKey, amount: 0 })),
    }),
    onSuccess: async () => {
      await queryClient.invalidateQueries();
      onDone();
    },
  });

  return (
    <form className="flex flex-col gap-4" onSubmit={(e) => { e.preventDefault(); save.mutate(); }}>
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <TextField id={`e-shop-${bill.id}`} label="Shop" required value={merchant} onChange={(e) => setMerchant(e.target.value)} />
        <TextField id={`e-date-${bill.id}`} label="Date" type="date" required value={date} onChange={(e) => setDate(e.target.value)} />
      </div>
      {lines.map((l, i) => (
        <fieldset key={i} className="grid grid-cols-3 items-end gap-3 rounded-[10px] border border-line-soft bg-[#f8fafb] p-3">
          <legend className="sr-only">Line {i + 1}</legend>
          <div className="col-span-3 flex flex-col gap-1">
            <TextField id={`e-name-${bill.id}-${i}`} label="Item" required value={l.name} onChange={(e) => setLine(i, { name: e.target.value, nameSi: null, marketKey: null })} />
            {l.nameSi && <span lang="si" className="text-xs text-muted">On the bill: {l.nameSi}</span>}
          </div>
          <TextField id={`e-qty-${bill.id}-${i}`} label="Qty" type="number" min="0.001" step="any" required mono value={l.qtyText} onChange={(e) => setLine(i, { qtyText: e.target.value })} />
          <TextField id={`e-unit-${bill.id}-${i}`} label="Unit" value={l.unit ?? ""} onChange={(e) => setLine(i, { unit: e.target.value })} />
          <TextField id={`e-price-${bill.id}-${i}`} label="Price (Rs)" type="number" min="0" step="any" required mono value={l.priceText} onChange={(e) => setLine(i, { priceText: e.target.value })} />
          {lines.length > 1 && <button type="button" aria-label={`Remove line ${i + 1}`} onClick={() => setLines(lines.filter((_, n) => n !== i))} className="col-span-3 min-h-11 rounded-[10px] border border-field bg-white px-3.5 text-sm font-semibold">Remove item</button>}
        </fieldset>
      ))}
      <button type="button" onClick={() => setLines([...lines, NEW_LINE])} className="min-h-11 self-start rounded-[10px] border border-field bg-white px-4 text-sm font-semibold">Add another item</button>
      {!bill.scanned && <p className="flex justify-between gap-3 border-t border-line pt-3 text-lg font-extrabold">Total <span className="font-mono">{money2(computed)}</span></p>}
      {bill.scanned && <TextField id={`e-total-${bill.id}`} label="Total (Rs)" type="number" min="0" step="any" required mono value={total} onChange={(e) => setTotal(e.target.value)} />}
      {bill.scanned && computed !== parseFloat(total) && (
        <div className="flex flex-wrap items-center justify-between gap-2 rounded-[10px] bg-warn-soft px-4 py-2 text-sm font-semibold text-warn">
          <span>The items{bill.charges.length > 0 ? " and charges" : ""} add up to {money2(computed)}.</span>
          <button type="button" onClick={() => setTotal(String(computed))} className="min-h-11 rounded-[10px] border border-field bg-white px-3.5 text-ink">Use this total</button>
        </div>
      )}
      {save.isError && <p role="alert" className="rounded-[10px] bg-warn-soft px-4 py-3 text-sm font-semibold text-warn">{save.error.message}</p>}
      <div className="flex flex-wrap gap-3">
        <Button type="submit" disabled={save.isPending}>{save.isPending ? "Saving…" : "Save changes"}</Button>
        <Button variant="secondary" onClick={onDone} disabled={save.isPending}>Cancel</Button>
      </div>
    </form>
  );
}
