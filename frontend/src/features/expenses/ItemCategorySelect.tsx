/** Picks an item's category; it shows the bill's category until the user overrides it. */
export function ItemCategorySelect({ id, value, billCategory, categories, onChange, className = "" }: {
  id: string;
  value: string | null | undefined;
  billCategory: string;
  categories: string[];
  onChange: (category: string | null) => void;
  className?: string;
}) {
  const options = categories.includes(billCategory) ? categories : [billCategory, ...categories];
  return (
    <div className={`flex min-w-0 flex-col gap-1.5 ${className}`}>
      <label htmlFor={id} className="text-[13px] font-bold text-ink-2">Category</label>
      <select id={id} value={value ?? billCategory} onChange={(e) => onChange(e.target.value === billCategory ? null : e.target.value)} className="min-h-11 min-w-0 rounded-[10px] border border-field bg-white px-3 text-[15px] text-ink">
        {options.map((c) => <option key={c} value={c}>{c}</option>)}
      </select>
    </div>
  );
}
