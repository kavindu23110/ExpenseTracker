import type { Metadata, Viewport } from "next";
import { IBM_Plex_Mono, Manrope, Noto_Sans_Sinhala } from "next/font/google";
import { Providers } from "./providers";
import "./globals.css";

const manrope = Manrope({ subsets: ["latin"], variable: "--font-manrope" });
const plexMono = IBM_Plex_Mono({ subsets: ["latin"], weight: ["400", "500", "600"], variable: "--font-plex-mono" });
const sinhala = Noto_Sans_Sinhala({ subsets: ["sinhala"], variable: "--font-sinhala" });

export const metadata: Metadata = {
  title: "Expense tracker",
  description: "Scan bills in English or Sinhala and track what you spend.",
};

export const viewport: Viewport = { width: "device-width", initialScale: 1, viewportFit: "cover" };

/** Root layout with fonts and providers. */
export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en" className={`${manrope.variable} ${plexMono.variable} ${sinhala.variable}`}>
      <body>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
