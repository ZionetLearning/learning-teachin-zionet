import {
  forwardRef,
  type ComponentPropsWithoutRef,
  type ElementRef,
} from "react";
import * as SeparatorPrimitive from "@radix-ui/react-separator";

import { cn } from "@ui-shadcn-components/utils";
import { separatorBaseStyles, separatorHorizontal, separatorVertical } from "./style";

const Separator = forwardRef<
  ElementRef<typeof SeparatorPrimitive.Root>,
  ComponentPropsWithoutRef<typeof SeparatorPrimitive.Root>
>(({ className, orientation = "horizontal", decorative = true, ...props }, ref) => (
  <SeparatorPrimitive.Root
    ref={ref}
    decorative={decorative}
    orientation={orientation}
    className={cn(
      separatorBaseStyles,
      orientation === "horizontal" ? separatorHorizontal : separatorVertical,
      className,
    )}
    {...props}
  />
));
Separator.displayName = SeparatorPrimitive.Root.displayName;

export { Separator };

