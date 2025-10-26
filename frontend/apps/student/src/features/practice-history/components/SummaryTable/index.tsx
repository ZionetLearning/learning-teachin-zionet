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
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { DifficultyChip, SuccessRateChip } from "@ui-components";
import { useGetGameHistorySummary } from "@student/api";
import type { GameHistorySummaryItem } from "@student/api";
import { useStyles } from "./style";

export const SummaryTable = ({
  studentId,
  page,
  rowsPerPage,
  onPageChange,
  onRowsPerPageChange,
}: {
  studentId: string;
  page: number;
  rowsPerPage: number;
  onPageChange: (p: number) => void;
  onRowsPerPageChange: (r: number) => void;
}) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const { data, isLoading, error } = useGetGameHistorySummary({
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
                <TableBody>
                  {items.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={6} align="center">
                        <Typography variant="h6">
                          {t("pages.practiceHistory.noMistakes")}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    (items as GameHistorySummaryItem[]).map((item, idx) => {
                      const r = item.attemptsCount
                        ? Math.round(
                            (item.totalSuccesses / item.attemptsCount) * 100,
                          )
                        : 0;
                      return (
                        <TableRow key={`s-${page}-${idx}`} hover>
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
                          <TableCell className={classes.td} align="center">
                            {item.attemptsCount}
                          </TableCell>
                          <TableCell className={classes.td} align="center">
                            {item.totalSuccesses}
                          </TableCell>
                          <TableCell className={classes.td} align="center">
                            {item.totalFailures}
                          </TableCell>
                          <TableCell className={classes.td} align="center">
                            <SuccessRateChip value={r} />
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
              page={page - 1}
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
