import { Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
export const HomePage = () => {
  const { t } = useTranslation();
  return (
    <>
      <Typography variant="h4" gutterBottom>
        {t("pages.home.title")}
      </Typography>
      <Typography>{t("pages.home.subTitle")}</Typography>
    </>
  );
};
