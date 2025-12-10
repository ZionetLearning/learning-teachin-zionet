import { type HTMLAttributes } from "react";

import { cn } from "@ui-shadcn-components/utils";
import { badgeVariants, type BadgeVariants } from "./badge.styles";

export interface BadgeProps extends HTMLAttributes<HTMLDivElement>, BadgeVariants {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return <div className={cn(badgeVariants({ variant }), className)} {...props} />;
}

export { Badge, badgeVariants };
