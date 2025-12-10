import { type HTMLAttributes } from "react";

import { cn } from "@ui-shadcn-components/utils";
import { skeletonStyles } from "./style";

function Skeleton({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn(skeletonStyles, className)} {...props} />;
}

export { Skeleton };

