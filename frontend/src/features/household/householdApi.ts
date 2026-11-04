import { ApiError } from "@/lib/api";
import type { Member, Pantry, Planning, PriceHistoryRow, Session, SmsDraft, ToBuySuggestion } from "./types";

async function call<T>(url: string, method = "GET", body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method,
    headers: body === undefined ? undefined : { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  if (res.status === 429) throw new ApiError("Too many attempts. Wait a minute and try again.", res.status);
  if (!res.ok) {
    const problem = await res.json().catch(() => null);
    throw new ApiError(problem?.detail ?? "Something went wrong. Please try again.", res.status);
  }
  return res.status === 204 ? (undefined as T) : res.json();
}

/** Calls the sign-in and household routes. */
export const householdApi = {
  session: () => call<Session>("/api/auth/session"),
  setup: (name: string, password: string) => call<Member>("/api/auth/setup", "POST", { name, password }),
  login: (name: string, password: string) => call<Member>("/api/auth/login", "POST", { name, password }),
  logout: () => call<void>("/api/auth/logout", "POST"),
  members: () => call<Member[]>("/api/auth/members"),
  addMember: (name: string, password: string) => call<Member>("/api/auth/members", "POST", { name, password }),
  removeMember: (id: number) => call<void>(`/api/auth/members/${id}`, "DELETE"),

  planning: () => call<Planning>("/api/planning"),
  addCategory: (name: string) => call<void>("/api/categories", "POST", { name }),
  removeCategory: (name: string) => call<void>(`/api/categories/${encodeURIComponent(name)}`, "DELETE"),
  setBudget: (category: string, limit: number | null) => call<void>("/api/budgets", "PUT", { category, limit }),
  addGoal: (name: string, target: number, dueDate: string) => call<void>("/api/goals", "POST", { name, target, dueDate }),
  addSaving: (id: number, amount: number) => call<void>(`/api/goals/${id}/savings`, "POST", { amount }),
  removeGoal: (id: number) => call<void>(`/api/goals/${id}`, "DELETE"),
  addRecurring: (name: string, amount: number, dayOfMonth: number, category: string) => call<void>("/api/recurring", "POST", { name, amount, dayOfMonth, category }),
  payRecurring: (id: number, amount: number) => call<void>(`/api/recurring/${id}/pay`, "POST", { amount }),
  removeRecurring: (id: number) => call<void>(`/api/recurring/${id}`, "DELETE"),

  prices: () => call<PriceHistoryRow[]>("/api/prices"),

  pantry: () => call<Pantry>("/api/pantry"),
  addPantry: (name: string, qty: number, unit: string) => call<void>("/api/pantry", "POST", { name, qty, unit }),
  adjustPantry: (id: number, change: number) => call<void>(`/api/pantry/${id}/adjust`, "POST", { change }),
  useUpPantry: (id: number) => call<void>(`/api/pantry/${id}/use-up`, "POST"),
  removePantry: (id: number) => call<void>(`/api/pantry/${id}`, "DELETE"),
  toBuySuggestions: () => call<ToBuySuggestion[]>("/api/shopping/suggestions"),
  lastMonthGroceries: () => call<{ month: string; total: number }>("/api/shopping/last-month"),
  knownItems: () => call<ToBuySuggestion[]>("/api/shopping/known"),
  addShopping: (name: string) => call<void>("/api/shopping", "POST", { name }),
  setShoppingDone: (id: number, done: boolean) => call<void>(`/api/shopping/${id}`, "PUT", { done }),
  setShoppingAmount: (id: number, qty: number | null, unit: string) => call<void>(`/api/shopping/${id}/amount`, "PUT", { qty, unit }),
  removeShopping: (id: number) => call<void>(`/api/shopping/${id}`, "DELETE"),

  smsPreview: (text: string) => call<SmsDraft[]>("/api/sms/preview", "POST", { text }),
  smsImport: (items: SmsDraft[]) => call<{ added: number }>("/api/sms/import", "POST", { items }),
};
