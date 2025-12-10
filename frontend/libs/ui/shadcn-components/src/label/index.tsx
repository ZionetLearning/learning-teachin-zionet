import { forwardRef, type ElementRef, type ComponentPropsWithoutRef } from "react";
import * as LabelPrimitive from "@radix-ui/react-label";

import { cn } from "@ui-shadcn-components/utils";
import { labelVariants, type LabelVariants } from "./label.styles";

const Label = forwardRef<
  ElementRef<typeof LabelPrimitive.Root>,
  ComponentPropsWithoutRef<typeof LabelPrimitive.Root> & LabelVariants
>(({ className, ...props }, ref) => (
  <LabelPrimitive.Root ref={ref} className={cn(labelVariants(), className)} {...props} />
));
Label.displayName = LabelPrimitive.Root.displayName;

export { Label };
