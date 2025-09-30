import { useState } from "react";
import {
  Box,
  LinearProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
} from "@mui/material";
import {
  Paged,
  SummaryHistoryWithStudentDto,
  useGetStudentPracticeHistory,
} from "./api";
import { useStyles } from "./style";
import { DifficultyLevel } from "@student/types";

//mock data for 20 users
const mockData: Paged<SummaryHistoryWithStudentDto> = {
  items: Array.from({ length: 20 }).map((_, i) => ({
    studentId: `student-${i + 1}`,
    gameType: i % 2 === 0 ? "math" : "spelling",
    difficulty: (i % 3 === 0
      ? "easy"
      : i % 3 === 1
        ? "medium"
        : "hard") as unknown as DifficultyLevel,
    attemptsCount: Math.floor(Math.random() * 100),
    totalSuccesses: Math.floor(Math.random() * 80),
    totalFailures: Math.floor(Math.random() * 20),
  })),
  totalCount: 20,
  page: 1,
  pageSize: 20,
  hasNextPage: false,
};

export const StudentPracticeHistory = () => {
  const classes = useStyles();

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);

  const { isLoading, isError, error } = useGetStudentPracticeHistory({
    page: page + 1,
    pageSize: rowsPerPage,
  });

  const data = mockData; // Replace with actual data from the hook when backend is ready
  const items = data?.items ?? [];
  const total = data?.totalCount ?? 0;

  const handlePageChange = (_: unknown, newPage: number) => setPage(newPage);
  const handleRowsPerPageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(e.target.value, 10));
    setPage(0);
  };

  return (
    <div className={classes.listContainer} data-testid="history-list">
      <h2 className={classes.sectionTitle}>Student Practice History</h2>

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
                    Student ID
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{ width: { xs: "28%", sm: "18%" } }}
                  >
                    Game Type
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{
                      width: "12%",
                      display: { xs: "none", sm: "table-cell" },
                    }}
                  >
                    Difficulty
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{
                      width: "12%",
                      display: { xs: "none", sm: "table-cell" },
                    }}
                  >
                    Attempts
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{
                      width: "12%",
                      display: { xs: "none", sm: "table-cell" },
                    }}
                  >
                    Successes
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{
                      width: "12%",
                      display: { xs: "none", sm: "table-cell" },
                    }}
                  >
                    Failures
                  </TableCell>
                  <TableCell
                    align="center"
                    sx={{ width: { xs: "28%", sm: "18%" } }}
                  >
                    Success Rate
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
                        <span>Loading…</span>
                      </Box>
                    </TableCell>
                  </TableRow>
                )}

                {isError && !isLoading && (
                  <TableRow>
                    <TableCell colSpan={7} style={{ color: "#c00" }}>
                      {(error as Error)?.message ?? "Failed to load"}
                    </TableCell>
                  </TableRow>
                )}

                {!isLoading && !isError && items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7}>No data found.</TableCell>
                  </TableRow>
                )}

                {!isLoading &&
                  !isError &&
                  items.map((it) => {
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
                          <code>{it.studentId.slice(0, 8)}…</code>
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
          labelRowsPerPage="Rows per page"
          labelDisplayedRows={({ from, to, count }) =>
            `${from}-${to} of ${count !== -1 ? count : to}`
          }
        />
      </div>
    </div>
  );
};
