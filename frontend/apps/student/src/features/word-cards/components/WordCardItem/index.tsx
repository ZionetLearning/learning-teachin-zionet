import {
  Box,
  Checkbox,
  FormControlLabel,
  IconButton,
  Tooltip,
  Typography,
} from "@mui/material";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import RadioButtonUncheckedIcon from "@mui/icons-material/RadioButtonUnchecked";
import { useSetWordCardLearned, type WordCard } from "@student/api";
import { useStyles } from "./style";
import { useTranslation } from "react-i18next";

export const WordCardItem = ({ card }: { card: WordCard }) => {
  const classes = useStyles();
  const { t } = useTranslation();

  const setLearned = useSetWordCardLearned();

  const toggle = () => {
    setLearned.mutate({ cardId: card.cardId, isLearned: !card.isLearned });
  };

  return (
    <Box className={classes.card}>
      <Box className={classes.cardTop}>
        <Box className={classes.wordGroup}>
          <Typography className={classes.hebrew}>{card.hebrew}</Typography>
          <Typography className={classes.english}>{card.english}</Typography>
        </Box>

        <Tooltip
          title={
            card.isLearned
              ? t("pages.wordCards.markAsUnlearned")
              : t("pages.wordCards.markAsLearned")
          }
        >
          <IconButton
            onClick={toggle}
            className={classes.learnBtn}
            disabled={setLearned.isPending}
            aria-label="toggle learned"
          >
            {card.isLearned ? (
              <CheckCircleIcon className={classes.learnIconActive} />
            ) : (
              <RadioButtonUncheckedIcon className={classes.learnIconIdle} />
            )}
          </IconButton>
        </Tooltip>
      </Box>

      <Box className={classes.cardFoot}>
        <FormControlLabel
          control={
            <Checkbox
              checked={card.isLearned}
              onChange={toggle}
              disabled={setLearned.isPending}
            />
          }
          label={t("pages.wordCards.learned")}
        />
      </Box>
    </Box>
  );
};
