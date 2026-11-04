import type { BillItem } from "@/lib/types";

/** An item's printed name with its English translation below when Sinhala. */
export function ItemName({ item, suffix }: { item: Pick<BillItem, "name" | "nameSi">; suffix?: React.ReactNode }) {
  return (
    <div className="flex min-w-0 flex-col gap-px">
      <span lang={item.nameSi ? "si" : "en"}>{item.nameSi ?? item.name} {suffix}</span>
      {item.nameSi && <span lang="en" className="text-xs text-muted">{item.name}</span>}
    </div>
  );
}
