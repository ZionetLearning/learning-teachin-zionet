import { cva, type VariantProps } from "class-variance-authority";

export const labelBaseStyles = "text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70";

export const labelVariants = cva(labelBaseStyles);

export type LabelVariants = VariantProps<typeof labelVariants>;

