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
  const classes = useStyles();

  return (
    <div className={classes.wordsBank} dir="rtl" data-testid="wog-bank">
      {loading && <div>{t("pages.wordOrderGame.loading")}</div>}
      {error && <div style={{ color: "red" }}>{error}</div>}
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
  );
};
