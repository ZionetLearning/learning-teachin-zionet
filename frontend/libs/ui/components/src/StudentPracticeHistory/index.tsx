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
      <Stack direction="row" spacing={12} className={classes.filtersRow}>
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
              className={classes.table}
              aria-label="history"
            >
              <TableHead>
                <TableRow>
                  <TableCell
                    align="center"
                    sx={{ width: { xs: "44%", sm: "26%" } }}
                  >
                    {t("pages.studentPracticeHistory.columns.studentName")}
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{ width: { xs: "28%", sm: "18%" } }}
                  >
                    {t("pages.studentPracticeHistory.columns.gameType")}
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{
                      width: "12%",
                      display: { xs: "none", sm: "table-cell" },
                    }}
                  >
                    {t("pages.studentPracticeHistory.columns.difficulty")}
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{
                      width: "12%",
                      display: { xs: "none", sm: "table-cell" },
                    }}
                  >
                    {t("pages.studentPracticeHistory.columns.attempts")}
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{
                      width: "12%",
                      display: { xs: "none", sm: "table-cell" },
                    }}
                  >
                    {t("pages.studentPracticeHistory.columns.successes")}
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{
                      width: "12%",
                      display: { xs: "none", sm: "table-cell" },
                    }}
                  >
                    {t("pages.studentPracticeHistory.columns.failures")}
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{ width: { xs: "28%", sm: "18%" } }}
                  >
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
                        <TableCell align="center" title={it.studentId}>
                          <code>{it.studentId}</code>
                        </TableCell>

                        <TableCell
                          align="center"
                          sx={{ textTransform: "capitalize" }}
                        >
                          {it.gameType}
                        </TableCell>

                        <TableCell
                          align="center"
                          sx={{
                            textTransform: "capitalize",
                            display: { xs: "none", sm: "table-cell" },
                          }}
                        >
                          {it.difficulty}
                        </TableCell>

                        <TableCell
                          align="center"
                          sx={{ display: { xs: "none", sm: "table-cell" } }}
                        >
                          {it.attemptsCount}
                        </TableCell>
                        <TableCell
                          align="center"
                          sx={{ display: { xs: "none", sm: "table-cell" } }}
                        >
                          {it.totalSuccesses}
                        </TableCell>
                        <TableCell
                          align="center"
                          sx={{ display: { xs: "none", sm: "table-cell" } }}
                        >
                          {it.totalFailures}
                        </TableCell>

                        <TableCell align="center">
                          <Box
                            display="flex"
                            alignItems="center"
                            gap={1}
                            justifyContent="center"
                          >
                            <Box
                              sx={{ flex: { xs: "0 0 90px", sm: "0 0 140px" } }}
                            >
                              <LinearProgress
                                variant="determinate"
                                value={Math.min(100, Math.max(0, rate))}
                                sx={{
                                  height: 8,
                                  borderRadius: 6,
                                  "& .MuiLinearProgress-bar": {
                                    borderRadius: 6,
                                  },
                                }}
                                aria-label="success rate"
                              />
                            </Box>
                            <Box
                              component="span"
                              sx={{ fontSize: 12, color: "#475569" }}
                            >
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
