import { Chip } from "@mui/material";

export const StatusChip = ({ text }: { text: string }) => (
  <Chip size="small" label={text} color="warning" variant="outlined" />
);
