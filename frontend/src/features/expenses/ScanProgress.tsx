import { Check, Circle } from "lucide-react";

/** The animated receipt and reading steps shown while a bill is read. */
export function ScanProgress({ fileName, step, lang }: { fileName: string; step: number; lang: string }) {
  const steps = [
    lang === "auto" ? "Detecting language" : `Language: ${lang === "si" ? "සිංහල (Sinhala)" : "English"}`,
    "Reading the text",
    "Finding shop, date and bill no.",
    "Reading line items and charges",
    "Choosing a category",
  ];
  return (
    <div className="flex flex-wrap items-center gap-6">
      <div aria-hidden className="relative flex h-[200px] w-40 flex-none flex-col gap-2 overflow-hidden rounded-lg border border-line bg-white p-4 shadow-[0_6px_18px_rgba(17,24,39,0.08)]">
        <div className="h-2 w-[70%] rounded bg-[#e5e7eb]" />
        <div className="h-1.5 w-1/2 rounded bg-line-soft" />
        <div className="mt-3 h-1.5 w-[90%] rounded bg-line-soft" />
        <div className="h-1.5 w-4/5 rounded bg-line-soft" />
        <div className="h-1.5 w-[85%] rounded bg-line-soft" />
        <div className="h-1.5 w-3/5 rounded bg-line-soft" />
        <div className="mt-auto h-2.5 w-[55%] self-end rounded bg-[#e5e7eb]" />
        <div className="scanline" />
      </div>
      <div className="flex flex-[1_1_260px] flex-col gap-3.5">
        <div className="flex flex-col gap-1">
          <h2 className="text-xl font-extrabold">Reading your bill…</h2>
          <p className="font-mono text-sm break-all text-muted">{fileName}</p>
        </div>
        <ol role="status" className="flex flex-col gap-2.5">
          {steps.map((label, i) => (
            <li key={label} className="flex items-center gap-2.5 text-[15px] font-semibold">
              {step > i ? <Check size={20} className="text-accent" aria-hidden /> : <Circle size={20} className="text-[#9ca3af]" aria-hidden />}
              {label}
            </li>
          ))}
        </ol>
      </div>
    </div>
  );
}
