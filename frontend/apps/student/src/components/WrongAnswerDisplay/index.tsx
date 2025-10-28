import { useTranslation } from "react-i18next";
import { Typography, Box } from "@mui/material";
import { useStyles } from "./style";

interface WrongAnswerDisplayProps {
  wrongAnswer: string[] | string;
  label?: string;
  show?: boolean;
}

export const WrongAnswerDisplay = ({
  wrongAnswer,
  label,
  show = true,
}: WrongAnswerDisplayProps) => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();
  const isHebrew = i18n.language === "he" || i18n.language === "heb";

  if (!show || !wrongAnswer) {
    return null;
  }

  const wrongAnswerText = Array.isArray(wrongAnswer)
    ? wrongAnswer.join(" ")
    : wrongAnswer;

  if (!wrongAnswerText.trim()) {
    return null;
  }

  const displayLabel = label || t("pages.wordOrderGame.yourLastWrongAnswer");

  return (
    <Box className={classes.container}>
      <Typography
        variant="body2"
        color="text.secondary"
        className={classes.label}
      >
        {displayLabel}
      </Typography>
      <Typography
        variant="body1"
        className={classes.wrongAnswer}
        dir={isHebrew ? "rtl" : "ltr"}
      >
        {wrongAnswerText}
      </Typography>
    </Box>
  );
};
