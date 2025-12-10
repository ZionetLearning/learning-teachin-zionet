import { forwardRef, type HTMLAttributes } from "react";

import { cn } from "@ui-shadcn-components/utils";
import { cva, type VariantProps } from "class-variance-authority";

const alertBaseStyles =
  "relative w-full rounded-lg border p-4 [&>svg~*]:pl-7 [&>svg+div]:translate-y-[-3px] [&>svg]:absolute [&>svg]:left-4 [&>svg]:top-4 [&>svg]:text-foreground";

const alertVariantStyles = {
  variant: {
    default: "bg-background text-foreground",
    destructive: "border-destructive/50 text-destructive dark:border-destructive [&>svg]:text-destructive",
  },
} as const;

const alertDefaultVariants = {
  variant: "default" as const,
};

const alertVariants = cva(alertBaseStyles, {
  variants: alertVariantStyles,
  defaultVariants: alertDefaultVariants,
});

type AlertVariants = VariantProps<typeof alertVariants>;

const alertTitleStyles = "mb-1 font-medium leading-none tracking-tight";

const alertDescriptionStyles = "text-sm [&_p]:leading-relaxed";

const Alert = forwardRef<HTMLDivElement, HTMLAttributes<HTMLDivElement> & AlertVariants>(
  ({ className, variant, ...props }, ref) => (
    <div ref={ref} role="alert" className={cn(alertVariants({ variant }), className)} {...props} />
  ),
);
Alert.displayName = "Alert";

const AlertTitle = forwardRef<HTMLParagraphElement, HTMLAttributes<HTMLHeadingElement>>(
  ({ className, ...props }, ref) => (
    <h5 ref={ref} className={cn(alertTitleStyles, className)} {...props} />
  ),
);
AlertTitle.displayName = "AlertTitle";

const AlertDescription = forwardRef<HTMLParagraphElement, HTMLAttributes<HTMLParagraphElement>>(
  ({ className, ...props }, ref) => (
    <div ref={ref} className={cn(alertDescriptionStyles, className)} {...props} />
  ),
);
AlertDescription.displayName = "AlertDescription";

export { Alert, AlertTitle, AlertDescription };
