import { Box, Button, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

type GameMode = "heb-to-eng" | "eng-to-heb";

interface ModeSelectionProps {
  onStartGame: (mode: GameMode) => void;
}

export const ModeSelection = ({ onStartGame }: ModeSelectionProps) => {
  const classes = useStyles();
  const { t } = useTranslation();

  return (
    <Box className={classes.container}>
      <Box className={classes.modeSelection}>
        <Typography className={classes.title}>
          {t("pages.wordCardsChallenge.title")}
        </Typography>
        <Typography className={classes.subtitle}>
          {t("pages.wordCardsChallenge.selectMode")}
        </Typography>
        <Box className={classes.modeButtons}>
          <Button
            variant="contained"
            className={classes.modeButton}
            onClick={() => onStartGame("heb-to-eng")}
          >
            {t("pages.wordCardsChallenge.hebToEng")}
          </Button>
          <Button
            variant="contained"
            className={classes.modeButton}
            onClick={() => onStartGame("eng-to-heb")}
          >
            {t("pages.wordCardsChallenge.engToHeb")}
          </Button>
        </Box>
      </Box>
    </Box>
  );
};
