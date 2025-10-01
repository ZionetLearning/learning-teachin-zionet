import { Chip } from "@mui/material";
import { GameMistakeItem } from "@student/api";

export const DifficultyChip = ({
  level,
}: {
  level: GameMistakeItem["difficulty"];
}) => {
  const map: Record<string, "success" | "warning" | "error" | "default"> = {
    easy: "success",
    medium: "warning",
    hard: "error",
  };
  return (
    <Chip
      size="small"
      label={level}
      color={map[level.toLowerCase()] ?? "default"}
    />
  );
};

export const StatusChip = ({ text }: { text: string }) => (
  <Chip size="small" label={text} color="warning" variant="outlined" />
);
