import { Button as MuiButton } from "@mui/material";
import { useStyles } from "./style";

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
  const classes = useStyles();

  return (
    <MuiButton
      type="submit"
      variant="contained"
      className={classes.button}
      disabled={disabled}
      onClick={onClick}
    >
      {children}
    </MuiButton>
  );
};