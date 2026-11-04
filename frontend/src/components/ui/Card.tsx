/** The white rounded panel used across the page. */
export function Card({ label, className = "", children }: { label?: string; className?: string; children: React.ReactNode }) {
  return (
    <section aria-label={label} className={`flex flex-col rounded-2xl border border-line bg-white p-4 sm:p-6 ${className}`}>
      {children}
    </section>
  );
}
