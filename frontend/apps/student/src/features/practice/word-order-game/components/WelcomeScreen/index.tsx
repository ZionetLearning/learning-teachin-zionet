import { useTranslation } from "react-i18next";
import { Button, Box, Typography } from "@mui/material";
import { GameConfigModal, GameConfig } from "../modals";
import { useStyles } from "./style";
import { DifficultyLevel } from "@student/types";

interface WelcomeScreenProps {
  configModalOpen: boolean;
  setConfigModalOpen: (open: boolean) => void;
  handleConfigConfirm: (config: GameConfig) => void;
  getDifficultyLabel: (level: DifficultyLevel) => string;
}

export const WelcomeScreen = ({
  configModalOpen,
  setConfigModalOpen,
  handleConfigConfirm,
  getDifficultyLabel,
}: WelcomeScreenProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  return (
    <>
      <Box className={classes.welcomeContainer}>
        <Typography
          className={classes.welcomeText}
          variant="body1"
          color="text.secondary"
        >
          {t("pages.wordOrderGame.welcome.description")}
        </Typography>
        <Button
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
