import {
  useMutation,
  useQuery,
  UseQueryResult,
  useQueryClient,
} from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";

export type ClassMemberId = string;

export type ClassItem = {
  classId: string;
  name: string;
  members: ClassMemberId[];
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
  userIds: ClassMemberId[];
  addedBy: string; // userId
};

export type RemoveMembersRequest = {
  classId: string;
  userIds: ClassMemberId[];
};

export type BasicMessageResponse = {
  message: string;
};

const CLASSES_BASE_URL = import.meta.env.VITE_CLASSES_MANAGER_URL;

export const useGetClass = (
  classId?: string,
  options?: { enabled?: boolean; staleTime?: number },
): UseQueryResult<GetClassResponse, Error> => {
  return useQuery<GetClassResponse, Error>({
    queryKey: ["class", classId] as const,
    enabled: Boolean(classId) && (options?.enabled ?? true),
    staleTime: options?.staleTime ?? 60_000,
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
    onSuccess: (_msg, { classId, userIds }) => {
      toast.success("Members added successfully.");
      // Update cached class members if present
      qc.setQueryData<GetClassResponse | undefined>(
        ["class", classId],
        (prev) => {
          if (!prev) return undefined;
          const existing = new Set(prev.members);
          userIds.forEach((id) => existing.add(id));
          return { ...prev, members: Array.from(existing) };
        },
      );
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
      // Update cached class members if present
      qc.setQueryData<GetClassResponse | undefined>(
        ["class", classId],
        (prev) => {
          if (!prev) return undefined;
          const toRemove = new Set(userIds);
          return {
            ...prev,
            members: prev.members.filter((id) => !toRemove.has(id)),
          };
        },
      );
    },
    onError: (error) => {
      console.error("Failed to remove members:", error);
      toast.error("Failed to remove members. Please try again.");
    },
  });
};
