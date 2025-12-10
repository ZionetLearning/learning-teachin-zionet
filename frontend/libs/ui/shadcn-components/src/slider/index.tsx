import {
  forwardRef,
  type ComponentPropsWithoutRef,
  type ElementRef,
} from "react";
import * as SliderPrimitive from "@radix-ui/react-slider";

import { cn } from "@ui-shadcn-components/utils";
import {
  sliderRootStyles,
  sliderTrackStyles,
  sliderRangeStyles,
  sliderThumbStyles,
} from "./style";

const Slider = forwardRef<
  ElementRef<typeof SliderPrimitive.Root>,
  ComponentPropsWithoutRef<typeof SliderPrimitive.Root>
>(({ className, ...props }, ref) => (
  <SliderPrimitive.Root
    ref={ref}
    className={cn(sliderRootStyles, className)}
    {...props}
  >
    <SliderPrimitive.Track className={sliderTrackStyles}>
      <SliderPrimitive.Range className={sliderRangeStyles} />
    </SliderPrimitive.Track>
    <SliderPrimitive.Thumb className={sliderThumbStyles} />
  </SliderPrimitive.Root>
));
Slider.displayName = SliderPrimitive.Root.displayName;

export { Slider };

