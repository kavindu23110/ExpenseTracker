import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { householdApi } from "./householdApi";
import type { Member } from "./types";
import { BUTTON, HEADING, INPUT, LABEL, PRIMARY } from "./ui";
import { useSaveAction } from "./useSaveAction";

function WhatsAppLink({ member }: { member: Member }) {
  const [number, setNumber] = useState(member.whatsAppNumber ?? "");
  const queryClient = useQueryClient();
  const save = useSaveAction(() => householdApi.linkWhatsApp(member.id, number), () => queryClient.invalidateQueries({ queryKey: ["members"] }));
  return (
    <form className="flex flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); save.run(undefined); }}>
      <label className={LABEL}>{member.name}&apos;s WhatsApp number<input inputMode="tel" autoComplete="off" placeholder="94771234567" value={number} onChange={(e) => setNumber(e.target.value)} className={INPUT} /></label>
      <button type="submit" disabled={save.busy} className={BUTTON}>Save number</button>
      {save.error && <p role="alert" className="w-full text-sm font-semibold text-warn">{save.error}</p>}
    </form>
  );
}

function BalancesCard() {
  const { data } = useQuery({ queryKey: ["balances"], queryFn: householdApi.balances });
  const [form, setForm] = useState({ from: "", to: "", amount: "" });
  const add = useSaveAction(() => householdApi.addSettlement(form.from, form.to, Number(form.amount)), () => setForm({ from: "", to: "", amount: "" }));
  const remove = useSaveAction((id: number) => householdApi.removeSettlement(id));
  const settle = useSaveAction((p: { from: string; to: string; amount: number }) => householdApi.addSettlement(p.from, p.to, p.amount));
  if (!data) return null;
  const names = data.members.map((m) => m.member);
  return (
    <Card label="Balances" className="max-w-2xl gap-3">
      <div className="flex flex-col gap-0.5">
        <h2 className={HEADING}>Who owes whom</h2>
        <p className="text-[13px] text-muted">Bills with a &quot;paid by&quot; member are shared equally by everyone. Record a payment here when someone pays another back.</p>
      </div>
      {data.members.map((m) => (
        <div key={m.member} className="flex flex-wrap items-center justify-between gap-2 border-b border-line-soft pb-3 text-sm">
          <strong>{m.member}</strong>
          <span>Paid Rs {m.paid.toLocaleString()} · share Rs {m.share.toLocaleString()} · <strong className={m.balance < 0 ? "text-warn" : ""}>{m.balance >= 0 ? `is owed Rs ${m.balance.toLocaleString()}` : `owes Rs ${(-m.balance).toLocaleString()}`}</strong></span>
        </div>
      ))}
      {data.payments.length > 0 && (
        <div className="flex flex-col gap-1.5 text-sm">
          <strong>To settle up</strong>
          {data.payments.map((p) => (
            <div key={`${p.from}-${p.to}`} className="flex flex-wrap items-center justify-between gap-2">
              <span>{p.from} pays {p.to} Rs {p.amount.toLocaleString()}</span>
              <button type="button" disabled={settle.busy} onClick={() => { if (window.confirm(`Record that ${p.from} paid ${p.to} Rs ${p.amount}?`)) settle.run(p); }} className={BUTTON}>Mark paid</button>
            </div>
          ))}
        </div>
      )}
      <form className="flex flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); add.run(undefined); }}>
        <label className={LABEL}>From<select required value={form.from} onChange={(e) => setForm({ ...form, from: e.target.value })} className={INPUT}><option value="">Choose</option>{names.map((n) => <option key={n}>{n}</option>)}</select></label>
        <label className={LABEL}>To<select required value={form.to} onChange={(e) => setForm({ ...form, to: e.target.value })} className={INPUT}><option value="">Choose</option>{names.map((n) => <option key={n}>{n}</option>)}</select></label>
        <label className={LABEL}>Amount<input required type="number" min="0.01" step="any" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} className={`${INPUT} w-28`} /></label>
        <button type="submit" disabled={add.busy} className={PRIMARY}>Record payment</button>
      </form>
      {data.settlements.length > 0 && (
        <div className="flex flex-col gap-1.5 text-sm">
          <strong>Recorded payments</strong>
          {data.settlements.map((s) => (
            <div key={s.id} className="flex flex-wrap items-center justify-between gap-2 text-muted">
              <span>{s.date}: {s.from} paid {s.to} Rs {s.amount.toLocaleString()}</span>
              <button type="button" aria-label={`Delete payment from ${s.from} to ${s.to}`} disabled={remove.busy} onClick={() => { if (window.confirm("Delete this payment?")) remove.run(s.id); }} className={BUTTON}>Delete</button>
            </div>
          ))}
        </div>
      )}
      {(add.error || remove.error || settle.error) && <p role="alert" className="text-sm font-semibold text-warn">{add.error || remove.error || settle.error}</p>}
    </Card>
  );
}

/** Who shares this app; the owner adds and removes members. */
export function MembersPage({ user }: { user: Member }) {
  const { data } = useQuery({ queryKey: ["members"], queryFn: householdApi.members });
  const [form, setForm] = useState({ name: "", password: "" });
  const add = useSaveAction(() => householdApi.addMember(form.name, form.password), () => setForm({ name: "", password: "" }));
  const remove = useSaveAction((id: number) => householdApi.removeMember(id));
  const logout = async () => {
    await householdApi.logout();
    window.location.reload();
  };
  return (
    <>
      <h1 className="text-[32px] font-extrabold tracking-[-0.02em]">Household</h1>
      <Card label="Members" className="max-w-2xl gap-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div className="flex flex-col gap-0.5">
            <h2 className={HEADING}>Members</h2>
            <p className="text-[13px] text-muted">Everyone sees and can change the same bills, budgets and lists.</p>
          </div>
          <button type="button" onClick={logout} className={BUTTON}>Sign out {user.name}</button>
        </div>
        {data?.map((m) => (
          <div key={m.id} className="flex items-center justify-between gap-2 border-b border-line-soft pb-3 text-sm">
            <span className="flex items-center gap-2"><strong>{m.name}</strong>{m.isOwner && <Badge tone="neutral">Owner</Badge>}</span>
            {user.isOwner && !m.isOwner && <button type="button" disabled={remove.busy} onClick={() => { if (window.confirm(`Remove ${m.name} from the household?`)) remove.run(m.id); }} className={BUTTON}>Remove</button>}
          </div>
        ))}
        {user.isOwner && data?.map((m) => <WhatsAppLink key={m.id} member={m} />)}
        {user.isOwner && (
          <form className="flex flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); add.run(undefined); }}>
            <label className={LABEL}>Name<input required autoComplete="off" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} className={INPUT} /></label>
            <label className={LABEL}>Password<input required type="password" minLength={8} autoComplete="new-password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} className={INPUT} /></label>
            <button type="submit" disabled={add.busy} className={PRIMARY}>Add member</button>
          </form>
        )}
        {(add.error || remove.error) && <p role="alert" className="text-sm font-semibold text-warn">{add.error || remove.error}</p>}
      </Card>
      <BalancesCard />
    </>
  );
}
