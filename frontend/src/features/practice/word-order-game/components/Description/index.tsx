import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

export const Description = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  return (
    <div className={classes.description}>
      {t("pages.wordOrderGame.subTitle")}
    </div>
  );
};
