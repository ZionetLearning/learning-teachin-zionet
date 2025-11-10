import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  CircularProgress,
  TablePagination,
  Tooltip,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { DifficultyChip, StatusChip } from "@ui-components";
import { useGetGameHistoryDetailed } from "@student/api";
import type { GameHistoryDetailedItem } from "@student/api";
import { useStyles } from "./style";

export const DetailedTable = ({
  studentId,
  page,
  rowsPerPage,
  onPageChange,
  onRowsPerPageChange,
  isHebrew,
}: {
  studentId: string;
  page: number;
  rowsPerPage: number;
  onPageChange: (p: number) => void;
  onRowsPerPageChange: (r: number) => void;
  isHebrew: boolean;
}) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const { data, isLoading, error } = useGetGameHistoryDetailed({
    studentId,
    page,
    pageSize: rowsPerPage,
  });

  if (error) {
    return (
      <Typography color="error" sx={{ textAlign: "center", mt: 3 }}>
        {t("pages.practiceHistory.failedToLoad")}
      </Typography>
    );
  }

  const items = data?.items ?? [];
  const total = data?.totalCount ?? 0;

  const formatDateTime = (iso?: string) => {
    if (!iso) return "-";
    const date = new Date(iso);
    return new Intl.DateTimeFormat(isHebrew ? "he-IL" : "en-US", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      second: isHebrew ? undefined : "2-digit",
      hour12: !isHebrew,
    }).format(date);
  };

  return (
    <Box className={classes.tableWrapper}>
      <Paper elevation={3} className={classes.paperWrapper}>
        {isLoading ? (
          <Box p={4} display="flex" justifyContent="center">
            <CircularProgress />
          </Box>
        ) : (
          <>
            <TableContainer className={classes.tableContainer}>
              <Table stickyHeader>
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
                <TableBody>
                  {items.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={5} align="center">
                        <Typography variant="h6">
                          {t("pages.practiceHistory.noMistakes")}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    (items as GameHistoryDetailedItem[]).map((item, idx) => {
                      const lastAnswer = item.givenAnswer?.length
                        ? item.givenAnswer.join(" ")
                        : "-";
                      return (
                        <TableRow key={`d-${page}-${idx}`} hover>
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
                    })
                  )}
                </TableBody>
              </Table>
            </TableContainer>

            <TablePagination
              className={classes.tablePaginationWrapper}
              component="div"
              count={total}
              page={page}
              onPageChange={(_, p) => onPageChange(p)}
              rowsPerPage={rowsPerPage}
              onRowsPerPageChange={(e) =>
                onRowsPerPageChange(parseInt(e.target.value, 10))
              }
              rowsPerPageOptions={[5, 10]}
            />
          </>
        )}
      </Paper>
    </Box>
  );
};
