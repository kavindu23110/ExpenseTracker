import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { TextField } from "@/components/ui/Field";
import { api } from "@/lib/api";
import { money, money2, qtyLabel } from "@/lib/format";
import type { BillDraft } from "@/lib/types";
import { ItemName } from "./ItemName";
import { ReceiptPreview } from "./ReceiptPreview";

/** Lets the user check and correct what was read, then save it. */
export function ScanReview({ draft, onChange, file, previewUrl, categories, onDiscard, onSaved }: {
  draft: BillDraft;
  onChange: (d: BillDraft) => void;
  file: File | null;
  previewUrl: string | null;
  categories: string[];
  onDiscard: () => void;
  onSaved: (id: number, message: string) => void;
}) {
  const [amount, setAmount] = useState(draft.amount.toFixed(2));
  const [category, setCategory] = useState(draft.suggestion.category);
  const isSi = draft.lang === "si";
  const parsedAmount = parseFloat(amount.replace(/[^0-9.]/g, "")) || 0;
  const change = draft.paid != null ? draft.paid - parsedAmount : null;

  const save = useMutation({
    mutationFn: () => api.saveBill({ ...draft, amount: parsedAmount, category, suggestedCategory: draft.suggestion.category }, file),
    onSuccess: ({ id }) => onSaved(id, `Saved ${money(parsedAmount)} at ${draft.merchant} under ${category}, with ${draft.items.length} line items${isSi ? " translated from Sinhala." : "."}`),
  });

  return (
    <div className="flex flex-col gap-5">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap items-center gap-2.5">
          <h2 className="text-xl font-extrabold">Check what we found</h2>
          {isSi && <Badge tone="si" large>Sinhala bill · translated</Badge>}
        </div>
        <span className="font-mono text-[13px] break-all text-muted">{draft.fileName}</span>
      </div>

      <div className="flex flex-wrap items-start gap-6">
        {previewUrl ? (
          <figure className="flex max-w-[340px] flex-[1_1_260px] flex-col gap-2">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={previewUrl} alt={`The bill you uploaded from ${draft.merchant}`} className="block h-auto w-full rounded-lg border border-line shadow-[0_6px_18px_rgba(17,24,39,0.08)]" />
            <figcaption className="text-xs text-muted">Your photo · {draft.fileName}</figcaption>
          </figure>
        ) : <ReceiptPreview draft={draft} />}

        <div className="flex min-w-0 flex-[1_1_300px] flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <TextField id="f-merchant" label="Shop" type="text" value={draft.merchant} onChange={(e) => onChange({ ...draft, merchant: e.target.value })} />
            {isSi && draft.merchantSi && <span className="text-[13px] text-muted">On the bill: <span lang="si" className="font-semibold text-ink">{draft.merchantSi}</span></span>}
          </div>
          <div className="grid grid-cols-2 gap-3">
            <TextField id="f-date" label="Date" type="date" value={draft.date ?? ""} onChange={(e) => onChange({ ...draft, date: e.target.value || null })} />
            <TextField id="f-amount" label="Total (Rs)" type="text" inputMode="decimal" mono value={amount} onChange={(e) => setAmount(e.target.value)} />
          </div>

          <div className="flex flex-col gap-2.5 rounded-[10px] border border-line-soft bg-[#f8fafb] px-3.5 py-3">
            <span className="text-[13px] font-bold text-ink-2">Line items found: {draft.items.length}</span>
            {draft.items.map((it, i) => (
              <div key={i} className="flex justify-between gap-3 text-sm">
                <ItemName item={it} suffix={<span className="text-muted">× {qtyLabel(it.qty, it.unit)}</span>} />
                <span className="font-mono whitespace-nowrap">{money2(it.amount)}</span>
              </div>
            ))}
            <div className="flex flex-col gap-1 border-t border-line pt-2 text-[13px]">
              <div className="flex justify-between gap-3 font-bold"><span>Total on bill</span><span className="font-mono">{money2(draft.amount)}</span></div>
              {draft.totalMismatch && <p role="alert" className="font-semibold text-warn">The lines add up to {money2(draft.computedTotal)}. Check the items and total against your bill.</p>}
              {draft.saved > 0 && <div className="flex justify-between gap-3 font-semibold text-accent-ink"><span>Saved against marked prices</span><span className="font-mono">{money2(draft.saved)}</span></div>}
              {change != null && <div className="flex justify-between gap-3 text-muted"><span>Paid {money2(draft.paid!)} · change</span><span className="font-mono">{money2(change)}</span></div>}
            </div>
          </div>

          <div className="flex flex-col gap-2">
            <div className="flex flex-wrap items-baseline justify-between gap-2">
              <span id="cat-label" className="text-[13px] font-bold text-ink-2">Category</span>
              <span className="text-[13px] font-semibold text-accent-ink">Suggested {draft.suggestion.category} · {draft.suggestion.confidence}% sure</span>
            </div>
            <div role="group" aria-labelledby="cat-label" className="flex flex-wrap gap-2">
              {categories.map((c) => (
                <button key={c} type="button" aria-pressed={category === c} onClick={() => setCategory(c)} className={`min-h-11 rounded-full px-3.5 text-sm ${category === c ? "border-2 border-accent bg-accent font-bold text-white" : "border border-field bg-white font-semibold text-ink"}`}>{c}</button>
              ))}
            </div>
            <p className="text-[13px] text-muted">Why: {draft.suggestion.reason}</p>
          </div>

          {save.isError && <p role="alert" className="rounded-[10px] bg-warn-soft px-4 py-3 text-sm font-semibold text-warn">{save.error.message}</p>}
          <div className="mt-1 flex flex-wrap gap-3">
            <Button onClick={() => save.mutate()} disabled={save.isPending}>{save.isPending ? "Saving…" : "Save bill"}</Button>
            <Button variant="secondary" onClick={onDiscard} disabled={save.isPending}>Discard</Button>
          </div>
        </div>
      </div>
    </div>
  );
}
