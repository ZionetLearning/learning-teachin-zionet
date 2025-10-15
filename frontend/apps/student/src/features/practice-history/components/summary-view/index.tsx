import {
  Box,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Typography,
} from "@mui/material";
import { Play } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { getDifficultyLabel } from "@student/features/practice/utils";
import { SummaryData, DetailedData } from "../../types";
import { useStyles } from "./style";

interface SummaryViewProps {
  summaryData: SummaryData | undefined;
  detailedData: DetailedData | undefined;
  isHebrew: boolean;
}

export const SummaryView = ({
  summaryData,
  detailedData,
  isHebrew,
}: SummaryViewProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const classes = useStyles();
  if (!summaryData) return null;

  const handleRetry = (
    gameType: string,
    difficulty: string,
    correctAnswer: string[] | undefined,
    attemptId: string | undefined,
    nikud: boolean = true,
  ) => {
    navigate("/word-order-game", {
      state: {
        retryMode: true,
        nikud: nikud,
        difficulty: Number(difficulty) as 0 | 1 | 2,
        gameType: gameType,
        sentence: correctAnswer?.join(" "),
        attemptId: attemptId
      },
    });
  };
  return (
    <TableContainer component={Paper}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell align="right">
              {t("pages.practiceHistory.game")}
            </TableCell>
            <TableCell align="right">
              {t("pages.practiceHistory.yourLastAnswer")}
            </TableCell>
            <TableCell align="right">
              {t("pages.practiceHistory.status")}
            </TableCell>
            <TableCell align="right">
              {t("pages.practiceHistory.action")}
            </TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {summaryData.items.map((item, index) => {
            const lastAttempt = detailedData?.items
              .filter(
                (detail) =>
                  detail.gameType === item.gameType &&
                  detail.difficulty === item.difficulty,
              )
              .sort((a, b) => b.attemptNumber - a.attemptNumber)[0];

            return (
              <TableRow key={index} hover>
                <TableCell align="right">
                  <Box>
                    <Typography variant="body2" fontWeight="medium">
                      {t(
                        `pages.practiceHistory.practiceTools.${item.gameType}`,
                      )}
                      {lastAttempt?.correctAnswer?.length ? (
                        <>: "{lastAttempt.correctAnswer.join(" ")}"</>
                      ) : null}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {getDifficultyLabel(
                        Number(item.difficulty) as 0 | 1 | 2,
                        t,
                      )}
                    </Typography>
                  </Box>
                </TableCell>

                <TableCell align="right">
                  <Typography variant="body2">
                    {lastAttempt?.givenAnswer?.length
                      ? lastAttempt.givenAnswer.join(", ")
                      : t("pages.practiceHistory.noAttemptYet")}
                  </Typography>
                </TableCell>

                <TableCell align="right">
                  <Typography
                    variant="body2"
                    color={
                      item.totalSuccesses > 0 ? "success.main" : "text.primary"
                    }
                  >
                    {item.totalSuccesses > 0
                      ? t("pages.practiceHistory.correct")
                      : t("pages.practiceHistory.tryAgain")}
                  </Typography>
                </TableCell>

                <TableCell align="right">
                  <Button
                    className={classes.retryButton}
                    variant="contained"
                    size="small"
                    startIcon={!isHebrew && <Play size={16} />}
                    endIcon={isHebrew && <Play size={16} />}
                    onClick={() =>
                      handleRetry(
                        item.gameType,
                        item.difficulty,
                        lastAttempt?.correctAnswer,
                        lastAttempt?.attemptId
                      )
                    }
                  >
                    <Typography
                      variant="button"
                      sx={{
                        paddingRight: isHebrew ? "5px" : 0,
                        paddingLeft: !isHebrew ? "10px" : 0,
                      }}
                    >
                      {t("pages.practiceHistory.retry")}
                    </Typography>
                  </Button>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
};
