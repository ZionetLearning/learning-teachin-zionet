export type DifficultyLabel = "Easy" | "Medium" | "Hard";

export const levelToLabel = (level: string | number): DifficultyLabel => {
  if (typeof level === "number") {
    const n = level;
    return n === 0 ? "Easy" : n === 1 ? "Medium" : "Hard";
  }
  const s = String(level).toLowerCase();
  if (s === "0" || s === "easy") return "Easy";
  if (s === "1" || s === "medium") return "Medium";
  return "Hard";
};

type CsvRow = {
  studentName: string;
  gameType: string;
  difficulty: string;
  attempts: number;
  successes: number;
  failures: number;
  successRate: string;
};

export const buildCsvHeaders = (t: (k: string) => string) => [
  {
    label: t("pages.studentPracticeHistory.columns.studentName"),
    key: "studentName",
  },
  {
    label: t("pages.studentPracticeHistory.columns.gameType"),
    key: "gameType",
  },
  {
    label: t("pages.studentPracticeHistory.columns.difficulty"),
    key: "difficulty",
  },
  {
    label: t("pages.studentPracticeHistory.columns.attempts"),
    key: "attempts",
  },
  {
    label: t("pages.studentPracticeHistory.columns.successes"),
    key: "successes",
  },
  {
    label: t("pages.studentPracticeHistory.columns.failures"),
    key: "failures",
  },
  {
    label: t("pages.studentPracticeHistory.columns.successRate"),
    key: "successRate",
  },
];

export const toCsvRow = (
  row: {
    studentId: string;
    gameType: string;
    difficulty: string | number;
    attemptsCount: number;
    totalSuccesses: number;
    totalFailures: number;
  },
  opts: {
    levelToLabel: (v: string | number) => string;
    rate: (s: number, a: number) => number;
  },
): CsvRow => {
  const rateNum = opts.rate(row.totalSuccesses, row.attemptsCount);
  return {
    studentName: row.studentId,
    gameType: row.gameType,
    difficulty: opts.levelToLabel(row.difficulty),
    attempts: row.attemptsCount,
    successes: row.totalSuccesses,
    failures: row.totalFailures,
    successRate: `${rateNum}%`,
  };
};
