import { useState } from "react";
import { useTranslation } from "react-i18next";
import {
  Box,
  Typography,
  ToggleButton,
  ToggleButtonGroup,
} from "@mui/material";
import { useAuth } from "@app-providers";
import { useStyles } from "./style";
import { SummaryTable, DetailedTable } from "./components";

type ViewMode = "summary" | "detailed";

export const PracticeHistory = () => {
  const { user } = useAuth();
  const { t, i18n } = useTranslation();
  const classes = useStyles();

  const studentId = user?.userId ?? "";
  const isHebrew = i18n.language === "he";

  const [view, setView] = useState<ViewMode>("summary");

  // separate pagination per view
  const [summaryPage, setSummaryPage] = useState(0);
  const [detailedPage, setDetailedPage] = useState(0);

  const [summaryRowsPerPage, setSummaryRowsPerPage] = useState(10);
  const [detailedRowsPerPage, setDetailedRowsPerPage] = useState(10);
  return (
    <Box>
      <Box className={classes.headerWrapper} sx={{ py: 3 }}>
        <Typography className={classes.title}>
          {t("pages.practiceHistory.title")}
        </Typography>
        <Typography className={classes.description} sx={{ mt: 0.5 }}>
          {t("pages.practiceHistory.description")}
        </Typography>

        <Box className={classes.toggleGroupWrapper}>
          <ToggleButtonGroup
            exclusive
            size="small"
            value={view}
            onChange={(_, v) => v && setView(v)}
            className={classes.toggleGroup}
          >
            <ToggleButton value="summary">
              {t("pages.practiceHistory.summary")}
            </ToggleButton>
            <ToggleButton value="detailed">
              {t("pages.practiceHistory.detailed")}
            </ToggleButton>
          </ToggleButtonGroup>
        </Box>
      </Box>

      {view === "summary" ? (
        <SummaryTable
          studentId={studentId}
          page={summaryPage}
          rowsPerPage={summaryRowsPerPage}
          onPageChange={(p) => setSummaryPage(p)}
          onRowsPerPageChange={(r) => {
            setSummaryRowsPerPage(r);
            setSummaryPage(0);
          }}
        />
      ) : (
        <DetailedTable
          studentId={studentId}
          page={detailedPage}
          rowsPerPage={detailedRowsPerPage}
          onPageChange={(p) => setDetailedPage(p)}
          onRowsPerPageChange={(r) => {
            setDetailedRowsPerPage(r);
            setDetailedPage(0);
          }}
          isHebrew={isHebrew}
        />
      )}
    </Box>
  );
};
