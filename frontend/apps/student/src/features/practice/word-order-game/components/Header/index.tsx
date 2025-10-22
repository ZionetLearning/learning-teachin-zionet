import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

export const Header = () => {
  const { t } = useTranslation();
  const classes = useStyles();

  return (
    <div className={classes.headerWrapper}>
      <h1 className={classes.header}>{t("pages.wordOrderGame.title")}</h1>
      <div className={classes.underline} />
    </div>
  );
};
