import { useState, useMemo, useEffect } from "react";
import { useTranslation } from "react-i18next";
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  CircularProgress,
  TablePagination,
  Box,
  Paper,
  Tooltip,
  ToggleButton,
  ToggleButtonGroup,
} from "@mui/material";

import { useAuth } from "@app-providers";
import { DifficultyChip, StatusChip, SuccessRateChip } from "@ui-components";

import {
  useGetGameHistorySummary,
  useGetGameHistoryDetailed,
} from "@student/api";
import type {
  GameHistorySummaryItem,
  GameHistorySummaryResponse,
  GameHistoryDetailedItem,
  GameHistoryDetailedResponse,
} from "@student/api";

import { useStyles } from "./style";

type ViewMode = "summary" | "detailed";

export const PracticeHistory = () => {
  const { user } = useAuth();
  const { t, i18n } = useTranslation();
  const classes = useStyles();

  const studentId = user?.userId ?? "";
  const isHebrew = i18n.language === "he";

  const [view, setView] = useState<ViewMode>("summary");
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const apiPage = useMemo(() => page + 1, [page]);

  // reset pagination when switching view
  useEffect(() => {
    setPage(0);
  }, [view, rowsPerPage]);

  const {
    data: summaryData,
    isLoading: summaryLoading,
    isFetching: summaryFetching,
    error: summaryError,
  } = useGetGameHistorySummary({
    studentId,
    page: apiPage,
    pageSize: rowsPerPage,
    enabled: view === "summary",
  });

  const {
    data: detailedData,
    isLoading: detailedLoading,
    isFetching: detailedFetching,
    error: detailedError,
  } = useGetGameHistoryDetailed({
    studentId,
    page: apiPage,
    pageSize: rowsPerPage,
    enabled: view === "detailed",
  });

  const activeData =
    view === "summary"
      ? (summaryData as GameHistorySummaryResponse | undefined)
      : (detailedData as GameHistoryDetailedResponse | undefined);

  const isLoading = view === "summary" ? summaryLoading : detailedLoading;
  const isFetching = view === "summary" ? summaryFetching : detailedFetching;
  const error = view === "summary" ? summaryError : detailedError;

  const items = activeData?.items ?? [];
  const total = activeData?.totalCount ?? 0;

  const handleChangePage = (_: unknown, newPage: number) => setPage(newPage);

  const handleChangeRowsPerPage = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    setRowsPerPage(parseInt(e.target.value, 10));
    setPage(0);
  };

  const handleViewChange = (_: unknown, next: ViewMode | null) => {
    if (next) setView(next);
  };

  const formatDateTime = (iso?: string) => {
    if (!iso) return "-";
    const date = new Date(iso);

    const options: Intl.DateTimeFormatOptions = {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      second: isHebrew ? undefined : "2-digit",
      hour12: !isHebrew, // AM/PM only in English
    };

    return new Intl.DateTimeFormat(
      isHebrew ? "he-IL" : "en-US",
      options,
    ).format(date);
  };

  if (error) {
    return (
      <Typography color="error" sx={{ textAlign: "center", mt: 3 }}>
        {t("pages.practiceHistory.failedToLoad")}
      </Typography>
    );
  }

  return (
    <Box>
      <Box className={classes.headerWrapper} sx={{ py: 3 }}>
        <Typography className={classes.title}>
          {t("pages.practiceHistory.title")}
        </Typography>
        <Typography className={classes.description} sx={{ mt: 0.5 }}>
          {t("pages.practiceHistory.description")}
        </Typography>

        <Box className={classes.toggleGroupWrapper}>
          <ToggleButtonGroup
            exclusive
            size="small"
            value={view}
            onChange={handleViewChange}
            className={classes.toggleGroup}
          >
            <ToggleButton value="summary">
              {t("pages.practiceHistory.summary")}
            </ToggleButton>
            <ToggleButton value="detailed">
              {t("pages.practiceHistory.detailed")}
            </ToggleButton>
          </ToggleButtonGroup>

          {isFetching && (
            <CircularProgress size={18} sx={{ color: "#7c4dff" }} />
          )}
        </Box>
      </Box>

      <Box className={classes.tableWrapper}>
        <Paper elevation={3} className={classes.paperWrapper}>
          {isLoading ? (
            <Box p={4} display="flex" justifyContent="center">
              <CircularProgress />
            </Box>
          ) : items.length === 0 ? (
            <Box p={4} textAlign="center">
              <Typography variant="h6" sx={{ mb: 1 }}>
                {t("pages.practiceHistory.noMistakes")}
              </Typography>
            </Box>
          ) : (
            <>
              <TableContainer className={classes.tableContainer}>
                <Table stickyHeader>
                  {view === "summary" ? (
                    <TableHead>
                      <TableRow>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.practiceName")}
                        </TableCell>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.difficulty")}
                        </TableCell>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.attemptsCount")}
                        </TableCell>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.totalSuccesses")}
                        </TableCell>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.totalFailures")}
                        </TableCell>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.successRate")}
                        </TableCell>
                      </TableRow>
                    </TableHead>
                  ) : (
                    <TableHead>
                      <TableRow>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.practiceName")}
                        </TableCell>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.difficulty")}
                        </TableCell>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.yourLastAnswer")}
                        </TableCell>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.status")}
                        </TableCell>
                        <TableCell className={classes.th}>
                          {t("pages.practiceHistory.date")}
                        </TableCell>
                      </TableRow>
                    </TableHead>
                  )}

                  <TableBody>
                    {view === "summary"
                      ? (items as GameHistorySummaryItem[]).map(
                          (item, idx: number) => {
                            const successRate =
                              item.attemptsCount > 0
                                ? Math.round(
                                    (item.totalSuccesses / item.attemptsCount) *
                                      100,
                                  )
                                : 0;
                            return (
                              <TableRow key={`s-${apiPage}-${idx}`} hover>
                                <TableCell className={classes.td}>
                                  <Typography fontWeight={600}>
                                    {t(
                                      `pages.practiceHistory.gameType.${item.gameType}`,
                                    )}
                                  </Typography>
                                </TableCell>
                                <TableCell className={classes.td}>
                                  <DifficultyChip level={item.difficulty} />
                                </TableCell>
                                <TableCell
                                  className={classes.td}
                                  align="center"
                                >
                                  {item.attemptsCount}
                                </TableCell>
                                <TableCell
                                  className={classes.td}
                                  align="center"
                                >
                                  {item.totalSuccesses}
                                </TableCell>
                                <TableCell
                                  className={classes.td}
                                  align="center"
                                >
                                  {item.totalFailures}
                                </TableCell>
                                <TableCell
                                  className={classes.td}
                                  align="center"
                                >
                                  <SuccessRateChip value={successRate} />
                                </TableCell>
                              </TableRow>
                            );
                          },
                        )
                      : (items as GameHistoryDetailedItem[]).map(
                          (item, idx: number) => {
                            const lastAnswer = item.givenAnswer?.length
                              ? item.givenAnswer.join(" ")
                              : "-";
                            return (
                              <TableRow key={`d-${apiPage}-${idx}`} hover>
                                <TableCell className={classes.td}>
                                  <Typography fontWeight={600}>
                                    {t(
                                      `pages.practiceHistory.gameType.${item.gameType}`,
                                    )}
                                  </Typography>
                                </TableCell>
                                <TableCell className={classes.td}>
                                  <DifficultyChip level={item.difficulty} />
                                </TableCell>
                                <TableCell className={classes.td}>
                                  <Tooltip title={lastAnswer}>
                                    <Box className={classes.lastAnswerBox}>
                                      {lastAnswer}
                                    </Box>
                                  </Tooltip>
                                </TableCell>
                                <TableCell className={classes.td}>
                                  <StatusChip
                                    text={
                                      item.status === "Success"
                                        ? t("pages.practiceHistory.succeeded")
                                        : t("pages.practiceHistory.failed")
                                    }
                                  />
                                </TableCell>
                                <TableCell className={classes.td}>
                                  {formatDateTime(item.createdAt)}
                                </TableCell>
                              </TableRow>
                            );
                          },
                        )}
                  </TableBody>
                </Table>
              </TableContainer>

              <TablePagination
                className={classes.tablePaginationWrapper}
                component="div"
                count={total}
                page={page}
                onPageChange={handleChangePage}
                rowsPerPage={rowsPerPage}
                onRowsPerPageChange={handleChangeRowsPerPage}
                rowsPerPageOptions={[5, 10]}
              />
            </>
          )}
        </Paper>
      </Box>
    </Box>
  );
};
