import { useState } from "react";
import { Typography } from "@mui/material";
import { useTranslation } from "react-i18next";

export const HomePage = () => {
  const { t } = useTranslation();
  const [error, setErrorState] = useState<boolean>(false);
  return (
    <>
      <Typography variant="h4" gutterBottom>
        {t("pages.home.title")}
      </Typography>
      <Typography>{t("pages.home.subTitle")}</Typography>
      <button onClick={()=> setErrorState(true)}>Click</button>
      {error && <TestErrorComponent />}
    
    </>
  );
};

const TestErrorComponent = () => {
  throw new Error("Test error for error boundary");
};
