import type { BillDetail, BillDraft, Dashboard, Filters } from "./types";

/** An API failure with a message safe to show. */
export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, init);
  if (res.status === 401) window.location.reload();
  if (res.status === 204) return undefined as T;
  if (!res.ok) {
    const body = await res.json().catch(() => null);
    throw new ApiError(body?.detail ?? "Something went wrong. Please try again.", res.status);
  }
  return res.json();
}

/** Calls the expense tracker backend. */
export const api = {
  dashboard: (f: Filters) => {
    const params = new URLSearchParams(Object.entries(f).filter(([, v]) => v));
    return request<Dashboard>(`/api/dashboard?${params}`);
  },
  decideItems: (keys: string[], same: boolean) =>
    request<void>("/api/item-mappings", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ keys, same }) }),
  changeBillCategory: (id: number, category: string) =>
    request<void>(`/api/bills/${id}/category`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ category }) }),
  bill: (id: number) => request<BillDetail>(`/api/bills/${id}`),
  scan: (file: File, lang: string) => {
    const form = new FormData();
    form.append("file", file);
    if (lang !== "auto") form.append("lang", lang);
    return request<BillDraft>("/api/scan", { method: "POST", body: form });
  },
  saveBill: (bill: unknown, file: File | null) => {
    const form = new FormData();
    form.append("bill", JSON.stringify(bill));
    if (file) form.append("file", file);
    return request<{ id: number }>("/api/bills", { method: "POST", body: form });
  },
};
