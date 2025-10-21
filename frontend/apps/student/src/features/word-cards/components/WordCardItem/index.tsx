import {
  Box,
  Checkbox,
  FormControlLabel,
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

  const title = card.isLearned
    ? t("pages.wordCards.markAsUnlearned")
    : t("pages.wordCards.markAsLearned");

  return (
    <Box className={classes.card} role="group" aria-label="word card">
      <Box className={classes.innerCard}>
        <Box className={classes.wordGroup}>
          <Typography className={classes.hebrew} dir="rtl">
            {card.hebrew}
          </Typography>
          <Typography className={classes.english}>{card.english}</Typography>
        </Box>

        <Tooltip title={title}>
          <FormControlLabel
            className={classes.learnControl}
            control={
              <Checkbox
                checked={card.isLearned}
                onChange={toggle}
                disabled={setLearned.isPending}
                icon={
                  <RadioButtonUncheckedIcon className={classes.learnIconIdle} />
                }
                checkedIcon={
                  <CheckCircleIcon className={classes.learnIconActive} />
                }
              />
            }
            label={
              <Typography className={classes.learnLabel}>
                {t("pages.wordCards.learned")}
              </Typography>
            }
          />
        </Tooltip>
      </Box>
    </Box>
  );
};
