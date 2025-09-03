import { Button as MuiButton } from "@mui/material";

export interface ButtonProps {
  children: React.ReactNode;
  disabled?: boolean;
  type?: "button" | "submit" | "reset" | undefined;
  variant?: "text" | "outlined" | "contained";
  onClick?: () => void;
  
}

export const Button = ({
  children,
  disabled = false,
  type,
  onClick,
  variant = "contained",
}: ButtonProps) => {

  return (
    <MuiButton
      type = {type}
      variant={variant}
      disabled={disabled}
      onClick={onClick}
    >
      {children}
    </MuiButton>
  );
};