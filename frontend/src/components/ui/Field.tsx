const CONTROL = "min-h-11 min-w-0 rounded-[10px] border border-field bg-white px-3 text-[15px] text-ink";

/** A labelled text input. */
export function TextField({ id, label, mono, className = "", ...props }: { id: string; label: string; mono?: boolean } & React.InputHTMLAttributes<HTMLInputElement>) {
  return (
    <div className={`flex min-w-0 flex-col gap-1.5 ${className}`}>
      <label htmlFor={id} className="text-[13px] font-bold text-ink-2">{label}</label>
      <input id={id} className={`${CONTROL} ${mono ? "font-mono" : ""}`} {...props} />
    </div>
  );
}

/** A labelled dropdown; every select on the page goes through this. */
export function SelectField({ id, label, value, onChange, options, className = "" }: {
  id: string;
  label: string;
  value: string;
  onChange: (value: string) => void;
  options: { value: string; label: string }[];
  className?: string;
}) {
  return (
    <div className={`flex min-w-0 flex-col gap-1.5 ${className}`}>
      <label htmlFor={id} className="text-[13px] font-bold text-ink-2">{label}</label>
      <select id={id} value={value} onChange={(e) => onChange(e.target.value)} className={CONTROL}>
        {options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
      </select>
    </div>
  );
}
