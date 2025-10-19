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
  TableRow,
  Tooltip,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { SuccessRateChip } from "@ui-components";
import type { SummaryHistoryWithStudentDto } from "@api";
import { levelToLabel } from "../../utils";
import { DifficultyFilter } from "../Filters";
import { useStyles } from "./style";

export type StudentGroup = {
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
  studentFirstName: string;
  studentLastName: string;
  timestamp: string;
};

interface StudentPracticeTableProps {
  pagedItems: StudentGroup[];
  isLoading: boolean;
  isError: boolean;
}

export const StudentPracticeTable = ({
  pagedItems,
  isLoading,
  isError,
}: StudentPracticeTableProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  return (
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
                              <Tooltip
                                title={
                                  `${g.studentFirstName} ${g.studentLastName}`.trim() ||
                                  g.studentId
                                }
                                arrow
                              >
                                <span
                                  className={`${classes.ellipsis} ${classes.studentId}`}
                                >
                                  {`${g.studentFirstName} ${g.studentLastName}`.trim() ||
                                    g.studentId}
                                </span>
                              </Tooltip>
                              <Box
                                component="span"
                                sx={{
                                  ml: 2,
                                  color: "text.secondary",
                                  fontSize: 12,
                                }}
                              >
                                {t(
                                  "pages.studentPracticeHistory.columns.lastAttemptTime",
                                )}
                                : {g.timestamp?.slice(0, 19).replace("T", " ")}
                              </Box>
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
                              <SuccessRateChip value={g.totals.ratePct} />
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
                                        (it.totalSuccesses / it.attemptsCount) *
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
    </div>
  );
};
