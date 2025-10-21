import { useMemo, useState } from "react";
import {
  Box,
  Button,
  Checkbox,
  CircularProgress,
  FormControlLabel,
  Typography,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import { Trans, useTranslation } from "react-i18next";
import { useGetWordCards, type WordCard } from "@student/api";
import { AddWordCardDialog } from "@student/components";
import { WordCardItem } from "./components";
import { useStyles } from "./style";

const mockData: WordCard[] = [
  { cardId: "1", hebrew: "שלום", english: "hello", isLearned: false },
  { cardId: "2", hebrew: "תודה", english: "thank you", isLearned: true },
  { cardId: "3", hebrew: "אור", english: "light", isLearned: false },
  { cardId: "4", hebrew: "מים", english: "water", isLearned: false },
  { cardId: "5", hebrew: "ספר", english: "book", isLearned: true },
  { cardId: "6", hebrew: "אוכל", english: "food", isLearned: false },

];

export const WordCards = () => {
  const classes = useStyles();
  const { t } = useTranslation();

  const { data, isLoading, isError } = useGetWordCards();
  const [hideLearned, setHideLearned] = useState<boolean>(false);
  const [addOpen, setAddOpen] = useState<boolean>(false);

  // need to change from "mockData" to "data" when api is ready
  const filtered = useMemo(() => {
    const list = mockData ?? [];
    return hideLearned ? list.filter((c) => !c.isLearned) : list;
  }, [hideLearned]);

  return (
    <Box>
      <Box className={classes.headerWrapper}>
        <Typography className={classes.title}>
          {t("pages.wordCards.title")}
        </Typography>
        <Typography className={classes.description}>
          <strong>{t("pages.wordCards.buildYourOwnWordDescription")}</strong>
        </Typography>
        <Typography className={classes.description}>
          {t("pages.wordCards.addNewWordsDescription")}
        </Typography>

        <Typography className={classes.helperNote}>
          <Trans
            i18nKey="pages.wordCards.tipHelperNote"
            components={[<strong />, <strong />]}
          />
        </Typography>

        <Box className={classes.headerActions}>
          <FormControlLabel
            control={
              <Checkbox
                checked={hideLearned}
                onChange={(e) => setHideLearned(e.target.checked)}
              />
            }
            label={t("pages.wordCards.hideLearnedWords")}
          />
          <Button
            startIcon={<AddIcon />}
            variant="contained"
            className={classes.primaryBtn}
            onClick={() => setAddOpen(true)}
          >
            {t("pages.wordCards.addCard")}
          </Button>
        </Box>
      </Box>

      <Box className={classes.body}>
        {isLoading && !mockData ? (
          <Box className={classes.centerState}>
            <CircularProgress />
            <Typography> {t("pages.wordCards.loadingCards")}</Typography>
          </Box>
        ) : isError && !mockData ? (
          <Box className={classes.centerState}>
            <Typography color="error">
              {t("pages.wordCards.failedToLoad")}
            </Typography>
          </Box>
        ) : filtered.length === 0 ? (
          <Box className={classes.centerState}>
            <Typography> {t("pages.wordCards.noWordCardsYet")}</Typography>
            <Typography variant="body2" color="text.secondary">
              {t("pages.wordCards.clickAddCardOrSelect")}
            </Typography>
          </Box>
        ) : (
          <Box className={classes.grid}>
            {filtered.map((card) => (
              <WordCardItem key={card.cardId} card={card} />
            ))}
          </Box>
        )}
      </Box>

      {addOpen && (
        <AddWordCardDialog open={addOpen} onClose={() => setAddOpen(false)} />
      )}
    </Box>
  );
};
