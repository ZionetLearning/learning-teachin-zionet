import DownloadIcon from "@mui/icons-material/Download";
import ClearIcon from "@mui/icons-material/Clear";
import {
  Box,
  Button,
  TextField,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Tooltip,
  IconButton,
  InputAdornment,
} from "@mui/material";
import { CSVLink } from "react-csv";
import { useTranslation } from "react-i18next";

import { DifficultyLabel } from "../../utils";
import { useStyles } from "./style";

export type DifficultyFilter = "all" | DifficultyLabel;
type CsvPrimitive = string | number | boolean | null | undefined;
type CsvRow = Record<string, CsvPrimitive>;

type StudentPracticeFiltersProps = {
  isDisabled: boolean;
  studentName: string;
  setStudentName: (v: string) => void;
  studentNames: string[];
  gameType: string;
  setGameType: (v: string) => void;
  gameTypes: string[];
  difficulty: DifficultyFilter;
  setDifficulty: (v: DifficultyFilter) => void;
  difficulties: DifficultyLabel[];
  csvHeaders: {
    label: string;
    key: string;
  }[];
  csvPage: ReadonlyArray<CsvRow>;
  filename: string;
  onAnyChange?: () => void;
  dateFrom?: string;
  setDateFrom?: (v: string | undefined) => void;
  dateTo?: string;
  setDateTo?: (v: string | undefined) => void;
};

export const StudentPracticeFilters = ({
  isDisabled,
  studentName,
  setStudentName,
  studentNames,
  gameType,
  setGameType,
  gameTypes,
  difficulty,
  setDifficulty,
  difficulties,
  csvHeaders,
  csvPage,
  filename,
  onAnyChange,
  dateFrom,
  setDateFrom,
  dateTo,
  setDateTo,
}: StudentPracticeFiltersProps) => {
  const classes = useStyles();
  const { t } = useTranslation();

  return (
    <Stack direction="row" className={classes.filtersRow}>
      <FormControl
        size="small"
        className={classes.filterControl}
        disabled={isDisabled}
      >
        <InputLabel id="filter-student-name-label">
          {t("pages.studentPracticeHistory.filters.student")}
        </InputLabel>
        <Select
          labelId="filter-student-name-label"
          label={t("pages.studentPracticeHistory.filters.student")}
          value={studentName}
          onChange={(e) => {
            setStudentName(e.target.value as string);
            onAnyChange?.();
          }}
        >
          <MenuItem value="all">
            {t("pages.studentPracticeHistory.filters.all")}
          </MenuItem>
          {studentNames.map((name) => (
            <MenuItem key={name} value={name}>
              {name}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      <FormControl
        size="small"
        className={classes.filterControl}
        disabled={isDisabled}
      >
        <InputLabel id="filter-game-type-label">
          {t("pages.studentPracticeHistory.filters.gameType")}
        </InputLabel>
        <Select
          labelId="filter-game-type-label"
          label={t("pages.studentPracticeHistory.filters.gameType")}
          value={gameType}
          onChange={(e) => {
            setGameType(e.target.value as string);
            onAnyChange?.();
          }}
        >
          <MenuItem value="all">
            {t("pages.studentPracticeHistory.filters.all")}
          </MenuItem>
          {gameTypes.map((gt) => (
            <MenuItem key={gt} value={gt}>
              {gt}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      <FormControl
        size="small"
        className={classes.filterControl}
        disabled={isDisabled}
      >
        <InputLabel id="filter-difficulty-label">
          {t("pages.studentPracticeHistory.filters.difficulty")}
        </InputLabel>
        <Select
          labelId="filter-difficulty-label"
          label={t("pages.studentPracticeHistory.filters.difficulty")}
          value={difficulty}
          onChange={(e) => {
            setDifficulty(e.target.value as DifficultyFilter);
            onAnyChange?.();
          }}
        >
          <MenuItem value="all">
            {t("pages.studentPracticeHistory.filters.all")}
          </MenuItem>
          {difficulties.map((d) => (
            <MenuItem key={d} value={d}>
              {d}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      <Box className={classes.dateGroup}>
        <TextField
          type="date"
          size="small"
          label={t("pages.studentPracticeHistory.filters.from")}
          value={dateFrom ?? ""}
          onChange={(e) => {
            setDateFrom?.(e.target.value || undefined);
            onAnyChange?.();
          }}
          className={classes.filterControl}
          disabled={isDisabled}
          slotProps={{
            inputLabel: { shrink: true },
            input: {
              endAdornment: (
                <InputAdornment position="end">
                  <Tooltip
                    title={
                      t("pages.studentPracticeHistory.filters.clear") || "Clear"
                    }
                  >
                    <span>
                      <IconButton
                        size="small"
                        aria-label={
                          t("pages.studentPracticeHistory.filters.clear") ||
                          "Clear"
                        }
                        onClick={() => {
                          setDateFrom?.(undefined);
                          onAnyChange?.();
                        }}
                        disabled={isDisabled || !dateFrom}
                      >
                        <ClearIcon fontSize="small" />
                      </IconButton>
                    </span>
                  </Tooltip>
                </InputAdornment>
              ),
            },
          }}
        />

        <TextField
          type="date"
          size="small"
          label={t("pages.studentPracticeHistory.filters.to")}
          value={dateTo ?? ""}
          onChange={(e) => {
            setDateTo?.(e.target.value || undefined);
            onAnyChange?.();
          }}
          className={classes.filterControl}
          disabled={isDisabled}
          slotProps={{
            inputLabel: { shrink: true },
            input: {
              endAdornment: (
                <InputAdornment position="end">
                  <Tooltip
                    title={t("pages.studentPracticeHistory.filters.clear")}
                  >
                    <span>
                      <IconButton
                        size="small"
                        aria-label={t(
                          "pages.studentPracticeHistory.filters.clear",
                        )}
                        onClick={() => {
                          setDateTo?.(undefined);
                          onAnyChange?.();
                        }}
                        disabled={isDisabled || !dateTo}
                      >
                        <ClearIcon fontSize="small" />
                      </IconButton>
                    </span>
                  </Tooltip>
                </InputAdornment>
              ),
            },
          }}
        />
      </Box>
      <Box className={classes.actions}>
        <Tooltip title={t("pages.studentPracticeHistory.filters.resetAll")}>
          <span>
            <Button
              variant="outlined"
              size="small"
              disabled={isDisabled}
              onClick={() => {
                setStudentName("all");
                setGameType("all");
                setDifficulty("all");
                setDateFrom?.(undefined);
                setDateTo?.(undefined);
                onAnyChange?.();
              }}
              className={classes.filterControl}
            >
              {t("pages.studentPracticeHistory.filters.resetAll")}
            </Button>
          </span>
        </Tooltip>

        <Tooltip title={t("pages.studentPracticeHistory.exportCurrentPage")}>
          <span>
            <Button
              variant="contained"
              size="small"
              startIcon={<DownloadIcon />}
              disabled={isDisabled || csvPage.length === 0}
              component={CSVLink as unknown as React.ElementType}
              headers={csvHeaders}
              data={csvPage}
              filename={filename}
              uFEFF
              target="_blank"
            >
              {t("pages.studentPracticeHistory.exportPage")}
            </Button>
          </span>
        </Tooltip>
      </Box>
    </Stack>
  );
};
