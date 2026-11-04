import { useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Bar } from "@/components/ui/Bar";
import { money, money2 } from "@/lib/format";
import type { Dashboard, PriceCheck } from "@/lib/types";

const HIGH_RATE = 10;
const BOX = "flex min-w-0 flex-col gap-3 rounded-xl border border-line p-[18px]";
const TH = "px-2 pt-1.5 pb-2 font-bold border-b border-line";
const TD = "px-2 py-2.5 border-b border-line-soft";

const priced = (n: number | null, unit: string | null) => (n == null ? "—" : `${money2(n)}${unit ? `/${unit}` : ""}`);

function CheckRow({ c }: { c: PriceCheck }) {
  return (
    <tr>
      <td className={`${TD} pl-0`}>
        <div className="flex flex-col gap-px">
          <span lang={c.nameSi ? "si" : "en"} className="font-semibold">{c.nameSi ?? c.name}</span>
          {c.nameSi && <span lang="en" className="text-xs text-muted">{c.name}</span>}
        </div>
      </td>
      <td className={`${TD} text-ink-2`}>{c.shop}</td>
      <td className={`${TD} text-right font-mono whitespace-nowrap`}>{priced(c.paid, c.unit)}</td>
      <td className={`${TD} text-right font-mono whitespace-nowrap text-muted`}>{priced(c.marked, c.unit)}</td>
      <td className={`${TD} text-right font-mono whitespace-nowrap text-muted`}>{priced(c.market, c.marketUnit)}</td>
      <td className={`${TD} pr-0`}>
        <div className="flex flex-col items-start gap-0.5">
          <Badge tone={c.tone} large>{c.verdict}</Badge>
          <span className="text-xs text-muted">{c.note}</span>
        </div>
      </td>
    </tr>
  );
}

const PHONE_CHECKS = 5;

function CheckCard({ c }: { c: PriceCheck }) {
  const fact = (label: string, value: string) => <div className="flex min-w-0 flex-col gap-0.5"><dt className="text-[11px] font-bold text-muted uppercase">{label}</dt><dd className="font-mono text-[13px] break-words">{value}</dd></div>;
  return (
    <div className="flex flex-col gap-2 border-b border-line-soft py-3 text-sm last:border-0">
      <div className="flex flex-col gap-px">
        <span lang={c.nameSi ? "si" : "en"} className="font-semibold">{c.nameSi ?? c.name}</span>
        <span className="text-xs text-muted">{c.nameSi && `${c.name} · `}{c.shop}</span>
      </div>
      <dl className="grid grid-cols-3 gap-2">
        {fact("You paid", priced(c.paid, c.unit))}
        {fact("Marked", priced(c.marked, c.unit))}
        {fact("Market", priced(c.market, c.marketUnit))}
      </dl>
      <div className="flex flex-wrap items-center gap-1.5"><Badge tone={c.tone} large>{c.verdict}</Badge><span className="text-xs text-muted">{c.note}</span></div>
    </div>
  );
}

