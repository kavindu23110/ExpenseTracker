"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Card } from "@/components/ui/Card";
import { householdApi } from "./householdApi";
import { HomeTabs } from "./HomeTabs";
import { INPUT, LABEL, PRIMARY } from "./ui";

function SignInForm({ needsSetup }: { needsSetup: boolean }) {
  const queryClient = useQueryClient();
  const [name, setName] = useState("");
  const [password, setPassword] = useState("");
  const submit = useMutation({
    mutationFn: () => (needsSetup ? householdApi.setup(name, password) : householdApi.login(name, password)),
    onSuccess: () => queryClient.invalidateQueries(),
  });
  return (
    <main className="mx-auto flex min-h-screen max-w-md flex-col justify-center px-4 py-8">
      <Card label={needsSetup ? "Create your account" : "Sign in"} className="gap-4">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-extrabold">{needsSetup ? "Create your account" : "Sign in"}</h1>
          <p className="text-sm text-muted">{needsSetup ? "You will be the owner, and can add the rest of your household afterwards." : "Sign in to see your household's expenses."}</p>
        </div>
        <form className="flex flex-col gap-3" onSubmit={(e) => { e.preventDefault(); submit.mutate(); }}>
          <label className={LABEL}>Name<input required autoComplete="username" value={name} onChange={(e) => setName(e.target.value)} className={INPUT} /></label>
          <label className={LABEL}>Password<input required type="password" minLength={needsSetup ? 8 : undefined} autoComplete={needsSetup ? "new-password" : "current-password"} value={password} onChange={(e) => setPassword(e.target.value)} className={INPUT} /></label>
          {submit.isError && <p role="alert" className="text-sm font-semibold text-warn">{submit.error.message}</p>}
          <button type="submit" disabled={submit.isPending} className={PRIMARY}>{needsSetup ? "Create account" : "Sign in"}</button>
        </form>
      </Card>
    </main>
  );
}

/** Shows the app only to a signed-in member. */
export function AuthGate() {
  const { data, isError, refetch } = useQuery({ queryKey: ["session"], queryFn: householdApi.session });
  if (isError && !data) {
    return (
      <main className="mx-auto flex max-w-xl flex-col items-start gap-3 p-8">
        <h1 className="text-2xl font-extrabold">The expense service isn&apos;t responding</h1>
        <button type="button" onClick={() => refetch()} className="min-h-11 rounded-[10px] border border-field bg-white px-4 font-semibold">Try again</button>
      </main>
    );
  }
  if (!data) return <main className="p-8 text-muted" role="status">Loading…</main>;
  if (!data.user) return <SignInForm needsSetup={data.needsSetup} />;
  return <HomeTabs user={data.user} />;
}
