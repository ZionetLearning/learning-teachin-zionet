import { useMutation, useQuery, UseQueryResult } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";

export type AttemptStatus = "Success" | "Failure" | "Pending";
export type GameDifficulty = "Easy" | "Medium" | "Hard";

// Game Attempt - Submit Attempt
export type GameAttemptRequest = {
  exerciseId: string;
  givenAnswer: string[];
};

export type GameAttemptResponse = {
  attemptId: string;
  exerciseId: string;
  studentId: string;
  gameType: string;
  difficulty: GameDifficulty;
  status: AttemptStatus;
  correctAnswer: string[];
  attemptNumber: number;
  accuracy: number;
};

// --------------

// Game History Summary
export type GameHistorySummaryItem = {
  gameType: string;
  difficulty: GameDifficulty;
  attemptsCount: number;
  totalSuccesses: number;
  totalFailures: number;
};

export type GameHistorySummaryResponse = {
  items: GameHistorySummaryItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
};

// --------------

// Game History Detailed
export type GameHistoryDetailedItem = {
  exerciseId: string;
  attemptId: string;
  gameType: string;
  difficulty: GameDifficulty;
  givenAnswer: string[];
  correctAnswer: string[];
  status: AttemptStatus;
  accuracy: number;
  createdAt: string; // ISO timestamp
};

export type GameHistoryDetailedResponse = {
  items: GameHistoryDetailedItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
};

type GetGameArgs = {
  studentId?: string;
  page?: number; // 0-indexed page number
  pageSize?: number;
};

// ------------

// Game Mistakes
export type MistakeAttemptDto = {
  attemptId: string;
  wrongAnswer: string[];
  accuracy: number;
  createdAt: string; // ISO timestamp
};

export type GameMistakeItem = {
  exerciseId: string;
  gameType: string;
  difficulty: GameDifficulty;
  correctAnswer: string[];
  mistakes: MistakeAttemptDto[];
};

export type GameMistakesResponse = {
  items: GameMistakeItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
};

export const useSubmitGameAttempt = () => {
  const GAMES_MANAGER_URL = import.meta.env.VITE_GAMES_MANAGER_URL;

  return useMutation<GameAttemptResponse, Error, GameAttemptRequest>({
    mutationFn: async (body: GameAttemptRequest) => {
      const res = await axios.post<GameAttemptResponse>(
        `${GAMES_MANAGER_URL}/attempt`,
        body,
      );
      return res.data;
    },
    onError: (error) => {
      console.error("Failed to submit attempt:", error);
      toast.error("Your answer couldn't be submitted. Please try again.");
    },
  });
};

export const useGetGameHistorySummary = ({
  studentId,
  page = 0,
  pageSize = 10,
}: GetGameArgs) => {
  const GAMES_MANAGER_URL = import.meta.env.VITE_GAMES_MANAGER_URL;

  return useQuery<GameHistorySummaryResponse, Error>({
    queryKey: ["gamesHistorySummary", { studentId, page, pageSize }],
    queryFn: async () => {
      if (!studentId) throw new Error("Missing studentId");

      const res = await axios.get<GameHistorySummaryResponse>(
        `${GAMES_MANAGER_URL}/history/${encodeURIComponent(studentId)}`,
        { params: { summary: true, page, pageSize, getPending: false } },
      );
      return res.data;
    },
    enabled: Boolean(studentId),
    staleTime: 60_000,
  });
};

export const useGetGameHistoryDetailed = ({
  studentId,
  page = 0,
  pageSize = 10,
}: GetGameArgs): UseQueryResult<GameHistoryDetailedResponse, Error> => {
  const GAMES_MANAGER_URL = import.meta.env.VITE_GAMES_MANAGER_URL!;

  return useQuery<GameHistoryDetailedResponse, Error>({
    queryKey: ["gamesHistoryDetailed", { studentId, page, pageSize }] as const,
    staleTime: 60_000,
    queryFn: async () => {
      if (!studentId) throw new Error("Missing studentId");
      const res = await axios.get<GameHistoryDetailedResponse>(
        `${GAMES_MANAGER_URL}/history/${encodeURIComponent(studentId)}`,
        { params: { summary: false, page, pageSize, getPending: false } },
      );
      return res.data;
    },
    enabled: Boolean(studentId),
  });
};

export const useGetGameMistakes = ({
  studentId,
  page = 0,
  pageSize = 10,
}: GetGameArgs): UseQueryResult<GameMistakesResponse, Error> => {
  const GAMES_MANAGER_URL = import.meta.env.VITE_GAMES_MANAGER_URL!;

  return useQuery<GameMistakesResponse, Error>({
    queryKey: ["gamesMistakes", { studentId, page, pageSize }] as const,
    enabled: Boolean(studentId),
    staleTime: 60_000,
    queryFn: async () => {
      if (!studentId) throw new Error("Missing studentId");
      const res = await axios.get<GameMistakesResponse>(
        `${GAMES_MANAGER_URL}/mistakes/${encodeURIComponent(studentId)}`,
        { params: { page, pageSize } },
      );
      return res.data;
    },
  });
};
