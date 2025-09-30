import { useMemo, useState } from "react";
import {
  Box,
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
import { useTranslation } from "react-i18next";

import { DifficultyLevel } from "@student/types";
import { useGetStudentPracticeHistory } from "./api";
import { useStyles } from "./style";

const levelToLabel = (
  level: DifficultyLevel | string,
): "Easy" | "Medium" | "Hard" => {
  if (typeof level === "number") {
    return level === 0 ? "Easy" : level === 1 ? "Medium" : "Hard";
  }
  const s = String(level).toLowerCase();
  if (s === "0" || s === "easy") return "Easy";
  if (s === "1" || s === "medium") return "Medium";
  return "Hard";
};

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

  const allItems = useMemo(() => data?.items ?? [], [data]);

  const gameTypes = useMemo(
    () => Array.from(new Set(allItems.map((i) => i.gameType))).sort(),
    [allItems],
  );

  const difficulties = useMemo(
    () =>
      Array.from(
        new Set(allItems.map((i) => levelToLabel(i.difficulty))),
      ).sort() as Array<"Easy" | "Medium" | "Hard">,
    [allItems],
  );

  const filtered = useMemo(
    () =>
      allItems.filter(
        (it) =>
          (gameType === "all" || it.gameType === gameType) &&
          (difficulty === "all" || levelToLabel(it.difficulty) === difficulty),
      ),
    [allItems, gameType, difficulty],
  );

  const total = filtered.length;
  const pagedItems = useMemo(
    () => filtered.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage),
    [filtered, page, rowsPerPage],
  );

  const handlePageChange = (_: unknown, newPage: number) => setPage(newPage);
  const handleRowsPerPageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(e.target.value, 10));
    setPage(0);
  };

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
              setDifficulty(e.target.value);
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
                  <TableCell align="center" className={classes.colSuccesses}>
                    {t("pages.studentPracticeHistory.columns.successes")}
                  </TableCell>
                  <TableCell align="center" className={classes.colFailures}>
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
                          {it.difficulty}
                        </TableCell>

                        <TableCell
                          align="center"
                          className={classes.colAttempts}
                        >
                          {it.attemptsCount}
                        </TableCell>
                        <TableCell
                          align="center"
                          className={classes.colSuccesses}
                        >
                          {it.totalSuccesses}
                        </TableCell>
                        <TableCell
                          align="center"
                          className={classes.colFailures}
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
