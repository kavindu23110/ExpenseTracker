/** A household member. */
export interface Member { id: number; name: string; isOwner: boolean; whatsAppNumber: string | null }

/** The sign-in state. */
export interface Session { needsSetup: boolean; user: Member | null }

/** One category's budget this month. */
export interface BudgetRow { category: string; limit: number | null; spent: number; percent: number; over: boolean }

/** A savings goal. */
export interface Goal { id: number; name: string; target: number; dueDate: string; saved: number; monthlyNeeded: number; percent: number }

/** A recurring bill this month. */
export interface RecurringBill { id: number; name: string; amount: number; dayOfMonth: number; category: string; dueDate: string; status: "paid" | "overdue" | "dueSoon" | "upcoming" }

/** The planning page. */
export interface Planning { month: string; categories: string[]; budgets: BudgetRow[]; goals: Goal[]; bills: RecurringBill[]; remindersDue: number; customCategories: string[] }

/** A product's price over time. */
export interface PriceHistoryRow {
  name: string;
  nameSi: string | null;
  unit: string | null;
  points: { date: string; shop: string; price: number; percent: number }[];
  changePercent: number;
  shops: { shop: string; price: number; date: string }[];
}

/** An item bought last month and not yet this month. */
export interface ToBuySuggestion { name: string; nameSi: string | null; qty: number; unit: string | null }

/** The pantry and shopping list. */
export interface Pantry {
  pantry: { id: number; name: string; qty: number; unit: string | null; isOut: boolean }[];
  shopping: { id: number; name: string; done: boolean; qty: number | null; unit: string | null }[];
}

/** A payment found in a bank message. */

/** One line of a statistics breakdown. */
export interface StatisticsRow {
  name: string;
  nameSi: string | null;
  amount: number;
}

/** The statistics page. */
export interface Statistics {
  averagePerMonth: number | null;
  months: { month: string; total: number; billCount: number; categories: StatisticsRow[]; shops: StatisticsRow[]; items: StatisticsRow[] }[];
}

/** The WhatsApp section of the settings page. */
export interface WhatsAppStatus { configured: boolean; settings: { name: string; present: boolean }[]; linkedMembers: number; lastReceived: string | null }
