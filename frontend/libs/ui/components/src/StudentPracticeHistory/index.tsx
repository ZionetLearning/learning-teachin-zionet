import { useMemo, useState } from "react";

import DownloadIcon from "@mui/icons-material/Download";
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  Tooltip,
} from "@mui/material";
import { CSVLink } from "react-csv";
import { useTranslation } from "react-i18next";

import { useGetStudentPracticeHistory } from "./api";
import { useStyles } from "./style";
import { buildCsvHeaders, levelToLabel, toCsvRow } from "./utils";

const DIFFICULTY_ORDER = ["Easy", "Medium", "Hard"] as const;

export const StudentPracticeHistory = () => {
  const classes = useStyles();
  const { t } = useTranslation();

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [gameType, setGameType] = useState("all");
  const [difficulty, setDifficulty] = useState<
    "all" | "Easy" | "Medium" | "Hard"
  >("all");

  const { data, isLoading, isError } = useGetStudentPracticeHistory({
    page: page + 1,
    pageSize: rowsPerPage,
  });

  const handlePageChange = (_: unknown, newPage: number) => setPage(newPage);
  const handleRowsPerPageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(e.target.value, 10));
    setPage(0);
  };

  const allItems = useMemo(() => data?.items ?? [], [data]);

  const gameTypes = useMemo(
    () => Array.from(new Set(allItems.map((i) => i.gameType))).sort(),
    [allItems],
  );

  const difficulties = useMemo(() => {
    const present = new Set(allItems.map((i) => levelToLabel(i.difficulty)));
    return DIFFICULTY_ORDER.filter((d) => present.has(d));
  }, [allItems]);

  const filtered = useMemo(
    () =>
      allItems.filter(
        (it) =>
          (gameType === "all" || it.gameType === gameType) &&
          (difficulty === "all" || levelToLabel(it.difficulty) === difficulty),
      ),
    [allItems, gameType, difficulty],
  );

  const pagedItems = useMemo(
    () => filtered.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage),
    [filtered, page, rowsPerPage],
  );

  const csvHeaders = useMemo(() => buildCsvHeaders(t), [t]);

  const csvPage = useMemo(
    () =>
      pagedItems.map((it) =>
        toCsvRow(it, {
          levelToLabel,
          rate: (s, a) => (a > 0 ? Math.round((s / a) * 100) : 0),
        }),
      ),
    [pagedItems],
  );

  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);

  const filenamePrefix = useMemo(() => {
    const gt = gameType === "all" ? "all-games" : gameType;
    const dl = difficulty === "all" ? "all-difficulties" : difficulty;
    return `practice-history_${gt}_${dl}_${today}`;
  }, [gameType, difficulty, today]);

  const total = filtered.length;

  return (
    <div className={classes.listContainer} data-testid="history-list">
      <h2 className={classes.sectionTitle}>
        {t("pages.studentPracticeHistory.title")}
      </h2>
      <Stack direction="row" className={classes.filtersRow}>
        <FormControl
          size="small"
          className={classes.filterControl}
          disabled={isLoading || isError}
        >
          <InputLabel id="filter-game-type-label">
            {t("pages.studentPracticeHistory.filters.gameType")}
          </InputLabel>
          <Select
            labelId="filter-game-type-label"
            label={t("pages.studentPracticeHistory.filters.gameType")}
            value={gameType}
            onChange={(e) => {
              setGameType(e.target.value);
              setPage(0);
            }}
          >
            <MenuItem value="all">
              {t("pages.studentPracticeHistory.filters.all")}
            </MenuItem>
            {gameTypes.map((gt) => (
              <MenuItem key={gt} value={gt}>
                {gt}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <FormControl
          size="small"
          className={classes.filterControl}
          disabled={isLoading || isError}
        >
          <InputLabel id="filter-difficulty-label">
            {t("pages.studentPracticeHistory.filters.difficulty")}
          </InputLabel>
          <Select
            labelId="filter-difficulty-label"
            label={t("pages.studentPracticeHistory.filters.difficulty")}
            value={difficulty}
            onChange={(e) => {
              setDifficulty(e.target.value as typeof difficulty);
              setPage(0);
            }}
          >
            <MenuItem value="all">
              {t("pages.studentPracticeHistory.filters.all")}
            </MenuItem>
            {difficulties.map((d) => (
              <MenuItem key={d} value={d}>
                {d}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <Box flexGrow={1} />

        <Tooltip
          title={
            t("pages.studentPracticeHistory.exportCurrentPage") ||
            "Export current page"
          }
        >
          <span>
            <Button
              variant="contained"
              size="small"
              startIcon={<DownloadIcon />}
              disabled={isLoading || isError || pagedItems.length === 0}
              component={CSVLink as unknown as React.ElementType}
              headers={csvHeaders}
              data={csvPage}
              filename={`${filenamePrefix}_page-${page + 1}_rpp-${rowsPerPage}.csv`}
              uFEFF
              target="_blank"
            >
              {t("pages.studentPracticeHistory.exportPage")}
            </Button>
          </span>
        </Tooltip>
      </Stack>
      <div className={classes.tableArea} data-testid="history-table">
        <div className={classes.tableShell} data-testid="history-table-shell">
          <TableContainer className={classes.rowsScroll}>
            <Table
              stickyHeader
              size="small"
              className={`${classes.table} ${classes.tableWide}`}
              aria-label="history"
            >
              <TableHead>
                <TableRow>
                  <TableCell align="center" className={classes.colStudent}>
                    {t("pages.studentPracticeHistory.columns.studentName")}
                  </TableCell>
                  <TableCell align="center" className={classes.colGameType}>
                    {t("pages.studentPracticeHistory.columns.gameType")}
                  </TableCell>
                  <TableCell align="center" className={classes.colDifficulty}>
                    {t("pages.studentPracticeHistory.columns.difficulty")}
                  </TableCell>
                  <TableCell align="center" className={classes.colAttempts}>
                    {t("pages.studentPracticeHistory.columns.attempts")}
                  </TableCell>
                  <TableCell align="center" className={classes.colAttempts}>
                    {t("pages.studentPracticeHistory.columns.successes")}
                  </TableCell>
                  <TableCell align="center" className={classes.colAttempts}>
                    {t("pages.studentPracticeHistory.columns.failures")}
                  </TableCell>
                  <TableCell align="center" className={classes.colRate}>
                    {t("pages.studentPracticeHistory.columns.successRate")}
                  </TableCell>
                </TableRow>
              </TableHead>

              <TableBody>
                {isLoading && (
                  <TableRow>
                    <TableCell colSpan={7}>
                      <Box display="flex" alignItems="center" gap={2} py={1}>
                        <LinearProgress
                          sx={{ flex: 1, height: 6, borderRadius: 1 }}
                        />
                        <span>{t("pages.studentPracticeHistory.loading")}</span>
                      </Box>
                    </TableCell>
                  </TableRow>
                )}

                {isError && !isLoading && (
                  <TableRow>
                    <TableCell colSpan={7} style={{ color: "#c00" }}>
                      {t("pages.studentPracticeHistory.failedToLoad")}
                    </TableCell>
                  </TableRow>
                )}

                {!isLoading && !isError && pagedItems.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7}>
                      {t("pages.studentPracticeHistory.noDataFound")}
                    </TableCell>
                  </TableRow>
                )}

                {!isLoading &&
                  !isError &&
                  pagedItems.map((it) => {
                    const rate =
                      it.attemptsCount > 0
                        ? Math.round(
                            (it.totalSuccesses / it.attemptsCount) * 100,
                          )
                        : 0;

                    return (
                      <TableRow
                        key={`${it.studentId}-${it.gameType}-${it.difficulty}`}
                      >
                        <TableCell
                          align="center"
                          title={it.studentId}
                          className={classes.colStudent}
                        >
                          <Tooltip title={it.studentId} arrow>
                            <span className={classes.ellipsis}>
                              {it.studentId}
                            </span>
                          </Tooltip>
                        </TableCell>

                        <TableCell
                          align="center"
                          className={`${classes.colGameType} ${classes.cap}`}
                        >
                          {it.gameType}
                        </TableCell>

                        <TableCell
                          align="center"
                          className={`${classes.colDifficulty} ${classes.cap}`}
                        >
                          {levelToLabel(it.difficulty)}
                        </TableCell>

                        <TableCell
                          align="center"
                          className={classes.colAttempts}
                        >
                          {it.attemptsCount}
                        </TableCell>
                        <TableCell
                          align="center"
                          className={classes.colAttempts}
                        >
                          {it.totalSuccesses}
                        </TableCell>
                        <TableCell
                          align="center"
                          className={classes.colAttempts}
                        >
                          {it.totalFailures}
                        </TableCell>

                        <TableCell align="center" className={classes.colRate}>
                          <Box className={classes.rateWrapper}>
                            <Box className={classes.rateBarWrap}>
                              <LinearProgress
                                variant="determinate"
                                value={Math.min(100, Math.max(0, rate))}
                                className={classes.rateBar}
                                aria-label="success rate"
                              />
                            </Box>
                            <Box component="span" className={classes.rateText}>
                              {rate}%
                            </Box>
                          </Box>
                        </TableCell>
                      </TableRow>
                    );
                  })}
              </TableBody>
            </Table>
          </TableContainer>
        </div>
        <TablePagination
          component="div"
          className={classes.paginationBar}
          data-testid="history-pagination"
          count={total}
          page={page}
          onPageChange={handlePageChange}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={handleRowsPerPageChange}
          rowsPerPageOptions={[10, 20, 50, 100]}
          labelRowsPerPage={t("pages.studentPracticeHistory.rowsPerPage")}
          labelDisplayedRows={({ from, to, count }) =>
            `${from}-${to} ${t("pages.studentPracticeHistory.of")} ${count !== -1 ? count : to}`
          }
        />
      </div>
    </div>
  );
};
