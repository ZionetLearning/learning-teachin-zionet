import { forwardRef, type ComponentRef, type ComponentPropsWithoutRef } from "react";
import * as LabelPrimitive from "@radix-ui/react-label";

import { cn } from "@ui-shadcn-components/utils";
import { cva, type VariantProps } from "class-variance-authority";

const labelBaseStyles =
  "text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70";

const labelVariants = cva(labelBaseStyles);

type LabelVariants = VariantProps<typeof labelVariants>;

const Label = forwardRef<
  ComponentRef<typeof LabelPrimitive.Root>,
  ComponentPropsWithoutRef<typeof LabelPrimitive.Root> & LabelVariants
>(({ className, ...props }, ref) => (
  <LabelPrimitive.Root ref={ref} className={cn(labelVariants(), className)} {...props} />
));
Label.displayName = LabelPrimitive.Root.displayName;

export { Label };
