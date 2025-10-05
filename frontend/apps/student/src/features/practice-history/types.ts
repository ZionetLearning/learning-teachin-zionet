export interface GameAttempt {
  attemptId: string;
  gameType: string;
  difficulty: string;
  attemptNumber: number;
  givenAnswer: string[];
  correctAnswer: string[];
  status: "Success" | "Failure" | "Pending";
  createdAt: string;
}

export interface SummaryItem {
  gameType: string;
  difficulty: string;
  totalSuccesses: number;
  totalFailures: number;
}

export interface HistoryData<T> {
  items: T[];
  totalCount: number;
  hasNextPage: boolean;
}

export type SummaryData = HistoryData<SummaryItem>;
export type DetailedData = HistoryData<GameAttempt>;