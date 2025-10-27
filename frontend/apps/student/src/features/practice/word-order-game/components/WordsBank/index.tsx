import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

interface WordsBankProps {
  loading: boolean;
  error: string | null;
  shuffledSentence: string[];
  handleChooseWord: (word: string) => void;
}

export const WordsBank = ({
  loading,
  error,
  shuffledSentence,
  handleChooseWord,
}: WordsBankProps) => {
  const { t } = useTranslation();
  const classes = useStyles({ isEmpty: shuffledSentence.length === 0 });

  return (
    <div className={classes.bankWrapper} dir="rtl">
      <div className={classes.bankHeader}>
        {t("pages.wordOrderGame.wordBankTitle")}
      </div>

      <div className={classes.wordsBank} data-testid="wog-bank">
        {loading && <div>{t("pages.wordOrderGame.loading")}</div>}
        {error && <div className={classes.errorText}>{error}</div>}
        {!loading &&
          !error &&
          shuffledSentence.map((w, i) => (
            <button
              key={`b-${w}-${i}`}
              className={classes.bankWord}
              onClick={() => handleChooseWord(w)}
            >
              {w}
            </button>
          ))}
      </div>

      <div className={classes.bankHint}>
        {t("pages.wordOrderGame.wordBankHint")}
      </div>
    </div>
  );
};
