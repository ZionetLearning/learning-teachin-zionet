import { Button as MuiButton } from "@mui/material";

export interface ButtonProps {
  children: React.ReactNode;
  disabled?: boolean;
  type?: "button" | "submit" | "reset" | undefined;
  variant?: "text" | "outlined" | "contained";
  sx?: object;
  size?: "small" | "medium" | "large" | undefined;
  onClick?: () => void;
  
}

export const Button = ({
  children,
  disabled = false,
  type,
  onClick,
  variant = "contained",
  sx,
  size = undefined
}: ButtonProps) => {

  return (
    <MuiButton
      type = {type}
      variant={variant}
      disabled={disabled}
      sx={sx ?? undefined}
      size={size}
      onClick={onClick}
    >
      {children}
    </MuiButton>
  );
};