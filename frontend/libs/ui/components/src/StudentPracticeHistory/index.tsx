import { useMemo, useState } from "react";

import { TablePagination } from "@mui/material";
import { useTranslation } from "react-i18next";

import {
  SummaryHistoryWithStudentDto,
  useGetStudentPracticeHistory,
} from "@api";
import {
  DifficultyFilter,
  StudentPracticeFilters,
  StudentPracticeTable,
} from "./components";
import { useStyles } from "./style";
import {
  buildCsvHeaders,
  DifficultyLabel,
  levelToLabel,
  toCsvRow,
} from "./utils";

const ORDER_INDEX: Record<DifficultyLabel, number> = {
  Easy: 0,
  Medium: 1,
  Hard: 2,
};

type StudentGroup = {
  studentId: string;
  items: SummaryHistoryWithStudentDto[];
  totals: {
    attempts: number;
    successes: number;
    failures: number;
    ratePct: number;
    gameTypes: string[];
    difficulties: DifficultyFilter[];
  };
};

export const StudentPracticeHistory = () => {
  const classes = useStyles();
  const { t } = useTranslation();

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [gameType, setGameType] = useState("all");
  const [studentId, setStudentId] = useState("all");
  const [difficulty, setDifficulty] = useState<"all" | DifficultyLabel>("all");

  const { data, isLoading, isError } = useGetStudentPracticeHistory({
    page: page + 1,
    pageSize: rowsPerPage,
  });

  const handlePageChange = (_: unknown, newPage: number) => setPage(newPage);
  const handleRowsPerPageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(e.target.value, 10));
    setPage(0);
  };

  const allItems = useMemo(() => data?.items ?? [], [data]);

  const gameTypes = useMemo(
    () => Array.from(new Set(allItems.map((i) => i.gameType))).sort(),
    [allItems],
  );

  const difficulties = useMemo(() => {
    const present = new Set(allItems.map((i) => levelToLabel(i.difficulty)));
    return Object.keys(ORDER_INDEX).filter((d) =>
      present.has(d as DifficultyLabel),
    );
  }, [allItems]);

  const studentIds = useMemo(
    () => Array.from(new Set(allItems.map((i) => i.studentId))).sort(),
    [allItems],
  );

  const filtered = useMemo(
    () =>
      allItems.filter(
        (it) =>
          (studentId === "all" || it.studentId === studentId) &&
          (gameType === "all" || it.gameType === gameType) &&
          (difficulty === "all" || levelToLabel(it.difficulty) === difficulty),
      ),
    [allItems, studentId, gameType, difficulty],
  );

  const grouped: StudentGroup[] = useMemo(() => {
    const map = new Map<string, StudentGroup>();
    for (const it of filtered) {
      const key = it.studentId;
      if (!map.has(key)) {
        map.set(key, {
          studentId: key,
          items: [],
          totals: {
            attempts: 0,
            successes: 0,
            failures: 0,
            ratePct: 0,
            gameTypes: [],
            difficulties: [],
          },
        });
      }
      const g = map.get(key)!;
      g.items.push(it);
    }
    for (const g of map.values()) {
      const attempts = g.items.reduce((s, i) => s + i.attemptsCount, 0);
      const successes = g.items.reduce((s, i) => s + i.totalSuccesses, 0);
      const failures = g.items.reduce((s, i) => s + i.totalFailures, 0);
      const ratePct =
        attempts > 0 ? Math.round((successes / attempts) * 100) : 0;
      const gameTypes = Array.from(
        new Set(g.items.map((i) => i.gameType)),
      ).sort();
      const diffs = Array.from(
        new Set(
          g.items.map((i) => levelToLabel(i.difficulty) as DifficultyLabel),
        ),
      ).sort((a, b) => ORDER_INDEX[a] - ORDER_INDEX[b]);

      g.totals = {
        attempts,
        successes,
        failures,
        ratePct,
        gameTypes,
        difficulties: diffs,
      };
    }
    return Array.from(map.values()).sort((a, b) =>
      a.studentId.localeCompare(b.studentId),
    );
  }, [filtered]);

  const pagedItems = useMemo(
    () => grouped.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage),
    [grouped, page, rowsPerPage],
  );

  const csvHeaders = useMemo(() => buildCsvHeaders(t), [t]);

  const csvPage = useMemo(
    () =>
      pagedItems.flatMap((g) =>
        g.items.map((it) =>
          toCsvRow(it, {
            levelToLabel,
            rate: (s, a) => (a > 0 ? Math.round((s / a) * 100) : 0),
          }),
        ),
      ),
    [pagedItems],
  );

  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);

  const filenamePrefix = useMemo(() => {
    const gt = gameType === "all" ? "all-games" : gameType;
    const dl = difficulty === "all" ? "all-difficulties" : difficulty;
    return `practice-history_${gt}_${dl}_${today}`;
  }, [gameType, difficulty, today]);

  const total = grouped.length;

  return (
    <div className={classes.listContainer} data-testid="history-list">
      <h2 className={classes.sectionTitle}>
        {t("pages.studentPracticeHistory.title")}
      </h2>
      <StudentPracticeFilters
        isDisabled={isLoading || isError}
        studentId={studentId}
        setStudentId={setStudentId}
        studentIds={studentIds}
        gameType={gameType}
        setGameType={setGameType}
        gameTypes={gameTypes}
        difficulty={difficulty}
        setDifficulty={setDifficulty}
        difficulties={difficulties as ("Easy" | "Medium" | "Hard")[]}
        csvHeaders={csvHeaders}
        csvPage={csvPage}
        filename={`${filenamePrefix}_page-${page + 1}_rpp-${rowsPerPage}.csv`}
        onAnyChange={() => setPage(0)}
      />
      <StudentPracticeTable
        pagedItems={pagedItems}
        isLoading={isLoading}
        isError={isError}
      />
      <TablePagination
        component="div"
        className={classes.paginationBar}
        data-testid="history-pagination"
        count={total}
        page={page}
        onPageChange={handlePageChange}
        rowsPerPage={rowsPerPage}
        onRowsPerPageChange={handleRowsPerPageChange}
        rowsPerPageOptions={[10, 20, 50, 100]}
        labelRowsPerPage={t("pages.studentPracticeHistory.rowsPerPage")}
        labelDisplayedRows={({ from, to, count }) =>
          `${from}-${to} ${t("pages.studentPracticeHistory.of")} ${count !== -1 ? count : to}`
        }
      />
    </div>
  );
};
