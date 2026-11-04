import { useQuery } from "@tanstack/react-query";
import { Badge } from "@/components/ui/Badge";
import { Bar } from "@/components/ui/Bar";
import { Card } from "@/components/ui/Card";
import { money2, shortDate } from "@/lib/format";
import { householdApi } from "./householdApi";

/** What each product has cost across the household's own bills. */
export function PricesPage() {
  const { data, isError } = useQuery({ queryKey: ["prices"], queryFn: householdApi.prices });
  if (isError) return <p role="alert" className="text-warn">This page could not be loaded.</p>;
  if (!data) return <p role="status" className="text-muted">Loading…</p>;
  return (
    <>
      <div className="flex flex-col gap-1">
        <h1 className="text-[32px] font-extrabold tracking-[-0.02em]">Your prices</h1>
        <p className="text-[15px] text-muted">Products you have bought at least twice, with the biggest price rises first. Pack sizes are only compared when the names match or you merged them.</p>
      </div>
      {data.length === 0 && <Card label="No price history"><p className="text-[15px] text-muted">Nothing yet. A product appears here once it is on two of your bills.</p></Card>}
      <div className="grid grid-cols-[repeat(auto-fit,minmax(min(320px,100%),1fr))] gap-6">
        {data.map((h) => (
          <Card key={`${h.nameSi ?? h.name}|${h.unit ?? ""}`} label={h.name} className="gap-3">
            <div className="flex flex-wrap items-start justify-between gap-2">
              <div className="flex min-w-0 flex-col gap-0.5">
                <h2 lang={h.nameSi ? "si" : "en"} className="text-base font-extrabold">{h.nameSi ?? h.name}{h.unit && `, per ${h.unit}`}</h2>
                {h.nameSi && <span className="text-xs text-muted">{h.name}</span>}
              </div>
              <Badge tone={h.changePercent > 0 ? "bad" : h.changePercent < 0 ? "good" : "neutral"}>{h.changePercent > 0 ? "+" : ""}{h.changePercent}%</Badge>
            </div>
            {h.points.map((p, i) => (
              <div key={i} className="flex flex-col gap-1">
                <div className="flex justify-between gap-3 text-[13px]">
                  <span className="text-ink-2">{shortDate(p.date)} {p.date.slice(0, 4)} · {p.shop}</span>
                  <span className="font-mono font-semibold whitespace-nowrap">{money2(p.price)}</span>
                </div>
                <Bar percent={p.percent} />
              </div>
            ))}
            <div className="flex flex-col gap-1 border-t border-line-soft pt-3">
              <h3 className="text-[13px] font-bold">Lowest price by shop, from the latest purchase at each</h3>
              {h.shops.map((s, i) => (
                <div key={s.shop} className="flex justify-between gap-3 text-[13px]">
                  <span className="min-w-0 text-ink-2">{s.shop} <span className="text-muted">· {shortDate(s.date)} {s.date.slice(0, 4)}</span></span>
                  <span className="whitespace-nowrap">
                    {i === 0 ? <Badge tone="good">Lowest</Badge> : <span className="text-muted">+{money2(s.price - h.shops[0].price)}</span>}
                    <strong className="ml-2 font-mono">{money2(s.price)}</strong>
                  </span>
                </div>
              ))}
            </div>
          </Card>
        ))}
      </div>
    </>
  );
}
