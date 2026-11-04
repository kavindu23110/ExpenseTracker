import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { householdApi } from "./householdApi";
import type { SmsDraft } from "./types";
import { BUTTON, HEADING, INPUT, PRIMARY } from "./ui";

/** Finds payments in pasted bank messages and saves them once checked. */
export function SmsImportPage() {
  const queryClient = useQueryClient();
  const { data: planning } = useQuery({ queryKey: ["planning"], queryFn: householdApi.planning });
  const [text, setText] = useState("");
  const [drafts, setDrafts] = useState<SmsDraft[] | null>(null);
  const [message, setMessage] = useState("");
  const preview = useMutation({ mutationFn: () => householdApi.smsPreview(text), onSuccess: (found) => { setMessage(""); setDrafts(found); } });
  const save = useMutation({
    mutationFn: (items: SmsDraft[]) => householdApi.smsImport(items),
    onSuccess: ({ added }) => {
      setMessage(`Added ${added} ${added === 1 ? "payment" : "payments"} to your expenses.`);
      setDrafts(null);
      setText("");
      return queryClient.invalidateQueries();
    },
  });
  const edit = (i: number, change: Partial<SmsDraft>) => setDrafts((d) => d && d.map((x, n) => (n === i ? { ...x, ...change } : x)));
  const fresh = drafts?.filter((d) => !d.alreadyImported) ?? [];
  const error = preview.error?.message ?? save.error?.message;
  return (
    <>
      <h1 className="text-[32px] font-extrabold tracking-[-0.02em]">Bank SMS</h1>
      <Card label="Paste messages" className="gap-3">
        <div className="flex flex-col gap-0.5">
          <h2 className={HEADING}>Paste your bank messages</h2>
          <p className="text-[13px] text-muted">One message per line. Only the payment details are kept, not the message text. OTPs, deposits and declined payments are skipped. Check each result, as bank formats vary.</p>
        </div>
        <textarea aria-label="Bank messages" rows={6} value={text} onChange={(e) => setText(e.target.value)} className={`${INPUT} py-2`} />
        <div><button type="button" disabled={!text.trim() || preview.isPending} onClick={() => preview.mutate()} className={PRIMARY}>Find payments</button></div>
        {message && <p role="status" className="rounded-[10px] bg-accent-soft px-4 py-3 text-sm font-semibold text-accent-ink">{message}</p>}
        {error && <p role="alert" className="text-sm font-semibold text-warn">{error}</p>}
      </Card>
      {drafts && (
        <Card label="Check payments" className="gap-3">
          <h2 className={HEADING}>Check what we found</h2>
          {drafts.length === 0 && <p className="text-[15px] text-muted">No payments found. Your bank&apos;s format may not be recognised yet.</p>}
          {drafts.map((d, i) => (
            <div key={d.reference} className="flex flex-wrap items-center gap-2 border-b border-line-soft pb-3">
              <input aria-label="Shop" value={d.merchant} disabled={d.alreadyImported} onChange={(e) => edit(i, { merchant: e.target.value })} className={`${INPUT} flex-[1_1_180px]`} />
              <input aria-label="Amount" type="number" min="0.01" step="any" value={d.amount} disabled={d.alreadyImported} onChange={(e) => edit(i, { amount: Number(e.target.value) })} className={`${INPUT} w-28`} />
              <input aria-label="Date" type="date" value={d.date} disabled={d.alreadyImported} onChange={(e) => edit(i, { date: e.target.value })} className={INPUT} />
              <select aria-label="Category" value={d.category} disabled={d.alreadyImported} onChange={(e) => edit(i, { category: e.target.value })} className={INPUT}>{planning?.categories.map((c) => <option key={c}>{c}</option>)}</select>
              {d.alreadyImported ? <Badge tone="neutral">Already added</Badge> : <button type="button" onClick={() => setDrafts((all) => all && all.filter((_, n) => n !== i))} className={BUTTON}>Skip</button>}
            </div>
          ))}
          {fresh.length > 0 && <div><button type="button" disabled={save.isPending} onClick={() => save.mutate(fresh)} className={PRIMARY}>Add {fresh.length} to expenses</button></div>}
        </Card>
      )}
    </>
  );
}
