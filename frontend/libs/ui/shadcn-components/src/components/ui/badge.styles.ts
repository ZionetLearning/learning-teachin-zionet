import { cva, type VariantProps } from "class-variance-authority";

export const badgeBaseStyles =
  "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2";

export const badgeVariantStyles = {
  variant: {
    default: "border-transparent bg-primary text-primary-foreground hover:bg-primary/80",
    secondary: "border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80",
    destructive: "border-transparent bg-destructive text-destructive-foreground hover:bg-destructive/80",
    outline: "text-foreground",
  },
} as const;

export const badgeDefaultVariants = {
  variant: "default" as const,
};

export const badgeVariants = cva(badgeBaseStyles, {
  variants: badgeVariantStyles,
  defaultVariants: badgeDefaultVariants,
});

export type BadgeVariants = VariantProps<typeof badgeVariants>;

