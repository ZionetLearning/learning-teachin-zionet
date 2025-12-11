import { Box, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

interface Props {
  userId: string;
  startDate: string;
  endDate: string;
}

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export const WordCardsSummary = (_props: Props) => {
  const { t } = useTranslation();
  const classes = useStyles();

  return (
    <Box className={classes.container}>
      <Typography variant="h6" color="text.secondary">
        {t("pages.summary.wordCards.comingSoon")}
      </Typography>
    </Box>
  );
};
