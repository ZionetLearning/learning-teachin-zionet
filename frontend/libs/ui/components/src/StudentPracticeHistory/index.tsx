import { useMemo, useState } from "react";

import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  LinearProgress,
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

import {
  SummaryHistoryWithStudentDto,
  useGetStudentPracticeHistory,
} from "./api";
import { DifficultyFilter, StudentPracticeFilters } from "./components";
import { useStyles } from "./style";
import {
  buildCsvHeaders,
  DifficultyLabel,
  levelToLabel,
  toCsvRow,
} from "./utils";

const ORDER_INDEX: Record<DifficultyLabel, number> = {
  Easy: 0,
  Medium: 1,
  Hard: 2,
};

type StudentGroup = {
  studentId: string;
  items: SummaryHistoryWithStudentDto[];
  totals: {
    attempts: number;
    successes: number;
    failures: number;
    ratePct: number;
    gameTypes: string[];
    difficulties: DifficultyFilter[];
  };
};

export const StudentPracticeHistory = () => {
  const classes = useStyles();
  const { t } = useTranslation();

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [gameType, setGameType] = useState("all");
  const [studentId, setStudentId] = useState("all");
  const [difficulty, setDifficulty] = useState<"all" | DifficultyLabel>("all");

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
    return Object.keys(ORDER_INDEX).filter((d) =>
      present.has(d as DifficultyLabel),
    );
  }, [allItems]);

  const studentIds = useMemo(
    () => Array.from(new Set(allItems.map((i) => i.studentId))).sort(),
    [allItems],
  );

  const filtered = useMemo(
    () =>
      allItems.filter(
        (it) =>
          (studentId === "all" || it.studentId === studentId) &&
          (gameType === "all" || it.gameType === gameType) &&
          (difficulty === "all" || levelToLabel(it.difficulty) === difficulty),
      ),
    [allItems, studentId, gameType, difficulty],
  );

  const grouped: StudentGroup[] = useMemo(() => {
    const map = new Map<string, StudentGroup>();
    for (const it of filtered) {
      const key = it.studentId;
      if (!map.has(key)) {
        map.set(key, {
          studentId: key,
          items: [],
          totals: {
            attempts: 0,
            successes: 0,
            failures: 0,
            ratePct: 0,
            gameTypes: [],
            difficulties: [],
          },
        });
      }
      const g = map.get(key)!;
      g.items.push(it);
    }
    for (const g of map.values()) {
      const attempts = g.items.reduce((s, i) => s + i.attemptsCount, 0);
      const successes = g.items.reduce((s, i) => s + i.totalSuccesses, 0);
      const failures = g.items.reduce((s, i) => s + i.totalFailures, 0);
      const ratePct =
        attempts > 0 ? Math.round((successes / attempts) * 100) : 0;
      const gameTypes = Array.from(
        new Set(g.items.map((i) => i.gameType)),
      ).sort();
      const diffs = Array.from(
        new Set(
          g.items.map((i) => levelToLabel(i.difficulty) as DifficultyLabel),
        ),
      ).sort((a, b) => ORDER_INDEX[a] - ORDER_INDEX[b]);

      g.totals = {
        attempts,
        successes,
        failures,
        ratePct,
        gameTypes,
        difficulties: diffs,
      };
    }
    return Array.from(map.values()).sort((a, b) =>
      a.studentId.localeCompare(b.studentId),
    );
  }, [filtered]);

  const pagedItems = useMemo(
    () => grouped.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage),
    [grouped, page, rowsPerPage],
  );

  const csvHeaders = useMemo(() => buildCsvHeaders(t), [t]);

  const csvPage = useMemo(
    () =>
      pagedItems.flatMap((g) =>
        g.items.map((it) =>
          toCsvRow(it, {
            levelToLabel,
            rate: (s, a) => (a > 0 ? Math.round((s / a) * 100) : 0),
          }),
        ),
      ),
    [pagedItems],
  );

  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);

  const filenamePrefix = useMemo(() => {
    const gt = gameType === "all" ? "all-games" : gameType;
    const dl = difficulty === "all" ? "all-difficulties" : difficulty;
    return `practice-history_${gt}_${dl}_${today}`;
  }, [gameType, difficulty, today]);

  const total = grouped.length;

  return (
    <div className={classes.listContainer} data-testid="history-list">
      <h2 className={classes.sectionTitle}>
        {t("pages.studentPracticeHistory.title")}
      </h2>
      <StudentPracticeFilters
        isDisabled={isLoading || isError}
        studentId={studentId}
        setStudentId={setStudentId}
        studentIds={studentIds}
        gameType={gameType}
        setGameType={setGameType}
        gameTypes={gameTypes}
        difficulty={difficulty}
        setDifficulty={setDifficulty}
        difficulties={difficulties as ("Easy" | "Medium" | "Hard")[]}
        csvHeaders={csvHeaders}
        csvPage={csvPage}
        filename={`${filenamePrefix}_page-${page + 1}_rpp-${rowsPerPage}.csv`}
        onAnyChange={() => setPage(0)}
      />
      <div className={classes.tableArea} data-testid="history-table">
        <div className={classes.tableShell} data-testid="history-table-shell">
          <TableContainer className={classes.rowsScroll}>
            <Table
              stickyHeader
              size="small"
              className={`${classes.table} ${classes.tableWide}`}
              aria-label="history"
            >
              <colgroup>
                <col className={classes.colStudent} />
                <col className={classes.colMetrics} />
              </colgroup>
              <TableHead>
                <TableRow>
                  <TableCell align="left" className={classes.colStudent}>
                    {t("pages.studentPracticeHistory.columns.studentName")}
                  </TableCell>
                  <TableCell align="right" className={classes.colMetrics}>
                    {t("pages.studentPracticeHistory.columns.metrics")}
                  </TableCell>
                </TableRow>
              </TableHead>

              <TableBody>
                {isLoading && (
                  <TableRow>
                    <TableCell colSpan={2}>
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
                    <TableCell colSpan={2} style={{ color: "#c00" }}>
                      {t("pages.studentPracticeHistory.failedToLoad")}
                    </TableCell>
                  </TableRow>
                )}

                {!isLoading && !isError && pagedItems.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={2}>
                      {t("pages.studentPracticeHistory.noDataFound")}
                    </TableCell>
                  </TableRow>
                )}

                {!isLoading &&
                  !isError &&
                  pagedItems.map((g) => (
                    <TableRow key={g.studentId}>
                      <TableCell colSpan={2} className={classes.groupCell}>
                        <Accordion
                          disableGutters
                          square
                          className={classes.groupAccordion}
                        >
                          <AccordionSummary
                            expandIcon={<ExpandMoreIcon />}
                            className={classes.groupSummary}
                          >
                            <div className={classes.summaryBar}>
                              <div className={classes.summaryLeft}>
                                <Tooltip title={g.studentId} arrow>
                                  <span
                                    className={`${classes.ellipsis} ${classes.studentId}`}
                                  >
                                    {g.studentId}
                                  </span>
                                </Tooltip>
                              </div>

                              <div className={classes.summaryRight}>
                                <span className={classes.metric}>
                                  {t(
                                    "pages.studentPracticeHistory.columns.attempts",
                                  )}
                                  : <strong>{g.totals.attempts}</strong>
                                </span>
                                <span className={classes.metric}>
                                  {t(
                                    "pages.studentPracticeHistory.columns.successes",
                                  )}
                                  : <strong>{g.totals.successes}</strong>
                                </span>
                                <span className={classes.metric}>
                                  {t(
                                    "pages.studentPracticeHistory.columns.failures",
                                  )}
                                  : <strong>{g.totals.failures}</strong>
                                </span>
                                <span
                                  className={classes.ratePill}
                                  aria-label="success rate"
                                >
                                  {g.totals.ratePct}%
                                </span>
                              </div>
                            </div>
                          </AccordionSummary>

                          <AccordionDetails className={classes.groupDetails}>
                            <Table
                              size="small"
                              aria-label="student attempts"
                              className={classes.innerTable}
                            >
                              <colgroup>
                                <col className={classes.colInnerGameType} />
                                <col className={classes.colInnerDifficulty} />
                                <col className={classes.colInnerAttempts} />
                                <col className={classes.colInnerSuccesses} />
                                <col className={classes.colInnerFailures} />
                                <col className={classes.colInnerRate} />
                              </colgroup>
                              <TableHead>
                                <TableRow>
                                  <TableCell
                                    align="center"
                                    className={classes.colGameType}
                                  >
                                    {t(
                                      "pages.studentPracticeHistory.columns.gameType",
                                    )}
                                  </TableCell>
                                  <TableCell
                                    align="center"
                                    className={classes.colDifficulty}
                                  >
                                    {t(
                                      "pages.studentPracticeHistory.columns.difficulty",
                                    )}
                                  </TableCell>
                                  <TableCell
                                    align="center"
                                    className={classes.colAttempts}
                                  >
                                    {t(
                                      "pages.studentPracticeHistory.columns.attempts",
                                    )}
                                  </TableCell>
                                  <TableCell
                                    align="center"
                                    className={classes.colAttempts}
                                  >
                                    {t(
                                      "pages.studentPracticeHistory.columns.successes",
                                    )}
                                  </TableCell>
                                  <TableCell
                                    align="center"
                                    className={classes.colAttempts}
                                  >
                                    {t(
                                      "pages.studentPracticeHistory.columns.failures",
                                    )}
                                  </TableCell>
                                  <TableCell
                                    align="center"
                                    className={classes.colRate}
                                  >
                                    {t(
                                      "pages.studentPracticeHistory.columns.successRate",
                                    )}
                                  </TableCell>
                                </TableRow>
                              </TableHead>
                              <TableBody>
                                {g.items.map((it) => {
                                  const rate =
                                    it.attemptsCount > 0
                                      ? Math.round(
                                          (it.totalSuccesses /
                                            it.attemptsCount) *
                                            100,
                                        )
                                      : 0;
                                  return (
                                    <TableRow
                                      key={`${it.studentId}-${it.gameType}-${it.difficulty}`}
                                      className={classes.innerRow}
                                    >
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
                                      <TableCell
                                        align="center"
                                        className={classes.colRate}
                                      >
                                        <Box className={classes.rateWrapper}>
                                          <Box className={classes.rateBarWrap}>
                                            <LinearProgress
                                              variant="determinate"
                                              value={Math.min(
                                                100,
                                                Math.max(0, rate),
                                              )}
                                              className={classes.rateBar}
                                              aria-label="success rate"
                                            />
                                          </Box>
                                          <Box
                                            component="span"
                                            className={classes.rateText}
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
                          </AccordionDetails>
                        </Accordion>
                      </TableCell>
                    </TableRow>
                  ))}
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
