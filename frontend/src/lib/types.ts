/** One line on a bill. */
export interface BillItem {
  name: string;
  nameSi: string | null;
  qty: number;
  price: number;
  unit: string | null;
  markedPrice: number | null;
  marketKey: string | null;
  amount: number;
}

/** A non-item amount on a bill. */
export interface BillCharge {
  label: string;
  labelSi: string | null;
  amount: number;
}

/** A row in the recent bills table. */
export interface BillRow {
  id: number;
  merchant: string;
  category: string;
  scanned: boolean;
  lang: string;
  date: string;
  amount: number;
  itemCount: number;
}

/** A bill with everything the detail panel shows. */
export interface BillDetail {
  id: number;
  lang: string;
  merchant: string;
  merchantSi: string | null;
  address: string | null;
  billNo: string | null;
  category: string;
  date: string;
  time: string | null;
  payment: string | null;
  paid: number | null;
  change: number | null;
  subtotal: number;
  amount: number;
  saved: number;
  scanned: boolean;
  fileName: string | null;
  fileContentType: string | null;
  items: BillItem[];
  charges: BillCharge[];
}

/** A scanned bill waiting to be checked. */
export interface BillDraft {
  lang: string;
  merchant: string;
  merchantSi: string | null;
  address: string | null;
  addressSi: string | null;
  billNo: string | null;
  date: string | null;
  time: string | null;
  payment: string | null;
  paymentSi: string | null;
  paid: number | null;
  amount: number;
  computedTotal: number;
  totalMismatch: boolean;
  saved: number;
  fileName: string;
  suggestion: { category: string; confidence: number; reason: string };
  items: BillItem[];
  charges: BillCharge[];
}

/** Item price check row. */
export interface PriceCheck {
  name: string;
  nameSi: string | null;
  shop: string;
  paid: number;
  marked: number | null;
  market: number | null;
  unit: string | null;
  marketUnit: string | null;
  verdict: string;
  tone: "bad" | "good" | "neutral";
  note: string;
}

/** A product's price history. */
export interface Timeline {
  title: string;
  summary: string;
  foot: string;
  points: { label: string; price: number; percent: number; isUser: boolean }[];
}

/** Everything the dashboard shows for a set of filters. */
export interface Dashboard {
  today: string;
  periodLabel: string;
  allCount: number;
  periods: { value: string; label: string }[];
  shops: string[];
  categories: string[];
  allCategories: string[];
  bills: BillRow[];
  summary: { total: number; count: number; sinhalaCount: number; topCategory: string | null };
  breakdown: { name: string; amount: number; share: number; percent: number }[];
  inflationCards: { label: string; rate: number; previousRate: number | null; note: string | null }[];
  headlineRate: number;
  inflationRows: { name: string; rate: number; extra: number }[];
  inflationTotal: number;
  discounts: { billId: number; shop: string; saved: number; percent: number }[];
  discountTotal: number;
  checks: PriceCheck[];
  itemMonths: { month: string; total: number; items: { name: string; nameSi: string | null; qty: number; unit: string | null; amount: number; keys: string[]; names: string[] }[] }[];
  itemQuestions: { keyA: string; nameA: string; keyB: string; nameB: string }[];
  timelines: Timeline[];
  readerAvailable: boolean;
}

/** The dashboard filters. */
export interface Filters {
  q: string;
  period: string;
  shop: string;
  lang: string;
  source: string;
  category: string;
}

export const NO_FILTERS: Filters = { q: "", period: "all", shop: "", lang: "", source: "", category: "" };
