import { apiClient as axios } from "@app-providers";
import { DifficultyLevel } from "@student/types";
import { useQuery } from "@tanstack/react-query";

export interface SummaryHistoryWithStudentDto {
  studentId: string;
  gameType: string;
  difficulty: DifficultyLevel;
  attemptsCount: number;
  totalSuccesses: number;
  totalFailures: number;
}

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
}

export interface Params {
  page: number;
  pageSize: number;
}

const DEFAULT_STALE_TIME = 5 * 60 * 1000; // 5 minutes

export const useGetStudentPracticeHistory = (params: Params) => {
  return useQuery({
    queryKey: ["student-practice-history", params],
    queryFn: async () => {
      const { data } = await axios.get<Paged<SummaryHistoryWithStudentDto>>(
        `${import.meta.env.VITE_GAMES_MANAGER_URL}/all-history`,
        { params: { page: params.page, pageSize: params.pageSize } },
      );
      return data;
    },
    staleTime: DEFAULT_STALE_TIME,
  });
};
