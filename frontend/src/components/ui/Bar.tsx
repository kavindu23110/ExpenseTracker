/** A rounded horizontal bar. */
export function Bar({ percent, warn }: { percent: number; warn?: boolean }) {
  return (
    <div className="h-2.5 overflow-hidden rounded-full bg-line-soft">
      <div className={`h-2.5 rounded-full ${warn ? "bg-[#c2410c]" : "bg-accent"}`} style={{ width: `${percent}%` }} />
    </div>
  );
}
