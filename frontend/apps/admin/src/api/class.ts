import {
  useMutation,
  useQuery,
  UseQueryResult,
  useQueryClient,
} from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";

export type ClassMember = {
  memberId: string;
  name: string;
  role: number;
};

export type ClassItem = {
  classId: string;
  name: string;
  members: ClassMember[];
};

export type ClassSummary = {
  classId: string;
  name: string;
  // optional if your list endpoint doesn't return members
  members?: ClassMember[];
};

export type GetClassResponse = ClassItem;

export type CreateClassRequest = {
  name: string;
  description?: string | null;
};

export type CreateClassResponse = {
  classId: string;
  name: string;
  code: string;
  description: string | null;
  createdAt: string; // ISO
};

export type AddMembersRequest = {
  classId: string;
  userIds: string[];
  addedBy: string; // userId
};

export type RemoveMembersRequest = {
  classId: string;
  userIds: string[];
};

export type BasicMessageResponse = {
  message: string;
};

const CLASSES_BASE_URL = import.meta.env.VITE_CLASSES_MANAGER_URL;

// NOT SUPPORTED YET BY THE BACKEND
// export const useGetAllClasses = (): UseQueryResult<ClassSummary[], Error> => {
//   return useQuery<ClassSummary[], Error>({
//     queryKey: ["classes"],
//     staleTime: 60_000,
//     queryFn: async () => {
//       const res = await axios.get<ClassSummary[]>(`${CLASSES_BASE_URL}`);
//       return res.data;
//     },
//   });
// };

const TEMP_CLASS_ID = "0176619e-6acc-4b02-85f1-b38bbef8d230";

// FAKE getAllClasses implementation until backend is ready
export const useGetAllClasses = (): UseQueryResult<ClassSummary[], Error> => {
  return useQuery<ClassSummary[], Error>({
    queryKey: ["classes"],
    queryFn: async () => {
      const res = await axios.get<GetClassResponse>(
        `${CLASSES_BASE_URL}/${encodeURIComponent(TEMP_CLASS_ID)}`,
      );
      const cls = res.data;
      // Map to summary shape; members may not be present on some endpoints
      const summary: ClassSummary = {
        classId: cls.classId,
        name: cls.name,
        members: cls.members, // optional in type, safe to pass if present
      };
      return [summary];
    },
  });
};

export const useGetClass = (
  classId: string,
): UseQueryResult<GetClassResponse, Error> => {
  return useQuery<GetClassResponse, Error>({
    queryKey: ["class", classId] as const,
    queryFn: async () => {
      if (!classId) throw new Error("Missing classId");
      const res = await axios.get<GetClassResponse>(
        `${CLASSES_BASE_URL}/${encodeURIComponent(classId)}`,
      );
      return res.data;
    },
  });
};

export const useCreateClass = () => {
  const qc = useQueryClient();

  return useMutation<CreateClassResponse, Error, CreateClassRequest>({
    mutationFn: async (body) => {
      const res = await axios.post<CreateClassResponse>(
        `${CLASSES_BASE_URL}`,
        body,
      );
      return res.data;
    },
    onSuccess: (created) => {
      toast.success("Class created successfully.");
      // refresh cache for this class
      qc.setQueryData<GetClassResponse>(["class", created.classId], {
        classId: created.classId,
        name: created.name,
        members: [],
      });
    },
    onError: (error) => {
      console.error("Failed to create class:", error);
      toast.error("Failed to create class. Please try again.");
    },
  });
};

export const useDeleteClass = () => {
  const qc = useQueryClient();

  return useMutation<void, Error, { classId: string }>({
    mutationFn: async ({ classId }) => {
      if (!classId) throw new Error("Missing classId");
      await axios.delete(`${CLASSES_BASE_URL}/${encodeURIComponent(classId)}`);
    },
    onSuccess: (_data, { classId }) => {
      toast.success("Class deleted.");
      // Invalidate any cached copies of this class
      qc.invalidateQueries({ queryKey: ["class", classId] });
    },
    onError: (error) => {
      console.error("Failed to delete class:", error);
      toast.error("Failed to delete class. Please try again.");
    },
  });
};

export const useAddClassMembers = () => {
  const qc = useQueryClient();

  return useMutation<BasicMessageResponse, Error, AddMembersRequest>({
    mutationFn: async ({ classId, userIds, addedBy }) => {
      if (!classId) throw new Error("Missing classId");
      const res = await axios.post<BasicMessageResponse>(
        `${CLASSES_BASE_URL}/${encodeURIComponent(classId)}/members`,
        { userIds, addedBy },
      );
      return res.data;
    },
    onSuccess: (_msg, { classId }) => {
      toast.success("Members added successfully.");
      // Just refetch the class to get full member objects
      qc.invalidateQueries({ queryKey: ["class", classId] });
      qc.invalidateQueries({ queryKey: ["classes"] });
    },
    onError: (error) => {
      console.error("Failed to add members:", error);
      toast.error("Failed to add members. Please try again.");
    },
  });
};

export const useRemoveClassMembers = () => {
  const qc = useQueryClient();

  return useMutation<BasicMessageResponse, Error, RemoveMembersRequest>({
    mutationFn: async ({ classId, userIds }) => {
      if (!classId) throw new Error("Missing classId");
      const res = await axios.delete<BasicMessageResponse>(
        `${CLASSES_BASE_URL}/${encodeURIComponent(classId)}/members`,
        { data: { userIds } },
      );
      return res.data;
    },

    onSuccess: (_msg, { classId, userIds }) => {
      toast.success("Members removed successfully.");

      // Optimistically update cache: remove by memberId
      qc.setQueryData<GetClassResponse | undefined>(
        ["class", classId],
        (prev) => {
          if (!prev) return prev;
          const toRemove = new Set(userIds);
          return {
            ...prev,
            members: prev.members.filter((m) => !toRemove.has(m.memberId)),
          };
        },
      );
    },

    onSettled: (_res, _err, vars) => {
      qc.invalidateQueries({ queryKey: ["class", vars.classId] });
      qc.invalidateQueries({ queryKey: ["classes"] });
    },

    onError: (error) => {
      console.error("Failed to remove members:", error);
      toast.error("Failed to remove members. Please try again.");
    },
  });
};
