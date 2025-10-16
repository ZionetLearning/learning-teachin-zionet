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
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { SummaryData } from "../../types";

interface SummaryViewProps {
  summaryData: SummaryData | undefined;

}

export const SummaryView = ({
  summaryData,
}: SummaryViewProps) => {
  const { t } = useTranslation();
  if (!summaryData) return null;

  return (
    <TableContainer component={Paper} sx={{ mt: 2 }}>
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
              {t("pages.practiceHistory.attemptsCount")}
            </TableCell>
            <TableCell align="right">
              {t("pages.practiceHistory.successesCount")}
            </TableCell>
            <TableCell align="right">
              {t("pages.practiceHistory.failuresCount")}
            </TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {summaryData.items.map((item, index) => {
            return (
              <TableRow key={index} hover>
                <TableCell align="right">
                  <Box>
                    <Typography variant="body2" fontWeight="medium">
                      {t(
                        `pages.practiceHistory.practiceTools.${item.gameType}`,
                      )}
                    </Typography>
                  </Box>
                </TableCell>

                <TableCell align="right">
                  <Typography variant="body2">
                    {t(`pages.practiceHistory.${item.difficulty.toLowerCase()}`)}
                  </Typography>
                </TableCell>

                <TableCell align="right">
                  <Typography variant="body2">
                    {item?.attemptsCount ?? (item.totalSuccesses + item.totalFailures)}
                  </Typography>
                </TableCell>

                <TableCell align="right">
                  <Typography variant="body2">
                    {item.totalSuccesses}
                  </Typography>
                </TableCell>

                <TableCell align="right">
                  <Typography variant="body2">
                    {item.totalFailures}
                  </Typography>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
};
