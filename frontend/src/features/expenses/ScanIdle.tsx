import { FileText } from "lucide-react";
import { useState } from "react";

const LANGS = [
  { id: "auto", name: "Auto-detect", code: "en", hint: "English or Sinhala, detected automatically" },
  { id: "en", name: "English", code: "en", hint: "reading as English" },
  { id: "si", name: "සිංහල", code: "si", hint: "reading as Sinhala" },
];

/** The upload area and language choice. */
export function ScanIdle({ lang, onLang, flash, error, readerAvailable, onPick, onManual }: {
  lang: string;
  onLang: (lang: string) => void;
  flash: string;
  error: string;
  readerAvailable: boolean;
  onPick: (file: File) => void;
  onManual: () => void;
}) {
  const [over, setOver] = useState(false);
  const current = LANGS.find((l) => l.id === lang) ?? LANGS[0];
  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="flex flex-[1_1_320px] flex-col gap-1">
          <h2 className="text-xl font-extrabold">Scan a bill</h2>
          <p className="text-[15px] leading-normal text-muted">Upload a photo or PDF of a receipt in English or Sinhala. We read every line item, the charges and the total, then suggest a category you can change.</p>
        </div>
        <div className="flex flex-col gap-1.5">
          <span id="lang-label" className="text-[13px] font-bold text-ink-2">Bill language</span>
          <div role="group" aria-labelledby="lang-label" className="flex gap-1 rounded-xl bg-soft p-1">
            {LANGS.map((l) => (
              <button key={l.id} type="button" lang={l.code} aria-pressed={lang === l.id} onClick={() => onLang(l.id)} className={`min-h-10 rounded-[9px] px-3.5 text-sm ${lang === l.id ? "bg-white font-bold text-ink shadow-[0_1px_3px_rgba(17,24,39,0.12)]" : "font-semibold text-ink-2"}`}>{l.name}</button>
            ))}
          </div>
        </div>
      </div>

      {flash && <p role="status" className="rounded-[10px] bg-accent-soft px-4 py-3 text-sm font-semibold text-accent-ink">{flash}</p>}
      {error && <p role="alert" className="rounded-[10px] bg-warn-soft px-4 py-3 text-sm font-semibold text-warn">{error}</p>}
      {!readerAvailable && !error && <p className="rounded-[10px] bg-soft px-4 py-3 text-sm text-ink-2">Reading uploaded bills needs Claude Code signed in on this machine, or a Claude API key on the backend.</p>}

      <label
        data-file
        onDragOver={(e) => { e.preventDefault(); setOver(true); }}
        onDragLeave={() => setOver(false)}
        onDrop={(e) => { e.preventDefault(); setOver(false); const f = e.dataTransfer.files[0]; if (f) onPick(f); }}
        className={`flex min-h-40 flex-col items-center justify-center gap-2.5 rounded-[14px] border-2 border-dashed p-6 text-center ${over ? "border-accent bg-accent-soft" : "border-[#b8c2cc] bg-[#f8fafb]"}`}
      >
        <FileText size={36} strokeWidth={1.8} className="text-accent" aria-hidden />
        <span className="text-base font-bold">Drop a bill here or choose a file</span>
        <span className="text-[13px] text-muted">JPG, PNG, WEBP or PDF · {current.hint}</span>
        <input className="sr-only" type="file" accept="image/jpeg,image/png,image/webp,application/pdf" onChange={(e) => { const f = e.target.files?.[0]; e.target.value = ""; if (f) onPick(f); }} />
      </label>
      <button type="button" onClick={onManual} className="min-h-12 rounded-xl border border-field bg-white px-5 text-[15px] font-semibold text-ink">Enter a bill by hand</button>
    </div>
  );
}
