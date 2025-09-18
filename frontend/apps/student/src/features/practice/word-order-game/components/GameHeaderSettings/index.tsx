import { useTranslation } from "react-i18next";
import { Box, Button, Typography } from "@mui/material";
import { Settings } from "@mui/icons-material";
import { GameConfig } from "../modals";
import { DifficultyLevel } from "@student/types";
import { useStyles } from "./style";

interface GameHeaderSettingsProps {
  gameConfig: GameConfig;
  currentSentenceIndex: number;
  sentenceCount: number;
  isHebrew: boolean;
  handleConfigChange: () => void;
  getDifficultyLabel: (level: DifficultyLevel) => string;
}
export const GameHeaderSettings = ({
  gameConfig,
  currentSentenceIndex,
  sentenceCount,
  isHebrew,
  handleConfigChange,
  getDifficultyLabel,
}: GameHeaderSettingsProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  return (
    <Box className={classes.gameHeader}>
      <Box className={classes.gameHeaderInfo}>
        <Typography variant="body2" color="text.secondary">
          {t("pages.wordOrderGame.current.difficulty")}:{" "}
          {getDifficultyLabel(gameConfig.difficulty)}
          {" | "}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t("pages.wordOrderGame.current.nikud")}:{" "}
          {gameConfig.nikud
            ? t("pages.wordOrderGame.yes")
            : t("pages.wordOrderGame.no")}
          {" | "}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t("pages.wordOrderGame.current.sentence")}:{" "}
          {currentSentenceIndex + 1}/{sentenceCount}
        </Typography>
      </Box>
      <Button
        variant="outlined"
        size="small"
        startIcon={<Settings />}
        className={
          isHebrew
            ? classes.settingsButtonHebrew
            : classes.settingsButtonEnglish
        }
        onClick={handleConfigChange}
      >
        {t("pages.wordOrderGame.settings")}
      </Button>
    </Box>
  );
};
