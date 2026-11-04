import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { householdApi } from "./householdApi";
import type { Member } from "./types";
import { BUTTON, HEADING, INPUT, LABEL, PRIMARY } from "./ui";
import { useSaveAction } from "./useSaveAction";

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
        {user.isOwner && (
          <form className="flex flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); add.run(undefined); }}>
            <label className={LABEL}>Name<input required autoComplete="off" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} className={INPUT} /></label>
            <label className={LABEL}>Password<input required type="password" minLength={8} autoComplete="new-password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} className={INPUT} /></label>
            <button type="submit" disabled={add.busy} className={PRIMARY}>Add member</button>
          </form>
        )}
        {(add.error || remove.error) && <p role="alert" className="text-sm font-semibold text-warn">{add.error || remove.error}</p>}
      </Card>
    </>
  );
}
