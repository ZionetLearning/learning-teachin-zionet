import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Typography,
  CircularProgress,
  TablePagination,
  Box,
} from "@mui/material";
import { useState, useMemo } from "react";
import { useAuth } from "@app-providers";
import { useGetGameMistakes, GameMistakeItem } from "@student/api";
import { useTranslation } from "react-i18next";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
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
    console.log("Retry:", item);
  };

  const handleChangePage = (_: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    setRowsPerPage(parseInt(e.target.value, 10));
    setPage(0);
  };

  if (error) {
    return (
      <Typography color="error">
        {t("pages.practiceMistakes.failedToLoad")}
      </Typography>
    );
  }

  return (
    <Box>
      <Box className={classes.headerWrapper}>
        <Typography className={classes.title}>
          {t("pages.practiceMistakes.title")}
        </Typography>
        <Typography className={classes.description}>
          {t("pages.practiceMistakes.description")}
        </Typography>
        {isFetching && <CircularProgress size={20} />}
      </Box>

      {isLoading ? (
        <Box p={3} display="flex" justifyContent="center">
          <CircularProgress />
        </Box>
      ) : items.length === 0 ? (
        <Box p={3}>
          <Typography>{t("pages.practiceMistakes.noMistakes")}</Typography>
        </Box>
      ) : (
        <Box className={classes.tableWrapper} sx={{ flex: 1 }}>
          <TableContainer
            sx={{
              maxHeight: 800,
            }}
          >
            <Table
              sx={{
                "& th, & td": {
                  textAlign: "center",
                },
              }}
            >
              <TableHead>
                <TableRow>
                  <TableCell>
                    {t("pages.practiceMistakes.practiceName")}
                  </TableCell>
                  <TableCell>
                    {t("pages.practiceMistakes.difficulty")}
                  </TableCell>
                  <TableCell>
                    {t("pages.practiceMistakes.yourLastAnswer")}
                  </TableCell>
                  <TableCell>{t("pages.practiceMistakes.status")}</TableCell>
                  <TableCell align="right">
                    {t("pages.practiceMistakes.action")}
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {items.map((item: GameMistakeItem, idx: number) => (
                  <TableRow key={`${apiPage}-${idx}`} hover>
                    <TableCell>{item.gameType}</TableCell>
                    <TableCell>{item.difficulty}</TableCell>
                    <TableCell align="center">
                      {item.wrongAnswers.length > 0 &&
                      item.wrongAnswers[item.wrongAnswers.length - 1].length > 0
                        ? item.wrongAnswers[item.wrongAnswers.length - 1].join(
                            " ",
                          )
                        : "-"}
                    </TableCell>
                    <TableCell>
                      <Typography color="warning.main">
                        {t("pages.practiceMistakes.tryAgain")}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <IconButton
                        sx={{ color: "#7c4dff" }}
                        aria-label="retry"
                        onClick={() => handleRetry(item)}
                      >
                        <PlayArrowIcon />
                        <Typography sx={{ fontSize: "16px" }}>
                          {t("pages.practiceMistakes.retry")}
                        </Typography>
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
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
        </Box>
      )}
    </Box>
  );
};
