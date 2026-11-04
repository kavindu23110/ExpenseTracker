import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Bar } from "@/components/ui/Bar";
import { Card } from "@/components/ui/Card";
import { money, monthYear, shortDate } from "@/lib/format";
import { householdApi } from "./householdApi";
import type { BudgetRow, Goal, Planning, RecurringBill } from "./types";
import { BUTTON, HEADING, INPUT, LABEL, PRIMARY } from "./ui";
import { useSaveAction } from "./useSaveAction";

const STATUS = {
  paid: { tone: "good", text: "Paid" },
  overdue: { tone: "bad", text: "Overdue" },
  dueSoon: { tone: "bad", text: "Due soon" },
  upcoming: { tone: "neutral", text: "Upcoming" },
} as const;

function BudgetLine({ row }: { row: BudgetRow }) {
  const [editing, setEditing] = useState(false);
  const [limit, setLimit] = useState("");
  const save = useSaveAction((value: number | null) => householdApi.setBudget(row.category, value), () => setEditing(false));
  return (
    <div className="flex flex-col gap-1.5 border-b border-line-soft py-3 last:border-0">
      <div className="flex flex-wrap items-center justify-between gap-2 text-sm">
        <strong>{row.category}</strong>
        <span className="flex items-center gap-2">
          <span className="font-mono">{money(row.spent)}{row.limit != null && ` of ${money(row.limit)}`}</span>
          {row.over && <Badge tone="bad">Over budget</Badge>}
          <button type="button" aria-label={`Set budget for ${row.category}`} onClick={() => { setLimit(row.limit?.toString() ?? ""); setEditing(!editing); }} className={BUTTON}>{row.limit == null ? "Set" : "Change"}</button>
        </span>
      </div>
      {row.limit != null && <Bar percent={Math.min(100, row.percent)} warn={row.over} />}
      {editing && (
        <form className="flex flex-wrap gap-2" onSubmit={(e) => { e.preventDefault(); save.run(Number(limit)); }}>
          <input required type="number" min="1" step="any" aria-label={`Monthly budget for ${row.category}`} value={limit} onChange={(e) => setLimit(e.target.value)} className={`${INPUT} w-36`} />
          <button type="submit" disabled={save.busy} className={PRIMARY}>Save</button>
          {row.limit != null && <button type="button" disabled={save.busy} onClick={() => { if (window.confirm(`Remove the budget for ${row.category}?`)) save.run(null); }} className={BUTTON}>Remove</button>}
        </form>
      )}
      {save.error && <p role="alert" className="text-sm font-semibold text-warn">{save.error}</p>}
    </div>
  );
}

function GoalCard({ goal }: { goal: Goal }) {
  const [amount, setAmount] = useState("");
  const add = useSaveAction((value: number) => householdApi.addSaving(goal.id, value), () => setAmount(""));
  const remove = useSaveAction(() => householdApi.removeGoal(goal.id));
  return (
    <div className="flex flex-col gap-2 rounded-xl border border-line p-4">
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <strong>{goal.name}</strong>
        <span className="text-[13px] text-muted">by {shortDate(goal.dueDate)} {goal.dueDate.slice(0, 4)}</span>
      </div>
      <Bar percent={Math.min(100, goal.percent)} />
      <p className="text-sm"><span className="font-mono">{money(goal.saved)}</span> of <span className="font-mono">{money(goal.target)}</span>{goal.monthlyNeeded > 0 && <> · set aside <strong className="font-mono">{money(goal.monthlyNeeded)}</strong> a month</>}</p>
      <form className="flex flex-wrap gap-2" onSubmit={(e) => { e.preventDefault(); add.run(Number(amount)); }}>
        <input required type="number" step="any" aria-label={`Amount for ${goal.name}`} placeholder="Amount" value={amount} onChange={(e) => setAmount(e.target.value)} className={`${INPUT} w-32`} />
        <button type="submit" disabled={add.busy} className={BUTTON}>Add saving</button>
        <button type="button" disabled={remove.busy} onClick={() => { if (window.confirm(`Delete the goal "${goal.name}"?`)) remove.run(undefined); }} className={BUTTON}>Delete</button>
      </form>
      {(add.error || remove.error) && <p role="alert" className="text-sm font-semibold text-warn">{add.error || remove.error}</p>}
    </div>
  );
}