/** Compares bill prices with market prices, marked prices and inflation. */
export function PriceWatch({ data }: { data: Dashboard }) {
  const [allChecks, setAllChecks] = useState(false);
  return (
    <section aria-labelledby="pw-title" className="flex flex-col gap-7 rounded-2xl border border-line bg-white p-4 sm:p-6">
      <div className="flex flex-col gap-1">
        <h2 id="pw-title" className="text-2xl font-extrabold tracking-[-0.01em]">Price watch</h2>
        <p className="max-w-[760px] text-[15px] leading-normal text-muted">How the prices on your bills compare with market prices, marked prices and inflation in Sri Lanka.</p>
      </div>

      <div className="flex flex-col gap-3">
        <h3 className="text-base font-extrabold">Inflation right now</h3>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-[repeat(auto-fit,minmax(190px,1fr))]">
          {data.inflationCards.map((c) => (
            <div key={c.label} className="flex flex-col gap-1 rounded-xl border border-line p-4">
              <span className="text-[13px] font-semibold text-muted">{c.label}</span>
              <span className={`font-mono text-[26px] font-semibold ${c.rate >= HIGH_RATE ? "text-warn" : ""}`}>{c.rate.toFixed(1)}%</span>
              <span className="text-xs text-muted">{c.note ? `${c.note} · ` : ""}{(c.previousRate ?? 0) > c.rate ? "down" : "up"} from {c.previousRate?.toFixed(1)}%</span>
            </div>
          ))}
        </div>
        <p className="text-[13px] leading-normal text-muted">Year-on-year change. Inflation has been above the Central Bank&apos;s 5% ± 2% target for four months running, and the bank expects high single digits until early 2027. A year ago (Oct 2025) it was 2.1%.</p>
      </div>

      <div className="flex flex-wrap items-stretch gap-6">
        <div className={`${BOX} flex-[1_1_360px]`}>
          <h3 className="text-base font-extrabold">What inflation adds to your spending</h3>
          <p className="text-sm leading-normal text-ink-2">The same spending a year ago would have cost about <strong className="font-mono">{money(Math.round(data.inflationTotal))}</strong> less.</p>
          {data.inflationRows.map((r) => (
            <div key={r.name} className="flex justify-between gap-3 border-t border-line-soft pt-2 text-sm">
              <span><strong>{r.name}</strong> <span className="text-muted">· {r.rate}% a year</span></span>
              <span className="font-mono">+{money(Math.round(r.extra))}</span>
            </div>
          ))}
          <p className="text-xs text-muted">Estimate: each category&apos;s spending scaled by its official yearly price rise (headline {data.headlineRate}% where there&apos;s no specific figure).</p>
        </div>
        <div className={`${BOX} flex-[1_1_300px]`}>
          <h3 className="text-base font-extrabold">Shop discounts you got</h3>
          <p className="text-sm leading-normal text-ink-2">Savings against the marked price printed on these bills: <strong className="font-mono text-accent-ink">{money2(data.discountTotal)}</strong>.</p>
          {data.discounts.map((d) => (
            <div key={d.billId} className="flex justify-between gap-3 border-t border-line-soft pt-2 text-sm">
              <span><strong>{d.shop}</strong> <span className="text-muted">· {d.percent.toFixed(1)}% off</span></span>
              <span className="font-mono text-accent-ink">{money2(d.saved)}</span>
            </div>
          ))}
        </div>
      </div>

      <div className="flex flex-col gap-3">
        <div className="flex flex-col gap-0.5">
          <h3 className="text-base font-extrabold">Item price checks</h3>
          <p className="text-[13px] text-muted">Each item compared with the Central Bank&apos;s Pettah market price where one exists, otherwise with the marked price on the bill.</p>
        </div>
        {data.checks.length === 0 ? <p className="text-[15px] text-muted">No items to check yet. Price checks appear once you have saved bills.</p> : (
        <>
        <div className="flex flex-col sm:hidden">
          {(allChecks ? data.checks : data.checks.slice(0, PHONE_CHECKS)).map((c, i) => <CheckCard key={i} c={c} />)}
          {data.checks.length > PHONE_CHECKS && <button type="button" onClick={() => setAllChecks(!allChecks)} className="mt-2 min-h-11 rounded-[10px] border border-field bg-white px-4 text-sm font-semibold">{allChecks ? "Show fewer" : `Show all ${data.checks.length} items`}</button>}
        </div>
        <div className="hidden overflow-x-auto sm:block">
          <table className="w-full min-w-[760px] border-collapse text-sm">
            <thead>
              <tr className="text-left text-[11px] tracking-[0.06em] text-muted uppercase">
                <th scope="col" className={`${TH} pl-0`}>Item</th>
                <th scope="col" className={TH}>Shop</th>
                <th scope="col" className={`${TH} text-right`}>You paid</th>
                <th scope="col" className={`${TH} text-right`}>Marked</th>
                <th scope="col" className={`${TH} text-right`}>Market</th>
                <th scope="col" className={`${TH} pr-0`}>Verdict</th>
              </tr>
            </thead>
            <tbody>{data.checks.map((c, i) => <CheckRow key={i} c={c} />)}</tbody>
          </table>
        </div>
        </>
        )}
      </div>

      {data.timelines.length > 0 && <div className="grid grid-cols-[repeat(auto-fit,minmax(min(320px,100%),1fr))] gap-6">
        {data.timelines.map((t) => (
          <div key={t.title} className={BOX}>
            <div className="flex flex-col gap-0.5">
              <h3 className="text-base font-extrabold">{t.title}</h3>
              <p className="text-[13px] text-muted">{t.summary}</p>
            </div>
            {t.points.map((p) => (
              <div key={p.label} className="flex flex-col gap-1">
                <div className="flex justify-between gap-3 text-[13px]">
                  <span className="text-ink-2">{p.label}</span>
                  <span className="font-mono font-semibold">{money(p.price)}</span>
                </div>
                <Bar percent={p.percent} warn={p.isUser} />
              </div>
            ))}
            <p className="text-xs text-muted">{t.foot}</p>
          </div>
        ))}
      </div>}

      <div className="flex flex-col gap-1.5 border-t border-line-soft pt-4 text-xs leading-relaxed text-muted">
        <span className="font-bold text-ink-2">Sources</span>
        <span>Inflation: <a href="https://www.cbsl.gov.lk/en/node/21123">Central Bank of Sri Lanka, CCPI October 2026</a> · <a href="https://www.ft.lk/front-page/CCPI-inflation-rises-to-8-3-in-Oct/44-799448">Daily FT, 31 Oct 2026</a></span>
        <span>Market prices: CBSL daily price reports, Pettah retail — <a href="https://www.cbsl.gov.lk/sites/default/files/cbslweb_documents/statistics/pricerpt/price_report_20261019_e.pdf">19 Oct 2026</a></span>
        <span>Pettah prices are Colombo market prices; neighbourhood shops usually charge more.</span>
      </div>
    </section>
  );
}
