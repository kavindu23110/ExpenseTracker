import { useState } from "react";
import { Card } from "@/components/ui/Card";

const firstOfMonth = () => {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-01`;
};

/** Downloads every bill line in a date range as a spreadsheet file. */
export function ExportBills() {
  const [from, setFrom] = useState(firstOfMonth);
  const [to, setTo] = useState("");
  const query = new URLSearchParams();
  if (from) query.set("from", from);
  if (to) query.set("to", to);
  return (
    <Card label="Export" className="gap-3">
      <div className="flex flex-col gap-0.5">
        <h2 className="text-xl font-extrabold">Export bills</h2>
        <p className="text-[13px] text-muted">A CSV file with one row per item, which opens in Excel or Google Sheets. Leave a date blank for no limit.</p>
      </div>
      <div className="flex flex-wrap items-end gap-2">
        <label className="flex min-w-0 flex-col gap-1 text-[13px] font-bold text-ink-2">From<input type="date" value={from} onChange={(e) => setFrom(e.target.value)} className="min-h-11 rounded-[10px] border border-field bg-white px-3 text-sm" /></label>
        <label className="flex min-w-0 flex-col gap-1 text-[13px] font-bold text-ink-2">To<input type="date" value={to} onChange={(e) => setTo(e.target.value)} className="min-h-11 rounded-[10px] border border-field bg-white px-3 text-sm" /></label>
        <a href={`/api/bills/export?${query}`} download className="inline-flex min-h-11 items-center rounded-[10px] bg-accent px-4 text-sm font-bold text-white">Download CSV</a>
      </div>
    </Card>
  );
}
