import { useQuery, UseQueryResult } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import type {
  GetPeriodOverviewResponse,
  GetPeriodOverviewParams,
} from "../types/summary";

const SUMMARY_URL = import.meta.env.VITE_SUMMARY_MANAGER_URL;
const SUMMARY_STALE_TIME = 1000 * 60 * 5; // 5 minutes

export const useGetPeriodOverview = (
  params: GetPeriodOverviewParams,
): UseQueryResult<GetPeriodOverviewResponse, Error> => {
  const { userId, startDate, endDate } = params;

  return useQuery<GetPeriodOverviewResponse, Error>({
    queryKey: ["periodOverview", userId, startDate, endDate],
    queryFn: async () => {
      if (!userId) throw new Error("Missing userId");

      const queryParams = new URLSearchParams();
      if (startDate) queryParams.append("startDate", startDate);
      if (endDate) queryParams.append("endDate", endDate);

      const queryString = queryParams.toString();
      const url = `${SUMMARY_URL}/summary/${userId}/overview${queryString ? `?${queryString}` : ""}`;

      const { data } = await axios.get<GetPeriodOverviewResponse>(url);
      return data;
    },
    enabled: !!userId,
    staleTime: SUMMARY_STALE_TIME,
    refetchOnWindowFocus: false,
  });
};
