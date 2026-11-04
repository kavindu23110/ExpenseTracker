import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { X } from "lucide-react";
import { useId, useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { SelectField } from "@/components/ui/Field";
import { api } from "@/lib/api";
import { money2, num2, qtyLabel, shortDate } from "@/lib/format";
import { BillEditForm } from "./BillEditForm";
import { ItemName } from "./ItemName";

const TH = "px-1 pt-1.5 pb-2 font-bold border-b border-line";
const TD = "px-1 py-[9px] border-b border-line-soft";

function Row({ label, value, strong }: { label: string; value: string; strong?: string }) {
  return <div className={`flex justify-between gap-3 ${strong ?? ""}`}><span className={strong ? "" : "text-muted"}>{label}</span><span className="font-mono">{value}</span></div>;
}

function Fact({ label, children, mono }: { label: string; children: React.ReactNode; mono?: boolean }) {
  return <div className="flex flex-col gap-0.5"><dt className="text-xs font-semibold text-muted">{label}</dt><dd className={mono ? "font-mono break-all" : ""}>{children}</dd></div>;
}

function CategoryPicker({ id, value, categories }: { id: number; value: string; categories: string[] }) {
  const fieldId = useId();
  const queryClient = useQueryClient();
  const change = useMutation({
    mutationFn: (category: string) => api.changeBillCategory(id, category),
    onSuccess: () => queryClient.invalidateQueries(),
  });
  return (
    <div className="col-span-2 flex flex-col gap-1">
      <SelectField id={fieldId} label="Category" value={change.isPending ? change.variables : value} onChange={(c) => change.mutate(c)} options={categories.map((c) => ({ value: c, label: c }))} />
      {change.isError && <p role="alert" className="text-sm font-semibold text-warn">{change.error.message}</p>}
    </div>
  );
}

/** The full contents of one bill. */
export function BillDetails({ id, categories, onClose }: { id: number; categories: string[]; onClose: () => void }) {
  const { data: b, isError } = useQuery({ queryKey: ["bill", id], queryFn: () => api.bill(id) });
  const [editing, setEditing] = useState(false);
  if (isError) return <Card label="Bill details"><p role="alert" className="text-sm text-warn">This bill could not be loaded.</p></Card>;
  if (!b) return <Card label="Bill details"><p role="status" className="text-sm text-muted">Loading bill…</p></Card>;
  if (editing) return <Card label="Edit bill" className="gap-4"><h2 className="text-xl font-extrabold">Edit bill</h2><BillEditForm bill={b} onDone={() => setEditing(false)} /></Card>;
  const isSi = b.lang === "si";
  const fileUrl = `/api/bills/${b.id}/file`;
  return (
    <Card label="Bill details" className="gap-[18px]">
      <div className="flex items-start justify-between gap-3">
        <div className="flex min-w-0 flex-col gap-1">
          <div className="flex flex-wrap items-center gap-2">
            <p className="text-xs font-bold tracking-[0.08em] text-muted uppercase">Bill details</p>
            {isSi && <Badge tone="si">Sinhala · translated</Badge>}
          </div>
          <h2 className="text-[22px] leading-tight font-extrabold">{b.merchant}</h2>
          {isSi && b.merchantSi && <p lang="si" className="text-base font-semibold">{b.merchantSi}</p>}
          <p className="text-sm text-muted">{b.address}</p>
        </div>
        <span className="flex flex-none gap-2">
          <button type="button" onClick={() => setEditing(true)} className="min-h-11 rounded-[10px] border border-field bg-white px-3.5 text-sm font-semibold">Edit</button>
          <button type="button" onClick={onClose} aria-label="Close bill details" className="flex size-11 flex-none items-center justify-center rounded-[10px] border border-line bg-white text-ink-2"><X size={18} aria-hidden /></button>
        </span>
      </div>

      <dl className="grid grid-cols-2 gap-x-4 gap-y-3 text-sm">
        <Fact label="Bill no." mono>{b.billNo ?? "—"}</Fact>
        <Fact label="Date & time">{shortDate(b.date)} {b.date.slice(0, 4)}{b.time ? `, ${b.time}` : ""}</Fact>
        <Fact label="Paid with">{b.payment ?? "—"}</Fact>
        <CategoryPicker id={b.id} value={b.category} categories={categories} />
      </dl>

      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-[13px]">
          <thead>
            <tr className="text-left text-[11px] tracking-[0.06em] text-muted uppercase">
              <th scope="col" className={`${TH} pl-0`}>Item</th>
              <th scope="col" className={`${TH} text-right`}>Qty</th>
              <th scope="col" className={`${TH} text-right`}>Price</th>
              <th scope="col" className={`${TH} pr-0 text-right`}>Amount</th>
            </tr>
          </thead>
          <tbody>
            {b.items.map((it, i) => {
              const discounted = it.markedPrice != null && it.markedPrice > it.price;
              return (
                <tr key={i}>
                  <td className={`${TD} pl-0`}><ItemName item={it} /></td>
                  <td className={`${TD} text-right whitespace-nowrap text-muted`}>{qtyLabel(it.qty, it.unit)}</td>
                  <td className={`${TD} text-right font-mono text-muted`}>
                    <div className="flex flex-col items-end gap-px">
                      {discounted && <span className="text-xs line-through" aria-label={`Marked price ${money2(it.markedPrice!)}`}>{num2(it.markedPrice!)}</span>}
                      <span className="whitespace-nowrap">{num2(it.price)}{it.unit && <span className="block text-right text-[11px]">per {it.unit}</span>}</span>
                    </div>
                  </td>
                  <td className={`${TD} pr-0 text-right font-mono whitespace-nowrap`}>{num2(it.amount)}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <div className="flex flex-col gap-2 text-sm">
        <Row label="Subtotal" value={money2(b.subtotal)} />
        {b.charges.map((c, i) => <Row key={i} label={c.label} value={money2(c.amount)} />)}
        <Row label="Total" value={money2(b.amount)} strong="mt-0.5 border-t-2 border-ink pt-2.5 text-[17px] font-extrabold" />
        {b.saved > 0 && <Row label="You saved vs marked prices" value={money2(b.saved)} strong="font-bold text-accent-ink" />}
        {b.paid != null && <><Row label="Cash given" value={money2(b.paid)} /><Row label="Change" value={money2(b.change ?? 0)} /></>}
      </div>

      <div className="flex flex-wrap items-center gap-2 text-[13px] text-muted">
        {b.scanned ? <><Badge tone="scan">FROM BILL</Badge><span className="break-all">Read from {b.fileName ?? "an uploaded bill"}</span></> : <span>Entered by hand</span>}
        {b.fileContentType?.startsWith("image/") && (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={fileUrl} alt="Photo of the original bill" className="block h-auto w-full max-w-[320px] rounded-lg border border-line" />
        )}
        {b.fileContentType === "application/pdf" && <a href={fileUrl} target="_blank" rel="noreferrer" className="font-semibold">Open the original PDF</a>}
      </div>
    </Card>
  );
}
