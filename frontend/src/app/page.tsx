import { AuthGate } from "@/features/household/AuthGate";

/** The app, behind sign-in. */
export default function Home() {
  return <AuthGate />;
}
