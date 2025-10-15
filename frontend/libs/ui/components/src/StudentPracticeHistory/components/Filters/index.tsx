import DownloadIcon from "@mui/icons-material/Download";
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Tooltip,
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
  studentId: string;
  setStudentId: (v: string) => void;
  studentIds: string[];

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
};

export const StudentPracticeFilters = ({
  isDisabled,
  studentId,
  setStudentId,
  studentIds,
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
        <InputLabel id="filter-student-id-label">
          {t("pages.studentPracticeHistory.filters.student")}
        </InputLabel>
        <Select
          labelId="filter-student-id-label"
          label={t("pages.studentPracticeHistory.filters.student")}
          value={studentId}
          onChange={(e) => {
            setStudentId(e.target.value as string);
            onAnyChange?.();
          }}
        >
          <MenuItem value="all">
            {t("pages.studentPracticeHistory.filters.all")}
          </MenuItem>
          {studentIds.map((sid) => (
            <MenuItem key={sid} value={sid}>
              {sid}
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

      <Box flexGrow={1} />

      <Tooltip
        title={
          t("pages.studentPracticeHistory.exportCurrentPage") ||
          "Export current page"
        }
      >
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
    </Stack>
  );
};
