import type { BillDraft } from "@/lib/types";

const fmt = (n: number) => n.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** A receipt-style view of the fields read, shown when there is no image to display. */
export function ReceiptPreview({ draft }: { draft: BillDraft }) {
  const si = draft.lang === "si";
  const dateText = draft.date ? draft.date.split("-").reverse().join("/") : "";
  const lines = [
    ...draft.items.map((i) => ({ label: `${(si && i.nameSi) || i.name}${i.qty !== 1 || i.unit ? ` ${i.qty}${i.unit ? ` ${i.unit}` : ""}` : ""}`, amount: i.amount })),
    ...draft.charges.map((c) => ({ label: (si && c.labelSi) || c.label, amount: c.amount })),
  ];
  return (
    <div aria-label="Text read from the bill" lang={draft.lang} className="flex max-w-[340px] flex-[1_1_260px] flex-col gap-1.5 rounded-md border border-[#e5e1d8] bg-[#fffefb] p-5 font-mono text-[13px] text-[#1f2937] shadow-[0_6px_18px_rgba(17,24,39,0.08)]">
      <p className="rounded bg-[#fef3c7] px-1 py-0.5 text-center text-sm font-semibold">{si ? draft.merchantSi ?? draft.merchant : draft.merchant.toUpperCase()}</p>
      <p className="text-center text-muted">{si ? draft.addressSi ?? draft.address : draft.address}</p>
      <p className="text-center text-muted">{si ? "බිල් අංකය" : "Bill"} {draft.billNo}</p>
      <p className="mb-2 rounded bg-[#fef3c7] px-1 py-0.5 text-center">{si ? "දිනය " : ""}{dateText} {draft.time}</p>
      {lines.map((l, i) => (
        <div key={i} className="flex justify-between gap-3 rounded bg-[#fef3c7] px-1 py-px"><span>{l.label}</span><span>{fmt(l.amount)}</span></div>
      ))}
      <div className="mt-2 flex justify-between rounded border-t border-dashed border-[#9ca3af] bg-[#fef3c7] px-1 pt-2 font-semibold"><span>{si ? "මුළු එකතුව" : "TOTAL"}</span><span>{fmt(draft.amount)}</span></div>
      <p className="text-muted">{si ? `ගෙවීම: ${draft.paymentSi ?? draft.payment ?? ""}` : `Paid: ${draft.payment ?? ""}`}</p>
      <p lang="en" className="mt-2 font-sans text-xs text-muted">Highlighted: fields we picked up</p>
    </div>
  );
}
