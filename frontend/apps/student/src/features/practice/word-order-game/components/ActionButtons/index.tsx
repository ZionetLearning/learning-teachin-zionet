import { useTranslation } from "react-i18next";
import { Button } from "@ui-components";
import { useStyles } from "./style";
interface ActionButtonsProps {
  loading: boolean;
  handleNextClick: () => void;
  handleCheck: () => void;
  handleReset: () => void;
}

export const ActionButtons = ({
  loading,
  handleNextClick,
  handleCheck,
  handleReset,
}: ActionButtonsProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  return (
    <div className={classes.actionButtons}>
      <Button data-testid="wog-check" onClick={handleCheck}>
        {t("pages.wordOrderGame.check")}
      </Button>
      <Button data-testid="wog-reset" onClick={handleReset}>
        {t("pages.wordOrderGame.reset")}
      </Button>
      <Button
        data-testid="wog-next"
        disabled={loading}
        onClick={handleNextClick}
      >
        {t("pages.wordOrderGame.next")}
      </Button>
    </div>
  );
};
