import { useQuery, type UseQueryResult } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";

export type Member = {
  memberId: string;
  name: string;
  role: number; // 1 = teacher, 0 = student
};

export type ClassItem = {
  classId: string;
  name: string;
  members: Member[];
};

const BASE = import.meta.env.VITE_CLASSES_MANAGER_URL;

// Backend decides if user is Teacher or Student based on JWT

export const useMyClasses = (): UseQueryResult<ClassItem[], Error> => {
  return useQuery({
    queryKey: ["my", "classes"],
    staleTime: 60_000,
    queryFn: async () => {
      const res = await axios.get<ClassItem[]>(`${BASE}/my`);
      return res.data;
    },
  });
};
