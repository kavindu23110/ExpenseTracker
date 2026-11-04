"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from "react";
import { ApiError } from "@/lib/api";

/** Waits about 30 seconds for sleeping servers to wake; a rejected request will not change on retry. */
const retry = (failures: number, error: Error) => !(error instanceof ApiError && error.status < 500) && failures < 5;

/** Client-side providers. */
export function Providers({ children }: { children: React.ReactNode }) {
  const [client] = useState(() => new QueryClient({ defaultOptions: { queries: { refetchOnWindowFocus: false, retry } } }));
  return <QueryClientProvider client={client}>{children}</QueryClientProvider>;
}
