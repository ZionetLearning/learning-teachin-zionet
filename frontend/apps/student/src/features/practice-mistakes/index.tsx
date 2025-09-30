import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Typography,
  CircularProgress,
  TablePagination,
  Box,
} from "@mui/material";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import { useState, useMemo } from "react";
import { useAuth } from "@app-providers";
import { useGetGameMistakes, GameMistakeItem } from "@student/api";
// import { useNavigate } from "react-router-dom";
export const PracticeMistakes = () => {
  const { user } = useAuth();
  const studentId = user?.userId ?? "";

  const [pageZero, setPageZero] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  const apiPage = useMemo(() => pageZero + 1, [pageZero]);

  const { data, isLoading, isFetching, error } = useGetGameMistakes({
    studentId,
    page: apiPage,
    pageSize: rowsPerPage,
  });

  console.log({data})

  // const navigate = useNavigate();
  const handleRetry = (item: GameMistakeItem) => {
    //future idea:

    // navigate(`/word-order-game/${item.gameType}`, { state: { mode: "retry", item } });
    console.log("Retry:", item);
  };

  const handleChangePage = (_: unknown, newPageZero: number) => {
    setPageZero(newPageZero);
  };

  const handleChangeRowsPerPage = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    setRowsPerPage(parseInt(e.target.value, 10));
    setPageZero(0); // חזרה לעמוד הראשון כשמשנים גודל עמוד
  };

  if (!studentId) {
    return <Typography color="error">Missing student ID.</Typography>;
  }

  if (error) {
    return <Typography color="error">Failed to load mistakes.</Typography>;
  }

  const items = data?.items ?? [];
  const total = data?.totalCount ?? 0;

  return (
    <Paper>
      <Box
        display="flex"
        alignItems="center"
        justifyContent="space-between"
        p={2}
      >
        <Typography variant="h6">Practice Mistakes</Typography>
        {isFetching && <CircularProgress size={20} />}
      </Box>

      {isLoading ? (
        <Box p={3} display="flex" justifyContent="center">
          <CircularProgress />
        </Box>
      ) : items.length === 0 ? (
        <Box p={3}>
          <Typography>No mistakes to practice 🎉</Typography>
        </Box>
      ) : (
        <>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Practice Name</TableCell>
                  <TableCell>Difficulty</TableCell>
                  <TableCell>Your Last Answer</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Action</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {items.map((item: GameMistakeItem, idx: number) => (
                  <TableRow key={`${apiPage}-${idx}`} hover>
                    <TableCell>{item.gameType}</TableCell>
                    <TableCell>{item.difficulty}</TableCell>
                    <TableCell>{item.wrongAnswers.flat().join(" ")}</TableCell>
                    <TableCell>
                      <Typography color="warning.main">Try again!</Typography>
                    </TableCell>
                    <TableCell align="right">
                      <IconButton
                        color="primary"
                        aria-label="retry"
                        onClick={() => handleRetry(item)}
                      >
                        <PlayArrowIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <TablePagination
            component="div"
            count={total}
            page={pageZero} // 0-based
            onPageChange={handleChangePage}
            rowsPerPage={rowsPerPage}
            onRowsPerPageChange={handleChangeRowsPerPage}
            rowsPerPageOptions={[5, 10, 25, 50]}
          />
        </>
      )}
    </Paper>
  );
};
