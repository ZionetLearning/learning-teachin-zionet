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

type PageParams = {
  page?: number;
  pageSize?: number;
  all?: false;
};

type AllParams = {
  all: true;
  pageSize?: number;
  maxPages?: number;
};

type Params = PageParams | AllParams;

const getStudentPracticeHistory = async (
  params?: Params,
): Promise<Paged<SummaryHistoryWithStudentDto>> => {
  const pageSize = params?.pageSize ?? 20;

  const getPage = async (page: number) => {
    const { data } = await axios.get<Paged<SummaryHistoryWithStudentDto>>(
      `${import.meta.env.VITE_GAMES_MANAGER_URL}/all-history`,
      {
        params: { page, pageSize },
      },
    );
    return data;
  };

  if (params?.all) {
    const maxPages = params?.maxPages ?? 100;
    const first = await getPage(1);
    const items: SummaryHistoryWithStudentDto[] = [...first.items];

    let hasNext = first.hasNextPage;
    let currentPage = 1;

    const computedTotalPages =
      first.totalCount > 0 ? Math.ceil(first.totalCount / pageSize) : undefined;

    while (
      hasNext &&
      currentPage < (computedTotalPages ?? Infinity) &&
      currentPage < maxPages
    ) {
      currentPage += 1;
      const next = await getPage(currentPage);
      items.push(...next.items);
      hasNext = next.hasNextPage;
    }

    return {
      items,
      page: 1,
      pageSize: items.length,
      totalCount: items.length,
      hasNextPage: false,
    };
  }

  const page = params?.page ?? 1;
  return getPage(page);
};

export const useGetStudentPracticeHistory = (params?: Params) => {
  return useQuery({
    queryKey: ["student-practice-history", params ?? {}],
    queryFn: () => getStudentPracticeHistory(params),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};
