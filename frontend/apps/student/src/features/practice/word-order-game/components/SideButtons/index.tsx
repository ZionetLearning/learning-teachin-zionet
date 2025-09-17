import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
interface SideButtonsProps {
  loading: boolean;
  handleNextClick: () => void;
  handleCheck: () => void;
  handleReset: () => void;
}

export const SideButtons = ({
  loading,
  handleNextClick,
  handleCheck,
  handleReset,
}: SideButtonsProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  return (
    <div className={classes.sideButtons}>
      <button data-testid="wog-reset" onClick={handleReset}>
        {t("pages.wordOrderGame.reset")}
      </button>
      <button data-testid="wog-check" onClick={handleCheck}>
        {t("pages.wordOrderGame.check")}
      </button>
      <button
        data-testid="wog-next"
        disabled={loading}
        onClick={handleNextClick}
      >
        {t("pages.wordOrderGame.next")}
      </button>
    </div>
  );
};
