import { ChevronDown } from "lucide-react";
import { useState } from "react";
import type { ToBuySuggestion } from "./types";
import { INPUT, LABEL } from "./ui";

/** A searchable dropdown of known items; text that matches nothing can still be added as a new item. */
export function ItemCombobox({ label, value, items, onChange, onPick }: {
  label: string;
  value: string;
  items: ToBuySuggestion[];
  onChange: (value: string) => void;
  onPick: (name: string) => void;
}) {
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(-1);
  const term = value.trim().toLowerCase();
  const matches = items.filter((k) => !term || k.name.toLowerCase().includes(term) || k.nameSi?.toLowerCase().includes(term));
  const pick = (k: ToBuySuggestion) => {
    setOpen(false);
    setActive(-1);
    onPick(k.nameSi ?? k.name);
  };
  const onKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Escape") setOpen(false);
    else if (e.key === "ArrowDown" || e.key === "ArrowUp") {
      e.preventDefault();
      setOpen(true);
      if (matches.length) setActive((a) => (a + (e.key === "ArrowDown" ? 1 : matches.length - 1 + (a < 0 ? 1 : 0))) % matches.length);
    } else if (e.key === "Enter" && open && matches[active]) {
      e.preventDefault();
      pick(matches[active]);
    }
  };
  return (
    <div className="relative min-w-0 flex-[1_1_220px]">
      <label className={LABEL}>
        {label}
        <span className="relative flex">
          <input required role="combobox" aria-expanded={open} aria-controls="item-options" aria-autocomplete="list" aria-activedescendant={open && active >= 0 ? `item-option-${active}` : undefined} autoComplete="off" placeholder="Search or type a new item" value={value} onFocus={() => setOpen(true)} onClick={() => setOpen(true)} onBlur={() => setOpen(false)} onKeyDown={onKeyDown} onChange={(e) => { onChange(e.target.value); setOpen(true); setActive(-1); }} className={`${INPUT} w-full pr-10`} />
          <ChevronDown size={18} aria-hidden className="pointer-events-none absolute top-1/2 right-3 -translate-y-1/2 text-muted" />
        </span>
      </label>
      {open && (
        <ul id="item-options" role="listbox" aria-label="Your items" className="absolute z-20 mt-1 max-h-64 w-full overflow-y-auto rounded-[10px] border border-line bg-white shadow-lg">
          {matches.length === 0 && <li className="px-3 py-2.5 text-sm text-muted">{term ? "No match. Press Add to add it as a new item." : "No items to suggest."}</li>}
          {matches.map((k, i) => (
            <li key={`${k.nameSi ?? k.name}|${k.unit ?? ""}`} id={`item-option-${i}`} role="option" aria-selected={i === active} onMouseDown={(e) => e.preventDefault()} onClick={() => pick(k)} className={`flex min-h-11 cursor-pointer flex-col justify-center border-b border-line-soft px-3 py-1.5 text-sm last:border-0 ${i === active ? "bg-soft" : "hover:bg-soft"}`}>
              <span lang={k.nameSi ? "si" : "en"} className="font-semibold">{k.nameSi ?? k.name}</span>
              {k.nameSi && <span className="text-xs text-muted">{k.name}</span>}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
