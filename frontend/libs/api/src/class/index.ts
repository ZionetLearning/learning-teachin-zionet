import { useQuery, type UseQueryResult } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";

export type Role = "Teacher" | "Student";

export type ClassSummary = {
  classId: string;
  name: string;
  // add fields if your API returns more
};

export type Member = {
  memberId: string;
  name: string;
  // role/email/avatar etc. can be added later without breaking callers
};

const BASE = import.meta.env.VITE_CLASSES_MANAGER_URL;

// Query Keys
const qk = {
  myClasses: (role: Role) => ["classes", "me", role] as const,
  classStudents: (classId: string) =>
    ["classes", "members", "students", classId] as const,
  classTeachers: (classId: string) =>
    ["classes", "members", "teachers", classId] as const,
};

// GET /me/classes?role=Teacher|Student
export const useMyClasses = (
  role: Role,
  opts?: { enabled?: boolean },
): UseQueryResult<ClassSummary[], Error> => {
  return useQuery({
    queryKey: qk.myClasses(role),
    enabled: opts?.enabled ?? true,
    staleTime: 60_000,
    gcTime: 5 * 60_000,
    queryFn: async () => {
      const res = await axios.get<ClassSummary[]>(`${BASE}/me/classes`, {
        params: { role },
      });
      // sort by name for stable UI
      return [...res.data].sort((a, b) => a.name.localeCompare(b.name));
    },
  });
};

// GET /classes/{classId}/students
export const useClassStudents = (
  classId: string,
  opts?: { enabled?: boolean },
): UseQueryResult<Member[], Error> => {
  return useQuery({
    queryKey: qk.classStudents(classId),
    enabled: (opts?.enabled ?? true) && Boolean(classId),
    staleTime: 60_000,
    gcTime: 5 * 60_000,
    queryFn: async () => {
      const res = await axios.get<Member[]>(
        `${BASE}/classes/${encodeURIComponent(classId)}/students`,
      );
      return res.data;
    },
  });
};

// GET /classes/{classId}/teachers
export const useClassTeachers = (
  classId: string,
  opts?: { enabled?: boolean },
): UseQueryResult<Member[], Error> => {
  return useQuery({
    queryKey: qk.classTeachers(classId),
    enabled: (opts?.enabled ?? true) && Boolean(classId),
    staleTime: 60_000,
    gcTime: 5 * 60_000,
    queryFn: async () => {
      const res = await axios.get<Member[]>(
        `${BASE}/classes/${encodeURIComponent(classId)}/teachers`,
      );
      return res.data;
    },
  });
};
