import { useQuery, type UseQueryResult } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";

export type RoleMode = "Teacher" | "Student";

export type Member = {
  memberId: string;
  name: string;
  role: number; // or "Teacher" | "Student" if your backend returns strings
};

export type ClassSummary = {
  classId: string;
  name: string;
};

export type ClassItem = {
  classId: string;
  name: string;
  members: Member[];
};

const CLASSES_BASE_URL = import.meta.env.VITE_CLASSES_MANAGER_URL;

// GET /me/classes?role=Teacher|Student
export const useMyClasses = (
  mode: RoleMode,
): UseQueryResult<ClassSummary[], Error> => {
  return useQuery({
    queryKey: ["me", "classes", mode],
    staleTime: 60_000,
    queryFn: async () => {
      const res = await axios.get<ClassSummary[]>(
        `${CLASSES_BASE_URL}/me/classes`,
        { params: { role: mode } },
      );
      return res.data;
    },
  });
};

// GET /classes/{classId}/students or /classes/{classId}/teachers
export const useClassMembers = (
  classId: string | undefined,
  mode: RoleMode,
  opts: { enabled?: boolean } = {},
): UseQueryResult<Member[], Error> => {
  const enabled = Boolean(classId) && (opts.enabled ?? true);
  return useQuery({
    queryKey: ["classMembers", classId, mode],
    enabled,
    queryFn: async () => {
      if (!classId) throw new Error("Missing classId");
      const suffix = mode === "Teacher" ? "students" : "teachers";
      const res = await axios.get<Member[]>(
        `${CLASSES_BASE_URL}/classes/${encodeURIComponent(classId)}/${suffix}`,
      );
      return res.data;
    },
  });
};
