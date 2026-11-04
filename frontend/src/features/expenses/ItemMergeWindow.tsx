import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useMemo, useRef, useState } from "react";
import { api } from "@/lib/api";
import { money2 } from "@/lib/format";
import type { Dashboard } from "@/lib/types";

/** A window for choosing items by hand and merging them into one product. */
export function ItemMergeWindow({ months, onClose }: { months: Dashboard["itemMonths"]; onClose: () => void }) {
  const queryClient = useQueryClient();
  const dialog = useRef<HTMLDialogElement>(null);
  const [search, setSearch] = useState("");
  const [chosen, setChosen] = useState<string[]>([]);
  useEffect(() => { dialog.current?.showModal(); }, []);

  const items = useMemo(() => {
    const all = new Map<string, { id: string; keys: string[]; name: string; names: string[]; units: Set<string>; amount: number }>();
    for (const item of months.flatMap((m) => m.items)) {
      const id = item.keys.join("|");
      const row = all.get(id) ?? { id, keys: item.keys, name: item.nameSi ?? item.name, names: item.names, units: new Set<string>(), amount: 0 };
      if (item.unit) row.units.add(item.unit);
      row.amount += item.amount;
      all.set(id, row);
    }
    return [...all.values()].sort((a, b) => a.name.localeCompare(b.name));
  }, [months]);

  const merge = useMutation({
    mutationFn: () => api.decideItems(items.filter((i) => chosen.includes(i.id)).flatMap((i) => i.keys), true),
    onSuccess: async () => {
      await queryClient.invalidateQueries();
      onClose();
    },
  });
  const term = search.trim().toLowerCase();
  const shown = items.filter((i) => !term || [i.name, ...i.names].some((n) => n.toLowerCase().includes(term)));

  return (
    <dialog ref={dialog} aria-labelledby="merge-title" onClose={onClose} className="m-auto w-[min(560px,calc(100vw-32px))] rounded-[14px] border border-line bg-white p-5 text-ink backdrop:bg-black/40">
      <div className="flex flex-col gap-3">
        <div className="flex flex-col gap-0.5">
          <h2 id="merge-title" className="text-xl font-extrabold">Merge items</h2>
          <p className="text-[13px] text-muted">Choose two or more names for the same product. Quantities are only added together when the unit is the same. Different units stay on separate lines.</p>
        </div>
        <input type="search" aria-label="Search items" placeholder="Search items" value={search} onChange={(e) => setSearch(e.target.value)} className="min-h-11 rounded-[10px] border border-field bg-white px-3 text-sm" />
        <div className="flex max-h-[50vh] flex-col overflow-y-auto">
          {shown.length === 0 && <p className="py-3 text-sm text-muted">No items match.</p>}
          {shown.map((i) => (
            <label key={i.id} className="flex min-h-11 items-center gap-3 border-b border-line-soft py-2 text-sm">
              <input type="checkbox" className="size-5 flex-none" checked={chosen.includes(i.id)} onChange={(e) => setChosen(e.target.checked ? [...chosen, i.id] : chosen.filter((c) => c !== i.id))} />
              <span className="flex min-w-0 flex-1 flex-col">
                <strong>{i.name}</strong>
                <span className="text-xs text-muted">{[i.keys.length > 1 ? `Already merged: ${i.names.join(", ")}` : "", [...i.units].join(", ")].filter(Boolean).join(" · ")}</span>
              </span>
              <span className="font-mono text-[13px] whitespace-nowrap">{money2(i.amount)}</span>
            </label>
          ))}
        </div>
        {merge.isError && <p role="alert" className="text-sm font-semibold text-warn">{merge.error.message}</p>}
        <div className="flex flex-wrap justify-end gap-2">
          <button type="button" onClick={onClose} className="min-h-11 rounded-[10px] border border-field bg-white px-4 text-sm font-semibold">Cancel</button>
          <button type="button" disabled={chosen.length < 2 || merge.isPending} onClick={() => merge.mutate()} className="min-h-11 rounded-[10px] bg-accent px-4 text-sm font-bold text-white disabled:opacity-50">Merge {chosen.length >= 2 ? `${chosen.length} items` : "items"}</button>
        </div>
      </div>
    </dialog>
  );
}
