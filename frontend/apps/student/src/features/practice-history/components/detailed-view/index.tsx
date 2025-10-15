import { useState } from "react";
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Typography,
  Chip,
  IconButton,
  Collapse,
} from "@mui/material";
import { ChevronDown, ChevronUp } from "lucide-react";
import { useTranslation } from "react-i18next";
import { getDifficultyLabel } from "@student/features/practice/utils";
import { DetailedData, GameAttempt } from "../../types";
import { useStyles } from "./style";
import { getStatusColor, formatDate } from "../../utils";

interface DetailedViewProps {
  detailedData: DetailedData | undefined;
}

interface GroupedAttempts {
  gameType: string;
  difficulty: string;
  attempts: GameAttempt[];
}

export const DetailedView = ({ detailedData }: DetailedViewProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const [expandedGames, setExpandedGames] = useState<Set<string>>(new Set());

  const toggleExpanded = (gameKey: string) => {
    setExpandedGames((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(gameKey)) {
        newSet.delete(gameKey);
      } else {
        newSet.add(gameKey);
      }
      return newSet;
    });
  };

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

  const groupedAttempts = detailedData.items.reduce<
    Record<string, GroupedAttempts>
  >((acc, item) => {
    const key = `${item.gameType}-${item.difficulty}`;
    if (!acc[key]) {
      acc[key] = {
        gameType: item.gameType,
        difficulty: item.difficulty,
        attempts: [],
      };
    }
    acc[key].attempts.push(item);
    return acc;
  }, {});

  return (
    <Box>
      {Object.entries(groupedAttempts).map(([key, group]) => {
        const isExpanded = expandedGames.has(key);
        // Filter out attempts with no answer
        const validAttempts = group.attempts.filter(
          (attempt) => attempt.givenAnswer.length > 0,
        );

        // Don't show groups with no valid attempts
        if (validAttempts.length === 0) return null;

        return (
          <Paper key={key} sx={{ mb: 2 }}>
            <Box
              className={classes.container}
              onClick={() => toggleExpanded(key)}
            >
              <Box className={classes.innerContainer}>
                <Box>
                  <Typography variant="h6">
                    {t(`pages.practiceHistory.practiceTools.${group.gameType}`)}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {getDifficultyLabel(
                      Number(group.difficulty) as 0 | 1 | 2,
                      t,
                    )}
                  </Typography>
                </Box>
                <Typography variant="body2" color="text.secondary">
                  {t("pages.practiceHistory.attemptsCount", {
                    count: validAttempts.length,
                  })}
                </Typography>
              </Box>
              <IconButton size="small">
                {isExpanded ? <ChevronUp /> : <ChevronDown />}
              </IconButton>
            </Box>

            <Collapse in={isExpanded}>
              <TableContainer
                className={classes.tableContainer}
              >
                <Table size="small" stickyHeader>
                  <TableHead>
                    <TableRow>
                      <TableCell align="right">
                        {t("pages.practiceHistory.attemptNumber")}
                      </TableCell>
                      <TableCell align="right">
                        {t("pages.practiceHistory.answer")}
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
                    {validAttempts.map((attempt) => {
                      console.log("Attempt data:", attempt);
                      return (
                        <TableRow key={attempt.attemptId}>
                          <TableCell align="right">
                            {attempt.attemptNumber}
                          </TableCell>
                          <TableCell align="right" dir="rtl">
                            {attempt.givenAnswer.join(", ")}
                          </TableCell>
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
                          <TableCell align="right">
                            {formatDate(attempt.createdAt)}
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </TableContainer>
            </Collapse>
          </Paper>
        );
      })}
    </Box>
  );
};