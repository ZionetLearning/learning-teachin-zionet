import { forwardRef, type ComponentProps } from "react";

import { cn } from "@ui-shadcn-components/utils";
import { inputStyles } from "./input.styles";

const Input = forwardRef<HTMLInputElement, ComponentProps<"input">>(({ className, type, ...props }, ref) => {
  return (
    <input
      type={type}
      className={cn(inputStyles, className)}
      ref={ref}
      {...props}
    />
  );
});
Input.displayName = "Input";

export { Input };
