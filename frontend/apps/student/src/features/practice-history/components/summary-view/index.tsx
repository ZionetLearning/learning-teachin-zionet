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
import { getDifficultyLabel } from "@student/features/practice/utils";
import { SummaryData, DetailedData } from "../../types";

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

  if (!summaryData) return null;

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
                  detail.difficulty === item.difficulty
              )
              .sort((a, b) => b.attemptNumber - a.attemptNumber)[0];

            return (
              <TableRow key={index} hover>
                <TableCell align="right">
                  <Box>
                    <Typography variant="body2" fontWeight="medium">
                      {t(
                        `pages.practiceHistory.practiceTools.${item.gameType}`
                      )}
                      {lastAttempt?.correctAnswer?.length ? (
                        <>: "{lastAttempt.correctAnswer.join(" ")}"</>
                      ) : null}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {getDifficultyLabel(
                        Number(item.difficulty) as 0 | 1 | 2,
                        t
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
                    sx={{
                      gap: 1.5,
                      display: "flex",
                      justifyContent: "center",
                      px: 1,
                      minWidth: "unset",
                    }}
                    variant="contained"
                    size="small"
                    startIcon={!isHebrew && <Play size={16} />}
                    endIcon={isHebrew && <Play size={16} />}
                    onClick={() =>
                      console.log("Retry game:", item.gameType, item.difficulty)
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