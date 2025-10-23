import { Button as MuiButton } from "@mui/material";

export interface ButtonProps {
  children: React.ReactNode;
  disabled?: boolean;
  type?: "button" | "submit" | "reset" | undefined;
  variant?: "text" | "outlined" | "contained";
  sx?: object;
  size?: "small" | "medium" | "large" | undefined;
  onClick?: () => void;
  className?: string;
}

export const Button = ({
  children,
  disabled = false,
  type,
  onClick,
  variant = "contained",
  sx,
  size = undefined,
  className,
}: ButtonProps) => {
  return (
    <MuiButton
      type={type}
      variant={variant}
      disabled={disabled}
      sx={sx ?? undefined}
      size={size}
      onClick={onClick}
      className={className}
    >
      {children}
    </MuiButton>
  );
};
