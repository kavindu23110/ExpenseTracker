import { useState } from "react";
import { Card } from "@/components/ui/Card";
import type { BillDraft } from "@/lib/types";
import { ManualBillForm } from "./ManualBillForm";
import { ScanIdle } from "./ScanIdle";
import { ScanProgress } from "./ScanProgress";
import { ScanReview } from "./ScanReview";
import type { useBillScan } from "./useBillScan";

/** Shows the current stage of the scan flow. */
export function ScanPanel({ scan, lang, onLang, flash, categories, readerAvailable, onPick, onSaved }: {
  scan: ReturnType<typeof useBillScan>;
  lang: string;
  onLang: (lang: string) => void;
  flash: string;
  categories: string[];
  readerAvailable: boolean;
  onPick: (file: File) => void;
  onSaved: (id: number, message: string) => void;
}) {
  const draft: BillDraft | null = scan.draft;
  const [manual, setManual] = useState(false);
  if (manual && scan.stage === "idle") {
    return <Card label="Enter a bill by hand"><ManualBillForm categories={categories} onCancel={() => setManual(false)} onSaved={(id, message) => { setManual(false); onSaved(id, message); }} /></Card>;
  }
  return (
    <Card label="Scan a bill" className="gap-5">
      {scan.stage === "idle" && <ScanIdle lang={lang} onLang={onLang} flash={flash} error={scan.error} readerAvailable={readerAvailable} onPick={onPick} onManual={() => setManual(true)} />}
      {scan.stage === "scanning" && <ScanProgress fileName={scan.fileName} step={scan.step} lang={lang} />}
      {scan.stage === "review" && draft && <ScanReview draft={draft} onChange={scan.setDraft} file={scan.file} previewUrl={scan.previewUrl} categories={categories} onDiscard={scan.reset} onSaved={onSaved} />}
    </Card>
  );
}
