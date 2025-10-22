import { useTranslation } from "react-i18next";
import { Button } from "@ui-components";
import { useStyles } from "./style";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import ReplayIcon from "@mui/icons-material/Replay";
import SkipNextIcon from "@mui/icons-material/SkipNext";

interface ActionButtonsProps {
  loading: boolean;
  showNext: boolean;
  handleNextClick: () => void;
  handleCheck: () => void;
  handleReset: () => void;
}

export const ActionButtons = ({
  loading,
  showNext,
  handleNextClick,
  handleCheck,
  handleReset,
}: ActionButtonsProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  return (
    <div
      className={classes.actionsBar}
      role="group"
      aria-label={t("pages.wordOrderGame.actions")}
    >
      <Button
        data-testid="wog-check"
        onClick={handleCheck}
        aria-label={t("pages.wordOrderGame.check")}
        className={classes.btnCheck}
      >
        <CheckCircleIcon className={classes.btnIcon} />
        {t("pages.wordOrderGame.check")}
      </Button>

      <Button
        data-testid="wog-reset"
        onClick={handleReset}
        aria-label={t("pages.wordOrderGame.reset")}
        className={classes.btnReset}
      >
        <ReplayIcon className={classes.btnIcon} />
        {t("pages.wordOrderGame.reset")}
      </Button>

      {showNext && (
        <Button
          data-testid="wog-next"
          disabled={loading}
          onClick={handleNextClick}
          aria-label={t("pages.wordOrderGame.next")}
          className={classes.btnNext}
        >
          <SkipNextIcon className={classes.btnIcon} />
          {t("pages.wordOrderGame.next")}
        </Button>
      )}
    </div>
  );
};
