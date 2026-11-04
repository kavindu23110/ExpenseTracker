const MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

/** Formats rupees, showing cents only when present. */
export const money = (n: number) => `Rs ${n.toLocaleString("en-US", { maximumFractionDigits: 2 })}`;

/** Formats rupees with two decimals. */
export const money2 = (n: number) => `Rs ${n.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;

/** Formats a number with two decimals and no currency. */
export const num2 = (n: number) => n.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** Formats an ISO date as "3 Nov". */
export function shortDate(iso: string) {
  const [, m, d] = iso.split("-");
  return `${parseInt(d, 10)} ${MONTHS[parseInt(m, 10) - 1] ?? ""}`;
}

/** Formats an ISO date as "November 2026". */
export function monthYear(iso: string) {
  return new Date(`${iso}T00:00:00`).toLocaleDateString("en-US", { month: "long", year: "numeric" });
}

/** Formats a quantity with its unit. */
export function qtyLabel(qty: number, unit: string | null) {
  return unit ? `${Number(qty.toFixed(3))} ${unit}` : String(qty);
}
