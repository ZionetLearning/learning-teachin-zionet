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
import { DifficultyChip, StatusChip } from "@ui-components";

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

  // Declare both queries; enable only the active one
  const {
    data: summaryData,
    isLoading: summaryLoading,
    isFetching: summaryFetching,
    error: summaryError,
  } = useGetGameHistorySummary({
    studentId,
    page: apiPage,
    pageSize: rowsPerPage,
    // @NOTE: if your hook doesn’t accept enabled, add it there; in the definitions you provided it already has enabled(Boolean(studentId))
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
  });

  // Gate queries via enabled flag inside the hooks’ implementation:
  // enabled: Boolean(studentId) && view === 'summary' / 'detailed'
  // If you haven't added that yet, do it in the hooks to avoid double-fetching.

  // pick active dataset/status
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
    try {
      // keep it simple; localize as needed
      return new Date(iso).toLocaleString();
    } catch {
      return iso;
    }
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
                          {t("pages.practiceHistory.attemptNumber")}
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
                                  {successRate}%
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
                                <TableCell
                                  className={classes.td}
                                  align="center"
                                >
                                  {item.attemptNumber ?? "-"}
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
