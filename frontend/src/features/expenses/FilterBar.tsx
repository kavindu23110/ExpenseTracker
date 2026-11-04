import { useState } from "react";
import { SelectField, TextField } from "@/components/ui/Field";
import { NO_FILTERS, type Dashboard, type Filters } from "@/lib/types";

/** Describes the active filters for the status line. */
function describe(f: Filters, data: Dashboard) {
  const active: string[] = [];
  if (f.q.trim()) active.push(`“${f.q.trim()}”`);
  if (f.period !== "all") active.push(data.periods.find((p) => p.value === f.period)?.label ?? f.period);
  if (f.shop) active.push(f.shop);
  if (f.lang) active.push(f.lang === "si" ? "Sinhala bills" : "English bills");
  if (f.source) active.push(f.source === "scanned" ? "read from bill" : "entered by hand");
  if (f.category) active.push(f.category);
  return active;
}

/** Search and filter controls with the result count. */
export function FilterBar({ filters, onChange, data }: { filters: Filters; onChange: (f: Filters) => void; data: Dashboard }) {
  const set = (patch: Partial<Filters>) => onChange({ ...filters, ...patch });
  const active = describe(filters, data);
  const [more, setMore] = useState(false);
  return (
    <section aria-label="Filters" className="flex flex-col gap-3 rounded-2xl border border-line bg-white p-4 sm:px-5">
      <div className="flex flex-wrap items-end gap-3">
        <TextField id="flt-q" label="Search" type="search" placeholder="Shop, item or bill no. — English or සිංහල" value={filters.q} onChange={(e) => set({ q: e.target.value })} className="flex-[2_1_260px]" />
        <button type="button" aria-expanded={more} onClick={() => setMore(!more)} className="min-h-11 w-full rounded-[10px] border border-field bg-white px-3.5 text-sm font-semibold sm:hidden">{more ? "Hide filters" : "More filters"}</button>
        <div className={more ? "contents" : "hidden sm:contents"}>
        <SelectField id="flt-period" label="Period" value={filters.period} onChange={(period) => set({ period })} options={data.periods} className="flex-[1_1_150px]" />
        <SelectField id="flt-shop" label="Shop" value={filters.shop} onChange={(shop) => set({ shop })} options={[{ value: "", label: "All shops" }, ...data.shops.map((s) => ({ value: s, label: s }))]} className="flex-[1_1_190px]" />
        <SelectField id="flt-lang" label="Bill language" value={filters.lang} onChange={(lang) => set({ lang })} options={[{ value: "", label: "All" }, { value: "en", label: "English" }, { value: "si", label: "සිංහල" }]} className="flex-[1_1_140px]" />
        <SelectField id="flt-src" label="Source" value={filters.source} onChange={(source) => set({ source })} options={[{ value: "", label: "All" }, { value: "scanned", label: "Read from bill" }, { value: "manual", label: "Entered by hand" }]} className="flex-[1_1_160px]" />
        </div>
      </div>
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p role="status" className="text-sm text-ink-2">
          Showing <strong>{data.bills.length}</strong> of {data.allCount} bills
          {active.length > 0 && <span className="text-muted"> · {active.join(" · ")}</span>}
        </p>
        {active.length > 0 && <button type="button" onClick={() => onChange(NO_FILTERS)} className="min-h-11 rounded-[10px] border border-field bg-white px-3.5 text-sm font-semibold">Clear filters</button>}
      </div>
    </section>
  );
}
