const VARIANTS = {
  primary: "border-0 bg-accent font-bold text-white",
  secondary: "border border-field bg-white font-semibold text-ink",
};

/** A standard action button. */
export function Button({ variant = "primary", className = "", ...props }: { variant?: keyof typeof VARIANTS } & React.ButtonHTMLAttributes<HTMLButtonElement>) {
  return <button type="button" className={`min-h-12 rounded-xl px-5 text-[15px] ${VARIANTS[variant]} ${className}`} {...props} />;
}