function AddGoal() {
  const [form, setForm] = useState({ name: "", target: "", due: "" });
  const save = useSaveAction(() => householdApi.addGoal(form.name, Number(form.target), form.due), () => setForm({ name: "", target: "", due: "" }));
  return (
    <form className="flex flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); save.run(undefined); }}>
      <label className={LABEL}>Saving for<input required value={form.name} placeholder="School fees" onChange={(e) => setForm({ ...form, name: e.target.value })} className={INPUT} /></label>
      <label className={LABEL}>Target<input required type="number" min="1" step="any" value={form.target} onChange={(e) => setForm({ ...form, target: e.target.value })} className={`${INPUT} w-32`} /></label>
      <label className={LABEL}>Needed by<input required type="date" value={form.due} onChange={(e) => setForm({ ...form, due: e.target.value })} className={INPUT} /></label>
      <button type="submit" disabled={save.busy} className={PRIMARY}>Add goal</button>
      {save.error && <p role="alert" className="w-full text-sm font-semibold text-warn">{save.error}</p>}
    </form>
  );
}

function BillLine({ bill }: { bill: RecurringBill }) {
  const [amount, setAmount] = useState(bill.amount.toString());
  const pay = useSaveAction(() => householdApi.payRecurring(bill.id, Number(amount)));
  const remove = useSaveAction(() => householdApi.removeRecurring(bill.id));
  const status = STATUS[bill.status];
  return (
    <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line-soft py-3 last:border-0">
      <div className="flex min-w-0 flex-col gap-0.5">
        <span className="flex flex-wrap items-center gap-2"><strong>{bill.name}</strong><Badge tone={status.tone}>{status.text}</Badge></span>
        <span className="text-[13px] text-muted">Due {shortDate(bill.dueDate)} · {bill.category}</span>
      </div>
      <form className="flex flex-wrap items-center gap-2" onSubmit={(e) => { e.preventDefault(); pay.run(undefined); }}>
        {bill.status === "paid" ? <span className="font-mono text-sm">{money(bill.amount)} usual</span> : (
          <>
            <input required type="number" min="0.01" step="any" aria-label={`Amount paid for ${bill.name}`} value={amount} onChange={(e) => setAmount(e.target.value)} className={`${INPUT} w-28`} />
            <button type="submit" disabled={pay.busy} className={PRIMARY}>Mark paid</button>
          </>
        )}
        <button type="button" disabled={remove.busy} onClick={() => { if (window.confirm(`Delete the recurring bill "${bill.name}"? Expenses already logged stay.`)) remove.run(undefined); }} className={BUTTON}>Delete</button>
      </form>
      {(pay.error || remove.error) && <p role="alert" className="w-full text-sm font-semibold text-warn">{pay.error || remove.error}</p>}
    </div>
  );
}

function AddBill({ categories }: { categories: string[] }) {
  const empty = { name: "", amount: "", day: "", category: "Utilities" };
  const [form, setForm] = useState(empty);
  const save = useSaveAction(() => householdApi.addRecurring(form.name, Number(form.amount), Number(form.day), form.category), () => setForm(empty));
  return (
    <form className="flex flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); save.run(undefined); }}>
      <label className={LABEL}>Bill<input required value={form.name} placeholder="Electricity" onChange={(e) => setForm({ ...form, name: e.target.value })} className={INPUT} /></label>
      <label className={LABEL}>Usual amount<input required type="number" min="0.01" step="any" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} className={`${INPUT} w-28`} /></label>
      <label className={LABEL}>Due day<input required type="number" min="1" max="31" value={form.day} onChange={(e) => setForm({ ...form, day: e.target.value })} className={`${INPUT} w-20`} /></label>
      <label className={LABEL}>Category<select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })} className={INPUT}>{categories.map((c) => <option key={c}>{c}</option>)}</select></label>
      <button type="submit" disabled={save.busy} className={PRIMARY}>Add bill</button>
      {save.error && <p role="alert" className="w-full text-sm font-semibold text-warn">{save.error}</p>}
    </form>
  );
}

