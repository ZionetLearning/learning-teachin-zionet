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
import { useGetWordCards } from "@student/api";
import { AddWordCardDialog } from "@student/components";
import { WordCardItem } from "./components";
import { useThemeColors } from "@app-providers";
import { useStyles } from "./style";

export const WordCards = () => {
  const color = useThemeColors();
  const classes = useStyles(color);
  const { t, i18n } = useTranslation();

  const { data, isLoading, isError } = useGetWordCards();

  const isHebrew = i18n.language === "he";
  const [hideLearned, setHideLearned] = useState<boolean>(false);
  const [addOpen, setAddOpen] = useState<boolean>(false);

  const filtered = useMemo(() => {
    const list = data ?? [];
    return hideLearned ? list.filter((c) => !c.isLearned) : list;
  }, [data, hideLearned]);

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

        <Box className={classes.headerActions} dir={isHebrew ? "rtl" : "ltr"}>
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
            variant="contained"
            color="primary"
            className={classes.addCardBtn}
            onClick={() => setAddOpen(true)}
            startIcon={isHebrew ? undefined : <AddIcon />}
            endIcon={isHebrew ? <AddIcon /> : undefined}
            dir={isHebrew ? "rtl" : "ltr"}
          >
            {t("pages.wordCards.addCard")}
          </Button>
        </Box>
      </Box>

      <Box className={classes.body}>
        {isLoading ? (
          <Box className={classes.centerState}>
            <CircularProgress />
            <Typography> {t("pages.wordCards.loadingCards")}</Typography>
          </Box>
        ) : isError ? (
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
