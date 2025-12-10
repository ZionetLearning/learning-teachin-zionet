import { cva, type VariantProps } from "class-variance-authority";

export const alertBaseStyles =
  "relative w-full rounded-lg border p-4 [&>svg~*]:pl-7 [&>svg+div]:translate-y-[-3px] [&>svg]:absolute [&>svg]:left-4 [&>svg]:top-4 [&>svg]:text-foreground";

export const alertVariantStyles = {
  variant: {
    default: "bg-background text-foreground",
    destructive: "border-destructive/50 text-destructive dark:border-destructive [&>svg]:text-destructive",
  },
} as const;

export const alertDefaultVariants = {
  variant: "default" as const,
};

export const alertVariants = cva(alertBaseStyles, {
  variants: alertVariantStyles,
  defaultVariants: alertDefaultVariants,
});

export type AlertVariants = VariantProps<typeof alertVariants>;

export const alertTitleStyles = "mb-1 font-medium leading-none tracking-tight";

export const alertDescriptionStyles = "text-sm [&_p]:leading-relaxed";

