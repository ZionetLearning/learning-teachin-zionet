import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Typography,
  Chip,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { DetailedData } from "../../types";
import { useStyles } from "./style";
import { getStatusColor, formatDate } from "../../utils";

interface DetailedViewProps {
  detailedData: DetailedData | undefined;
}

export const DetailedView = ({ detailedData }: DetailedViewProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  if (!detailedData) {
    return (
      <Paper className={classes.paper}>
        <Typography variant="body1" color="text.secondary">
          {t("pages.practiceHistory.noDetailedData")}
        </Typography>
      </Paper>
    );
  }

  if (detailedData.items.length === 0) {
    return (
      <Paper className={classes.paper}>
        <Typography variant="body1" color="text.secondary">
          {t("pages.practiceHistory.noAttemptsYet")}
        </Typography>
      </Paper>
    );
  }

  return (
    <TableContainer component={Paper}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell align="right">
              {t("pages.practiceHistory.game")}
            </TableCell>
            <TableCell align="right">
              {t("pages.practiceHistory.difficulty")}
            </TableCell>
            <TableCell align="right">
              {t("pages.practiceHistory.correctAnswer")}
            </TableCell>
            <TableCell align="right">
              {t("pages.practiceHistory.givenAnswer")}
            </TableCell>
            <TableCell align="right">
              {t("pages.practiceHistory.result")}
            </TableCell>
            <TableCell align="right">
              {t("pages.practiceHistory.timestamp")}
            </TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {detailedData.items.map((attempt) => (
            <TableRow key={attempt.attemptId} hover>
              {/* Game */}
              <TableCell align="right">
                {t(`pages.practiceHistory.practiceTools.${attempt.gameType}`)}
              </TableCell>

              {/* Difficulty */}
              <TableCell align="right">
                {t(`pages.practiceHistory.${attempt.difficulty.toLowerCase()}`)}
              </TableCell>

              {/* Correct Answer */}
              <TableCell align="right" dir="rtl">
                {attempt.correctAnswer.length > 0
                  ? attempt.correctAnswer.join(", ")
                  : "-"}
              </TableCell>

              {/* Given Answer */}
              <TableCell align="right" dir="rtl">
                {attempt.givenAnswer.length > 0
                  ? attempt.givenAnswer.join(", ")
                  : t("pages.practiceHistory.noAttemptYet")}
              </TableCell>

              {/* Result */}
              <TableCell align="right">
                <Chip
                  label={
                    attempt.status === "Success"
                      ? t("pages.practiceHistory.correct")
                      : t("pages.practiceHistory.tryAgain")
                  }
                  color={
                    getStatusColor(attempt.status) as
                      | "success"
                      | "error"
                      | "warning"
                      | "default"
                  }
                  size="small"
                />
              </TableCell>

              {/* Timestamp */}
              <TableCell align="right">
                {formatDate(attempt.createdAt)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};
