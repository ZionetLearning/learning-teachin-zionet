import { useTranslation } from "react-i18next";
import { Button, Box, Typography } from "@mui/material";
import { useStyles } from "./style";
import { DifficultyLevel } from "@student/types";
import { GameConfig, GameConfigModal } from "@ui-components";

interface GameSetupPanelProps {
  configModalOpen: boolean;
  setConfigModalOpen: (open: boolean) => void;
  handleConfigConfirm: (config: GameConfig) => void;
  getDifficultyLabel: (
    level: DifficultyLevel,
    t: (key: string) => string,
  ) => string;
}

export const GameSetupPanel = ({
  configModalOpen,
  setConfigModalOpen,
  handleConfigConfirm,
  getDifficultyLabel,
}: GameSetupPanelProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  return (
    <>
      <Box
        data-testid="typing-level-selection"
        className={classes.welcomeContainer}
      >
        <Typography
          className={classes.welcomeText}
          variant="body1"
          color="text.secondary"
        >
          {t("pages.wordOrderGame.welcome.description")}
        </Typography>
        <Button
          data-testid="typing-configure-button"
          className={classes.welcomeButton}
          variant="contained"
          size="large"
          onClick={() => setConfigModalOpen(true)}
        >
          {t("pages.wordOrderGame.welcome.configure")}
        </Button>
      </Box>

      <GameConfigModal
        open={configModalOpen}
        onClose={() => setConfigModalOpen(false)}
        onConfirm={handleConfigConfirm}
        getDifficultyLevelLabel={getDifficultyLabel}
      />
    </>
  );
};