function Content({ data }: { data: Planning }) {
  const attention = data.bills.filter((b) => b.status === "overdue" || b.status === "dueSoon");
  return (
    <>
      <h1 className="text-[32px] font-extrabold tracking-[-0.02em]">Budgets &amp; bills</h1>
      {attention.length > 0 && (
        <p role="status" className="rounded-[10px] bg-warn-soft px-4 py-3 text-sm font-semibold text-warn">
          Needs attention: {attention.map((b) => `${b.name} (${b.status === "overdue" ? "overdue since" : "due"} ${shortDate(b.dueDate)})`).join(", ")}
        </p>
      )}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card label="Monthly budgets" className="gap-1">
          <h2 className={HEADING}>Budgets for {monthYear(`${data.month}-01`)}</h2>
          <p className="mb-2 text-[13px] text-muted">Spending counts every bill dated this month.</p>
          {data.budgets.map((b) => <BudgetLine key={b.category} row={b} />)}
        </Card>
        <div className="flex min-w-0 flex-col gap-6">
          <Card label="Recurring bills" className="gap-3">
            <div className="flex flex-col gap-0.5">
              <h2 className={HEADING}>Recurring bills</h2>
              <p className="text-[13px] text-muted">Marking a bill paid adds it to your expenses.</p>
            </div>
            {data.bills.length === 0 ? <p className="text-[15px] text-muted">No recurring bills yet.</p> : <div>{data.bills.map((b) => <BillLine key={b.id} bill={b} />)}</div>}
            <AddBill categories={data.categories} />
          </Card>
          <Categories custom={data.customCategories} />
          <Card label="Savings goals" className="gap-3">
            <div className="flex flex-col gap-0.5">
              <h2 className={HEADING}>Saving for irregular costs</h2>
              <p className="text-[13px] text-muted">Set money aside each month for costs that don&apos;t come monthly. This records what you have set aside; it doesn&apos;t move any money.</p>
            </div>
            {data.goals.length === 0 ? <p className="text-[15px] text-muted">No goals yet.</p> : data.goals.map((g) => <GoalCard key={g.id} goal={g} />)}
            <AddGoal />
          </Card>
        </div>
      </div>
    </>
  );
}

function Categories({ custom }: { custom: string[] }) {
  const [name, setName] = useState("");
  const add = useSaveAction(() => householdApi.addCategory(name), () => setName(""));
  const remove = useSaveAction((category: string) => householdApi.removeCategory(category));
  return (
    <Card label="Your categories" className="gap-3">
      <div className="flex flex-col gap-0.5">
        <h2 className={HEADING}>Your categories</h2>
        <p className="text-[13px] text-muted">Add your own for bills and budgets. A category in use can&apos;t be removed.</p>
      </div>
      {custom.map((c) => (
        <div key={c} className="flex items-center justify-between gap-2 border-b border-line-soft pb-3 text-sm">
          <strong className="min-w-0">{c}</strong>
          <button type="button" aria-label={`Remove category ${c}`} disabled={remove.busy} onClick={() => { if (!window.confirm(`Remove the category "${c}"?`)) return; add.reset(); remove.run(c); }} className={BUTTON}>Remove</button>
        </div>
      ))}
      <form className="flex flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); remove.reset(); add.run(undefined); }}>
        <label className={LABEL}>New category<input required maxLength={50} autoComplete="off" value={name} onChange={(e) => setName(e.target.value)} className={INPUT} /></label>
        <button type="submit" disabled={add.busy} className={PRIMARY}>Add category</button>
      </form>
      {(add.error || remove.error) && <p role="alert" className="text-sm font-semibold text-warn">{add.error || remove.error}</p>}
    </Card>
  );
}

/** Budgets, recurring bills and savings goals. */
export function PlanningPage() {
  const { data, isError } = useQuery({ queryKey: ["planning"], queryFn: householdApi.planning });
  if (isError) return <p role="alert" className="text-warn">This page could not be loaded.</p>;
  if (!data) return <p role="status" className="text-muted">Loading…</p>;
  return <Content data={data} />;
}
