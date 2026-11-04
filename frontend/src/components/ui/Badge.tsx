const TONES = {
  si: "bg-warn-soft text-si",
  scan: "bg-accent-soft text-accent-ink tracking-[0.04em]",
  bad: "bg-warn-soft text-warn",
  good: "bg-accent-soft text-accent-ink",
  neutral: "bg-soft text-ink-2",
};

/** A small coloured label. */
export function Badge({ tone, lang, children, large }: { tone: keyof typeof TONES; lang?: string; large?: boolean; children: React.ReactNode }) {
  return <span lang={lang} className={`inline-block rounded-md font-bold ${large ? "px-2 py-[3px] text-[13px]" : "px-1.5 py-0.5 text-[11px]"} ${TONES[tone]}`}>{children}</span>;
}
