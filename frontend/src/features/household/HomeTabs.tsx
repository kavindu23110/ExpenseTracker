import { useQuery } from "@tanstack/react-query";
import { Ellipsis, ListChecks, ReceiptText, Tag, Wallet } from "lucide-react";
import { useState } from "react";
import { ExpensesPage } from "@/features/expenses/ExpensesPage";
import { householdApi } from "./householdApi";
import { MembersPage } from "./MembersPage";
import { PantryPage } from "./PantryPage";
import { PlanningPage } from "./PlanningPage";
import { PricesPage } from "./PricesPage";
import { SmsImportPage } from "./SmsImportPage";
import { ToBuyPage } from "./ToBuyPage";
import type { Member } from "./types";

const TABS = [
  { id: "expenses", name: "Expenses" },
  { id: "planning", name: "Budgets & bills" },
  { id: "prices", name: "Prices" },
  { id: "tobuy", name: "To buy" },
  { id: "pantry", name: "Pantry" },
  { id: "sms", name: "Bank SMS" },
  { id: "members", name: "Household" },
];

const PHONE_TABS = [
  { id: "expenses", name: "Expenses", icon: ReceiptText },
  { id: "planning", name: "Budgets", icon: Wallet },
  { id: "tobuy", name: "To buy", icon: ListChecks },
  { id: "prices", name: "Prices", icon: Tag },
];
const MORE_TABS = TABS.filter((t) => !PHONE_TABS.some((p) => p.id === t.id));

/** The navigation between the app's pages. */
export function HomeTabs({ user }: { user: Member }) {
  const [tab, setTab] = useState("expenses");
  const [moreOpen, setMoreOpen] = useState(false);
  const open = (id: string) => {
    setTab(id);
    setMoreOpen(false);
    window.scrollTo(0, 0);
  };
  const { data: planning } = useQuery({ queryKey: ["planning"], queryFn: householdApi.planning });
  const { data: pantry } = useQuery({ queryKey: ["pantry"], queryFn: householdApi.pantry });
  const reminders = planning?.remindersDue ?? 0;
  const toBuy = pantry?.shopping.filter((s) => !s.done).length ?? 0;
  return (
    <>
      <nav aria-label="Main" className="hidden border-b border-line bg-white px-6 sm:block">
        <div className="mx-auto flex max-w-[1280px] gap-1 overflow-x-auto py-2">
          {TABS.map((t) => (
            <button key={t.id} type="button" aria-current={tab === t.id ? "page" : undefined} onClick={() => open(t.id)} className={`inline-flex min-h-11 flex-none items-center gap-2 rounded-[10px] px-3.5 text-sm ${tab === t.id ? "bg-ink font-bold text-white" : "font-semibold text-ink-2"}`}>
              {t.name}
              {t.id === "tobuy" && toBuy > 0 && <span aria-label={`${toBuy} items to buy`} className="rounded-full bg-soft px-1.5 text-xs font-bold text-ink">{toBuy}</span>}
              {t.id === "planning" && reminders > 0 && <span aria-label={`${reminders} bills need attention`} className="rounded-full bg-warn-soft px-1.5 text-xs font-bold text-warn">{reminders}</span>}
            </button>
          ))}
        </div>
      </nav>
      <nav aria-label="Main" className="fixed inset-x-0 bottom-0 z-30 border-t border-line bg-white pb-[env(safe-area-inset-bottom)] sm:hidden">
        {moreOpen && (
          <div className="flex flex-col border-b border-line p-2">
            {MORE_TABS.map((t) => <button key={t.id} type="button" aria-current={tab === t.id ? "page" : undefined} onClick={() => open(t.id)} className={`min-h-12 rounded-[10px] px-4 text-left text-[15px] ${tab === t.id ? "bg-ink font-bold text-white" : "font-semibold text-ink-2"}`}>{t.name}</button>)}
          </div>
        )}
        <div className="grid grid-cols-5">
          {PHONE_TABS.map((t) => (
            <button key={t.id} type="button" aria-current={tab === t.id ? "page" : undefined} onClick={() => open(t.id)} className={`relative flex min-h-14 flex-col items-center justify-center gap-0.5 text-[11px] ${tab === t.id ? "font-bold text-accent" : "font-semibold text-muted"}`}>
              <t.icon size={21} aria-hidden />
              {t.name}
              {t.id === "tobuy" && toBuy > 0 && <span aria-label={`${toBuy} items to buy`} className="absolute top-1 left-1/2 ml-1.5 rounded-full bg-accent px-1.5 text-[10px] font-bold text-white">{toBuy}</span>}
              {t.id === "planning" && reminders > 0 && <span aria-label={`${reminders} bills need attention`} className="absolute top-1 left-1/2 ml-1.5 rounded-full bg-warn px-1.5 text-[10px] font-bold text-white">{reminders}</span>}
            </button>
          ))}
          <button type="button" aria-expanded={moreOpen} onClick={() => setMoreOpen(!moreOpen)} className={`flex min-h-14 flex-col items-center justify-center gap-0.5 text-[11px] ${moreOpen || MORE_TABS.some((t) => t.id === tab) ? "font-bold text-accent" : "font-semibold text-muted"}`}>
            <Ellipsis size={21} aria-hidden />
            More
          </button>
        </div>
      </nav>
      {tab === "expenses" && <ExpensesPage />}
      {tab !== "expenses" && (
        <main className="min-h-screen px-4 pt-5 pb-28 sm:px-6 sm:pt-8 sm:pb-16">
          <div className="mx-auto flex max-w-[1280px] flex-col gap-6">
            {tab === "planning" && <PlanningPage />}
            {tab === "prices" && <PricesPage />}
            {tab === "tobuy" && <ToBuyPage />}
            {tab === "pantry" && <PantryPage />}
            {tab === "sms" && <SmsImportPage />}
            {tab === "members" && <MembersPage user={user} />}
          </div>
        </main>
      )}
    </>
  );
}
