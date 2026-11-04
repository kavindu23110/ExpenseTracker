import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { householdApi } from "./householdApi";
import { BUTTON, HEADING, INPUT, LABEL, PRIMARY } from "./ui";
import { useSaveAction } from "./useSaveAction";

type Result = { ok: boolean; message: string };

function Outcome({ result }: { result: Result | null }) {
  if (!result) return null;
  return <p role="status" className={`rounded-[10px] px-4 py-3 text-sm font-semibold ${result.ok ? "bg-accent-soft text-accent-ink" : "bg-warn-soft text-warn"}`}>{result.message}</p>;
}

const EMPTY_CONFIG = { verifyToken: "", appSecret: "", accessToken: "", phoneNumberId: "" };
const FIELDS = [
  { key: "verifyToken", label: "Verify token", setting: "Verify token", secret: true },
  { key: "appSecret", label: "App secret", setting: "App secret", secret: true },
  { key: "accessToken", label: "Access token", setting: "Access token", secret: true },
  { key: "phoneNumberId", label: "Phone number ID", setting: "Phone number ID", secret: false },
] as const;

/** Lets the owner set up the WhatsApp number and check that it works. */
export function SettingsPage() {
  const { data, isError, refetch } = useQuery({ queryKey: ["whatsapp-settings"], queryFn: householdApi.whatsAppStatus });
  const [number, setNumber] = useState("");
  const [config, setConfig] = useState(EMPTY_CONFIG);
  const [connection, setConnection] = useState<Result | null>(null);
  const saved = () => {
    setConfig(EMPTY_CONFIG);
    setConnection(null);
    refetch();
  };
  const saveConfig = useSaveAction(() => householdApi.saveWhatsAppConfig(config), saved);
  const clearConfig = useSaveAction(() => householdApi.clearWhatsAppConfig(), saved);
  const [message, setMessage] = useState<Result | null>(null);
  const check = useSaveAction(() => householdApi.whatsAppCheck().then(setConnection));
  const send = useSaveAction(() => householdApi.whatsAppTestMessage(number).then(setMessage));
  if (isError) return <p role="alert" className="text-warn">This page could not be loaded.</p>;
  if (!data) return <p role="status" className="text-muted">Loading…</p>;
  const error = check.error || send.error || saveConfig.error || clearConfig.error;
  return (
    <>
      <div className="flex flex-col gap-1">
        <h1 className="text-[32px] font-extrabold tracking-[-0.02em]">Settings</h1>
        <p className="text-[15px] text-muted">Set up and test the WhatsApp number that receives bills. Saved secrets are encrypted and never shown again.</p>
      </div>
      {error && <p role="alert" className="rounded-[10px] bg-warn-soft px-4 py-3 text-sm font-semibold text-warn">{error}</p>}
      <Card label="WhatsApp settings" className="gap-3">
        <h2 className={HEADING}>Setup</h2>
        <form className="flex flex-col gap-3" onSubmit={(e) => { e.preventDefault(); saveConfig.run(undefined); }}>
          {FIELDS.map((f) => {
            const present = data.settings.find((s) => s.name === f.setting)?.present;
            return (
              <div key={f.key} className="flex flex-col gap-1">
                <span className="flex items-center justify-between gap-2"><label htmlFor={`wa-${f.key}`} className="text-[13px] font-bold text-ink-2">{f.label}</label><Badge tone={present ? "good" : "bad"}>{present ? "Set" : "Missing"}</Badge></span>
                <input id={`wa-${f.key}`} type={f.secret ? "password" : "text"} autoComplete="off" placeholder={present ? "Leave blank to keep" : undefined} value={config[f.key]} onChange={(e) => setConfig({ ...config, [f.key]: e.target.value })} className={INPUT} />
              </div>
            );
          })}
          <button type="submit" disabled={saveConfig.busy} className={`${PRIMARY} self-start`}>{saveConfig.busy ? "Saving…" : "Save settings"}</button>
        </form>
        <button type="button" disabled={clearConfig.busy} onClick={() => { if (window.confirm("Remove the WhatsApp settings saved in the app?")) clearConfig.run(undefined); }} className={`${BUTTON} self-start`}>Remove saved settings</button>
        <p className="text-sm text-ink-2">In Meta, set the callback URL to <code className="font-mono break-all">https://your-hostname/api/whatsapp/webhook</code> with the same verify token, and subscribe to <code className="font-mono">messages</code>.</p>
        <p className="text-sm text-ink-2">Linked members: <strong>{data.linkedMembers}</strong>. Link numbers under Household.</p>
      </Card>
      <Card label="Connection test" className="gap-3">
        <h2 className={HEADING}>Connection</h2>
        <p className="text-[15px] text-muted">Asks Meta whether the access token and phone number ID work.</p>
        <button type="button" disabled={check.busy || !data.configured} onClick={() => check.run(undefined)} className={`${PRIMARY} self-start`}>{check.busy ? "Checking…" : "Test connection"}</button>
        {!data.configured && <p className="text-sm text-muted">Add the missing settings above first.</p>}
        <Outcome result={connection} />
      </Card>
      <Card label="Message test" className="gap-3">
        <h2 className={HEADING}>Test message</h2>
        <p className="text-[15px] text-muted">Message the business number from your phone first. WhatsApp only lets it send free-form replies within 24 hours of your last message.</p>
        <form className="flex flex-wrap items-end gap-2" onSubmit={(e) => { e.preventDefault(); send.run(undefined); }}>
          <label className={LABEL}>Send to<input required inputMode="tel" autoComplete="off" placeholder="94771234567" value={number} onChange={(e) => setNumber(e.target.value)} className={INPUT} /></label>
          <button type="submit" disabled={send.busy || !data.configured} className={PRIMARY}>{send.busy ? "Sending…" : "Send test message"}</button>
        </form>
        <Outcome result={message} />
      </Card>
      <Card label="Incoming messages" className="gap-3">
        <h2 className={HEADING}>Incoming messages</h2>
        <p className="text-[15px] text-muted">Send the number a message, then refresh. A new time here means WhatsApp is reaching this app. It resets when the app restarts.</p>
        <p className="text-sm font-semibold">{data.lastReceived ? `Last received: ${new Date(data.lastReceived).toLocaleString()}` : "Nothing received since the app started."}</p>
        <button type="button" onClick={() => refetch()} className={`${BUTTON} self-start`}>Refresh</button>
      </Card>
    </>
  );
}
