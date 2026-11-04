"use client";

import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { Upload } from "lucide-react";
import { useState } from "react";
import { api } from "@/lib/api";
import { monthYear } from "@/lib/format";
import { NO_FILTERS, type Filters } from "@/lib/types";
import { CategoryBreakdown } from "./CategoryBreakdown";
import { FilterBar } from "./FilterBar";
import { ItemTotals } from "./ItemTotals";
import { PriceWatch } from "./PriceWatch";
import { RecentBills } from "./RecentBills";
import { ScanPanel } from "./ScanPanel";
import { SummaryCards } from "./SummaryCards";
import { useBillScan } from "./useBillScan";
import { useDebounced } from "./useDebounced";

/** The whole dashboard: composes filters, scan, bills and price watch. */
export function ExpensesPage() {
  const queryClient = useQueryClient();
  const [filters, setFilters] = useState<Filters>(NO_FILTERS);
  const [openId, setOpenId] = useState<number | null>(null);
  const [lang, setLang] = useState("auto");
  const [flash, setFlash] = useState("");
  const scan = useBillScan();
  const debounced = useDebounced(filters);

  const { data, isError, refetch } = useQuery({ queryKey: ["dashboard", debounced], queryFn: () => api.dashboard(debounced), placeholderData: keepPreviousData });

  const onSaved = (id: number, message: string) => {
    scan.reset();
    setFlash(message);
    setOpenId(id);
    setFilters((f) => ({ ...f, category: "" }));
    queryClient.invalidateQueries({ queryKey: ["dashboard"] });
  };

  const pickFile = (file: File) => {
    setFlash("");
    scan.scanFile(file, lang);
  };

  if (isError && !data) {
    return (
      <main className="mx-auto flex max-w-xl flex-col items-start gap-3 p-8">
        <h1 className="text-2xl font-extrabold">The expense service isn&apos;t responding</h1>
        <p className="text-muted">Check that the backend is running, then try again.</p>
        <button type="button" onClick={() => refetch()} className="min-h-11 rounded-[10px] border border-field bg-white px-4 font-semibold">Try again</button>
      </main>
    );
  }
  if (!data) return <main className="p-8 text-muted" role="status">Loading your bills…</main>;

  return (
    <main className="min-h-screen px-4 pt-5 pb-28 sm:px-6 sm:pt-8 sm:pb-16">
      <div className="mx-auto flex max-w-[1280px] flex-col gap-6">
        <header className="flex flex-wrap items-center justify-between gap-4">
          <div className="flex flex-col gap-1">
            <p className="text-[13px] font-semibold tracking-[0.08em] text-muted uppercase">{monthYear(data.today)}</p>
            <h1 className="text-[32px] font-extrabold tracking-[-0.02em]">Expenses</h1>
          </div>
          <label data-file className="inline-flex min-h-12 w-full items-center justify-center gap-2.5 sm:w-auto rounded-xl bg-accent px-[22px] text-[15px] font-bold text-white">
            <Upload size={20} aria-hidden />
            Upload bill
            <input className="sr-only" type="file" accept="image/jpeg,image/png,image/webp,application/pdf" onChange={(e) => { const f = e.target.files?.[0]; e.target.value = ""; if (f) pickFile(f); }} />
          </label>
        </header>

        <FilterBar filters={filters} onChange={setFilters} data={data} />
        <SummaryCards data={data} />

        <div className="flex flex-wrap items-start gap-6">
          <div className="flex min-w-0 flex-[999_1_560px] flex-col gap-6">
            <ScanPanel scan={scan} lang={lang} onLang={setLang} flash={flash} categories={data.allCategories} readerAvailable={data.readerAvailable} onPick={pickFile} onSaved={onSaved} />
            <RecentBills data={data} category={filters.category} onCategory={(category) => setFilters({ ...filters, category })} openId={openId} onToggle={(id) => setOpenId(id === openId ? null : id)} />
            <ItemTotals data={data} />
          </div>
          <div className="flex min-w-0 flex-[1_1_340px] flex-col gap-6">
            <CategoryBreakdown data={data} />
          </div>
        </div>

        <PriceWatch data={data} />
      </div>
    </main>
  );
}
