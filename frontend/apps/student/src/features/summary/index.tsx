import { useState } from "react";
import { Box, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useAuth } from "@app-providers";
import { useGetPeriodOverview } from "../../api";
import { PeriodOverview, PeriodSelector } from "../../components";
import { DetailedSummary } from "./components";
import { useStyles } from "./style";

const getStartOfWeek = (date: Date): Date => {
  const d = new Date(date);
  const day = d.getDay();
  const diff = d.getDate() - day;  
  return new Date(d.setDate(diff));
};

const getEndOfWeek = (date: Date): Date => {
  const start = getStartOfWeek(date);
  const end = new Date(start);
  end.setDate(start.getDate() + 6);
  return end;
};

export const SummaryDashboard = () => {
  const { t } = useTranslation();
  const { user } = useAuth();
  const classes = useStyles();
  const [startDate, setStartDate] = useState(() => getStartOfWeek(new Date()));
  const [endDate, setEndDate] = useState(() => getEndOfWeek(new Date()));

  const { data, isLoading } = useGetPeriodOverview({
    userId: user?.userId || "",
    startDate: startDate.toISOString(),
    endDate: endDate.toISOString(),
  });

  const handleDateRangeChange = (newStart: Date, newEnd: Date) => {
    setStartDate(newStart);
    setEndDate(newEnd);
  };

  if (!user) {
    return (
      <Box>
        <Typography>{t("common.loading")}</Typography>
      </Box>
    );
  }

  return (
    <Box className={classes.pageContainer}>
      <Box className={classes.headerSection}>
        <Box className={classes.headerColumn}>
          <PeriodOverview summary={data?.summary} isLoading={isLoading} />
        </Box>
        <Box className={classes.headerColumn}>
          <PeriodSelector
            startDate={startDate}
            endDate={endDate}
            onDateRangeChange={handleDateRangeChange}
          />
        </Box>
      </Box>

      <Box className={classes.mainContent}>
        <DetailedSummary
          userId={user.userId}
          startDate={startDate.toISOString()}
          endDate={endDate.toISOString()}
        />
      </Box>
    </Box>
  );
};
