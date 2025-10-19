import { Chip } from "@mui/material";

type MUIColor = "success" | "error" | "warning";

const TEXT_TO_COLOR: Record<string, MUIColor> = {
  // English
  Succeeded: "success",
  Failed: "error",
  "Try Again!": "warning",
  // Hebrew
  הצלחה: "success",
  כישלון: "error",
  "נסה שוב": "warning",
};

interface StatusChipProps {
  text: string;
}

export const StatusChip = ({ text }: StatusChipProps) => {
  const normalized = text.replace(/\s+/g, " ").trim();
  const color = TEXT_TO_COLOR[normalized] ?? "warning";
  return <Chip size="small" label={text} color={color} variant="outlined" />;
};
