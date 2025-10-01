import { useState, useMemo } from "react";
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
  Button,
} from "@mui/material";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import { useAuth } from "@app-providers";
import { useGetGameMistakes, GameMistakeItem } from "@student/api";
import { DifficultyChip, StatusChip } from "./components";
import { useStyles } from "./style";

export const PracticeMistakes = () => {
  const { user } = useAuth();
  const { t } = useTranslation();
  const classes = useStyles();

  const studentId = user?.userId ?? "";

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const apiPage = useMemo(() => page + 1, [page]);

  const { data, isLoading, isFetching, error } = useGetGameMistakes({
    studentId,
    page: apiPage,
    pageSize: rowsPerPage,
  });

  const items = data?.items ?? [];
  const total = data?.totalCount ?? 0;

  const handleRetry = (item: GameMistakeItem) => {
    // TODO: navigate to practice screen with item context (currently to WordOrderGame)
    console.log("Retry:", item);
  };

  const handleChangePage = (_: unknown, newPage: number) => setPage(newPage);
  const handleChangeRowsPerPage = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    setRowsPerPage(parseInt(e.target.value, 10));
    setPage(0);
  };

  if (error) {
    return (
      <Typography color="error" sx={{ textAlign: "center", mt: 3 }}>
        {t("pages.practiceMistakes.failedToLoad")}
      </Typography>
    );
  }

  return (
    <Box>
      <Box className={classes.headerWrapper} sx={{ py: 3 }}>
        <Typography className={classes.title}>
          {t("pages.practiceMistakes.title")}
        </Typography>
        <Typography className={classes.description} sx={{ mt: 0.5 }}>
          {t("pages.practiceMistakes.description")}
        </Typography>
        {isFetching && <CircularProgress size={20} sx={{ mt: 1 }} />}
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
                {t("pages.practiceMistakes.noMistakes")}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {t("pages.practiceMistakes.keepPracticingHint")}
              </Typography>
            </Box>
          ) : (
            <>
              <TableContainer className={classes.tableContainer}>
                <Table stickyHeader>
                  <TableHead>
                    <TableRow>
                      <TableCell className={classes.th}>
                        {t("pages.practiceMistakes.practiceName")}
                      </TableCell>
                      <TableCell className={classes.th}>
                        {t("pages.practiceMistakes.difficulty")}
                      </TableCell>
                      <TableCell className={classes.th}>
                        {t("pages.practiceMistakes.yourLastAnswer")}
                      </TableCell>
                      <TableCell className={classes.th}>
                        {t("pages.practiceMistakes.status")}
                      </TableCell>
                      <TableCell className={classes.th} align="right">
                        {t("pages.practiceMistakes.action")}
                      </TableCell>
                    </TableRow>
                  </TableHead>

                  <TableBody>
                    {items.map((item: GameMistakeItem, idx: number) => {
                      const last =
                        item.wrongAnswers.length > 0
                          ? item.wrongAnswers[item.wrongAnswers.length - 1]
                          : [];
                      const lastAnswer =
                        last && last.length > 0 ? last.join(" ") : "-";

                      return (
                        <TableRow key={`${apiPage}-${idx}`} hover>
                          <TableCell className={classes.td}>
                            <Typography fontWeight={600}>
                              {item.gameType}
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
                              text={t("pages.practiceMistakes.tryAgain")}
                            />
                          </TableCell>

                          <TableCell className={classes.td}>
                            <Button
                              size="small"
                              variant="contained"
                              startIcon={<PlayArrowIcon />}
                              onClick={() => handleRetry(item)}
                              className={classes.retryButton}
                            >
                              {t("pages.practiceMistakes.retry")}
                            </Button>
                          </TableCell>
                        </TableRow>
                      );
                    })}
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
