import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";

const USERS_MANAGER_URL = import.meta.env.VITE_USERS_URL!;

export type UploadUrlRequest = {
  contentType: string;
  sizeBytes: number;
};

export type UploadUrlResponse = {
  uploadUrl: string;
  blobPath: string;
  expiresAtUtc: string;
  maxBytes: number;
  acceptedContentTypes: string[];
};

export type ConfirmAvatarRequest = {
  blobPath: string;
  contentType: string;
};

export const useGetAvatarUploadUrl = (userId: string) => {
  return useMutation<UploadUrlResponse, Error, UploadUrlRequest>({
    mutationFn: async (body) => {
      const res = await axios.post<UploadUrlResponse>(
        `${USERS_MANAGER_URL}/user/${userId}/avatar/upload-url`,
        body,
      );
      return res.data;
    },
    onError: (error) => {
      console.error("Failed to get upload URL:", error);
      toast.error("Failed to prepare avatar upload. Please try again.");
    },
  });
};

export const useConfirmAvatar = (userId: string) => {
  const qc = useQueryClient();

  return useMutation<void, Error, ConfirmAvatarRequest>({
    mutationFn: async (body) => {
      await axios.post(
        `${USERS_MANAGER_URL}/user/${userId}/avatar/confirm`,
        body,
      );
    },
    onSuccess: () => {
      toast.success("Avatar uploaded successfully!");
      qc.invalidateQueries({ queryKey: ["avatar", userId] });
    },
    onError: (error) => {
      console.error("Failed to confirm avatar:", error);
      toast.error("Failed to save avatar. Please try again.");
    },
  });
};

export const useGetAvatarUrl = (userId: string) => {
  return useQuery<string | null, Error>({
    queryKey: ["avatar", userId],
    queryFn: async () => {
      try {
        const res = await axios.get<string>(
          `${USERS_MANAGER_URL}/user/${userId}/avatar/url`,
        );
        return res.data || null;
      } catch {
        return null;
      }
    },
    staleTime: 5 * 60 * 1000,
    retry: false,
  });
};

export const useDeleteAvatar = (userId: string) => {
  const qc = useQueryClient();

  return useMutation<void, Error, void>({
    mutationFn: async () => {
      await axios.delete(`${USERS_MANAGER_URL}/user/${userId}/avatar`);
    },
    onSuccess: () => {
      toast.success("Avatar deleted successfully!");
      qc.invalidateQueries({ queryKey: ["avatar", userId] });
    },
    onError: (error) => {
      console.error("Failed to delete avatar:", error);
      toast.error("Failed to delete avatar. Please try again.");
    },
  });
};
