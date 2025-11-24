import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
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
import SkipNextIcon from "@mui/icons-material/SkipNext";
import SkipPreviousIcon from "@mui/icons-material/SkipPrevious";
import { useAuth } from "@app-providers";
import { useGetGameMistakes, GameMistakeItem } from "@student/api";
import { DifficultyChip, StatusChip } from "@ui-components";
import { useStyles } from "./style";

export const PracticeMistakes = () => {
  const { user } = useAuth();
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const classes = useStyles();

  const studentId = user?.userId ?? "";
  const isHebrew = i18n.language === "he";

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  const { data, isLoading, isFetching, error } = useGetGameMistakes({
    studentId,
    page,
    pageSize: rowsPerPage,
  });

  const items = data?.items ?? [];
  const total = data?.totalCount ?? 0;

  const handleRetry = (item: GameMistakeItem) => {
    const retryData = {
      exerciseId: item.exerciseId,
      correctAnswer: item.correctAnswer,
      mistakes: item.mistakes,
      difficulty:
        item.difficulty === "Easy" ? 0 : item.difficulty === "Medium" ? 1 : 2,
    };

    let route = "/word-order-game"; // default
    switch (item.gameType) {
      case "WordOrderGame":
        route = "/word-order-game";
        break;
      case "TypingPractice":
        route = "/typing";
        break;
      case "SpeakingPractice":
        route = "/speaking";
        break;
    }

    navigate(route, {
      state: { retryData },
    });
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
                        {t("pages.practiceMistakes.lastAccuracy")}
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
                      const lastMistake =
                        item.mistakes.length > 0
                          ? item.mistakes[item.mistakes.length - 1]
                          : null;
                      const lastAnswer = lastMistake
                        ? lastMistake.wrongAnswer.join(" ")
                        : "-";

                      return (
                        <TableRow key={`${page}-${idx}`} hover>
                          <TableCell className={classes.td}>
                            <Typography fontWeight={600}>
                              {t(
                                `pages.practiceMistakes.gameType.${item.gameType}`,
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
                            {lastMistake ? (
                              <Typography
                                fontWeight={600}
                                color={
                                  lastMistake.accuracy >= 80
                                    ? "success.main"
                                    : lastMistake.accuracy >= 60
                                      ? "warning.main"
                                      : "error.main"
                                }
                              >
                                {lastMistake.accuracy.toFixed(1)}%
                              </Typography>
                            ) : (
                              "-"
                            )}
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
                              endIcon={
                                isHebrew ? (
                                  <SkipPreviousIcon />
                                ) : (
                                  <SkipNextIcon />
                                )
                              }
                              onClick={() => handleRetry(item)}
                              className={classes.retryButton}
                              sx={{
                                "& .MuiButton-endIcon": {
                                  ml: 1,
                                  mr: isHebrew ? 1 : 0,
                                },
                              }}
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
