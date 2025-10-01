import { Chip } from "@mui/material";
import { useTranslation } from "react-i18next";
import { GameMistakeItem } from "@student/api";

export const DifficultyChip = ({
  level,
}: {
  level: GameMistakeItem["difficulty"];
}) => {
  const { t } = useTranslation();
  const levelLowerCase = level.toLowerCase();

  const map: Record<string, "success" | "warning" | "error" | "default"> = {
    easy: "success",
    medium: "warning",
    hard: "error",
  };
  return (
    <Chip
      size="small"
      label={t(`pages.wordOrderGame.difficulty.${levelLowerCase}`)}
      color={map[levelLowerCase] ?? "default"}
    />
  );
};

export const StatusChip = ({ text }: { text: string }) => (
  <Chip size="small" label={text} color="warning" variant="outlined" />
);
