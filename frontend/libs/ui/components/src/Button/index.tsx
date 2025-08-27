import { Button as MuiButton } from "@mui/material";

export interface ButtonProps {
  children: React.ReactNode;
  disabled?: boolean;
  onClick?: () => void;
}

export const Button = ({
  children,
  disabled = false,
  onClick,
}: ButtonProps) => {

  return (
    <MuiButton
      type="submit"
      variant="contained"
      disabled={disabled}
      onClick={onClick}
    >
      {children}
    </MuiButton>
  );
};