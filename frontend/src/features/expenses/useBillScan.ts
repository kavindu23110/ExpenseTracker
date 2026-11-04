import { useCallback, useEffect, useRef, useState } from "react";
import { api } from "@/lib/api";
import { prepareUpload } from "@/lib/prepareUpload";
import type { BillDraft } from "@/lib/types";

/** Scan flow stages. */
export type ScanStage = "idle" | "scanning" | "review";

const STEP_MS = 900;
const WAITING_STEP = 3;

/** Owns the scan state machine: idle, scanning, review. */
export function useBillScan() {
  const [stage, setStage] = useState<ScanStage>("idle");
  const [step, setStep] = useState(0);
  const [fileName, setFileName] = useState("");
  const [draft, setDraft] = useState<BillDraft | null>(null);
  const [file, setFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [error, setError] = useState("");
  const run = useRef(0);

  useEffect(() => () => { if (previewUrl?.startsWith("blob:")) URL.revokeObjectURL(previewUrl); }, [previewUrl]);

  useEffect(() => {
    if (stage !== "scanning") return;
    const t = setInterval(() => setStep((s) => Math.min(s + 1, WAITING_STEP)), STEP_MS);
    return () => clearInterval(t);
  }, [stage]);

  const start = useCallback(async (name: string, read: () => Promise<{ draft: BillDraft; file: File | null; preview: string | null }>) => {
    const id = ++run.current;
    setError("");
    setFileName(name);
    setStep(0);
    setStage("scanning");
    try {
      const result = await read();
      if (id !== run.current) return;
      setDraft(result.draft);
      setFile(result.file);
      setPreviewUrl(result.preview);
      setStage("review");
    } catch (e) {
      if (id !== run.current) return;
      setError(e instanceof Error ? e.message : "That bill could not be read.");
      setStage("idle");
    }
  }, []);

  const scanFile = useCallback((picked: File, lang: string) => start(picked.name, async () => {
    const prepared = await prepareUpload(picked);
    const scanned = await api.scan(prepared, lang);
    return { draft: scanned, file: prepared, preview: prepared.type.startsWith("image/") ? URL.createObjectURL(prepared) : null };
  }), [start]);

  const reset = useCallback(() => {
    run.current++;
    setStage("idle");
    setDraft(null);
    setFile(null);
    setPreviewUrl(null);
  }, []);

  return { stage, step, fileName, draft, setDraft, file, previewUrl, error, scanFile, reset };
}
