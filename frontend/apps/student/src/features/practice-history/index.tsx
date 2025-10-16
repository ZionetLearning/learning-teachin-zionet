import { useState } from "react";
import { useAuth } from "@app-providers";
import { useTranslation } from "react-i18next";
import {
  Box,
  Button,
  Typography,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  CircularProgress,
} from "@mui/material";
import {
  useGetGameHistorySummary,
  useGetGameHistoryDetailed,
} from "@student/api";
import { SummaryView } from "./components/summary-view";
import { DetailedView } from "./components/detailed-view";
import { useStyles } from "./style";

export const PracticeHistory = () => {
  const { user } = useAuth();
  const { t } = useTranslation();
  const studentId = user?.userId ?? "";

  const [viewMode, setViewMode] = useState<"summary" | "detailed">("summary");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const classes = useStyles();

  const { data: summaryData, isLoading: summaryLoading } =
    useGetGameHistorySummary({
      studentId,
      page,
      pageSize,
    });

  const { data: detailedData, isLoading: detailedLoading } =
    useGetGameHistoryDetailed({
      studentId,
      page,
      pageSize,
    });

  const isLoading = summaryLoading || detailedLoading;

  const currentData = viewMode === "summary" ? summaryData : detailedData;

  if (viewMode === "summary" && !summaryData && !summaryLoading) return null;
  if (viewMode === "detailed" && !detailedData && !detailedLoading) return null;

  return (
    <Box className={classes.container} dir="rtl">
      <Box mb={3}>
        <Typography variant="h4" component="h1" gutterBottom>
          {t("pages.practiceHistory.title")}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t("pages.practiceHistory.subtitle")}
        </Typography>
      </Box>

      <Box className={classes.buttonsContainer}>
        <Box className={classes.innerButtonsContainer}>
          <Button
            variant={viewMode === "summary" ? "contained" : "outlined"}
            onClick={() => {
              setViewMode("summary");
              setPage(1); // Reset to page 1 when switching modes
            }}
          >
            {t("pages.practiceHistory.summaryMode")}
          </Button>
          <Button
            variant={viewMode === "detailed" ? "contained" : "outlined"}
            onClick={() => {
              setViewMode("detailed");
              setPage(1); // Reset to page 1 when switching modes
            }}
          >
            {t("pages.practiceHistory.detailedMode")}
          </Button>
        </Box>

        <FormControl size="small" className={classes.formControl}>
          <InputLabel>{t("pages.practiceHistory.perPage")}</InputLabel>
          <Select
            value={pageSize}
            label={t("pages.practiceHistory.perPage")}
            onChange={(e) => {
              setPageSize(Number(e.target.value));
              setPage(1); // Reset to page 1 when changing page size
            }}
          >
            <MenuItem value={5}>5</MenuItem>
            <MenuItem value={10}>10</MenuItem>
            <MenuItem value={20}>20</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {isLoading ? (
        <Box className={classes.loadingContainer}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          {viewMode === "summary" ? (
            <SummaryView
              summaryData={summaryData}
            />
          ) : (
            <DetailedView detailedData={detailedData} />
          )}

          {currentData && currentData.totalCount > pageSize && (
            <Box className={classes.navigationButtons}>
              <Button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
              >
                {t("pages.practiceHistory.previous")}
              </Button>

              <Typography variant="body2">
                {t("pages.practiceHistory.pageInfo", {
                  current: page,
                  total: Math.ceil(currentData.totalCount / pageSize),
                })}
              </Typography>

              <Button
                onClick={() => setPage((p) => p + 1)}
                disabled={!currentData.hasNextPage}
              >
                {t("pages.practiceHistory.next")}
              </Button>
            </Box>
          )}
        </>
      )}
    </Box>
  );
};