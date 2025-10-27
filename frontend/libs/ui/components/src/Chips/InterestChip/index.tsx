import { Chip } from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import { useStyles } from "./style";

export interface InterestChipProps {
  label: string;
  onDelete?: () => void;
  size?: "small" | "medium";
  color?: "default" | "primary" | "secondary";
}

export const InterestChip = ({
  label,
  onDelete,
  size = "small",
  color = "default",
}: InterestChipProps) => {
  const classes = useStyles();
  return (
    <Chip
      label={label}
      onDelete={onDelete}
      deleteIcon={<CloseIcon fontSize="small" />}
      size={size}
      color={color}
      className={classes.root}
    />
  );
};
