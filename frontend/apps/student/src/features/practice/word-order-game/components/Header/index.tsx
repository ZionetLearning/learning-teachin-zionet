import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
export const Header = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  return <h1 className={classes.header}>{t("pages.wordOrderGame.title")}</h1>;
};
