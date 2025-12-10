import { type HTMLAttributes } from "react";

import { cn } from "@ui-shadcn-components/utils";
import { cva, type VariantProps } from "class-variance-authority";

const badgeBaseStyles =
  "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2";

const badgeVariantStyles = {
  variant: {
    default: "border-transparent bg-primary text-primary-foreground hover:bg-primary/80",
    secondary: "border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80",
    destructive: "border-transparent bg-destructive text-destructive-foreground hover:bg-destructive/80",
    outline: "text-foreground",
  },
} as const;

const badgeDefaultVariants = {
  variant: "default" as const,
};

const badgeVariants = cva(badgeBaseStyles, {
  variants: badgeVariantStyles,
  defaultVariants: badgeDefaultVariants,
});

type BadgeVariants = VariantProps<typeof badgeVariants>;

export interface BadgeProps extends HTMLAttributes<HTMLDivElement>, BadgeVariants {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return <div className={cn(badgeVariants({ variant }), className)} {...props} />;
}

export { Badge, badgeVariants };
