import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";
import axiosBase from "axios";

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

export type UploadToBlobRequest = {
  uploadUrl: string;
  file: File;
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

export const useUploadToBlob = () => {
  return useMutation<void, Error, UploadToBlobRequest>({
    mutationFn: async ({ uploadUrl, file }) => {
      // use base axios instance (not apiClient) to avoid Authorization header
      // Azure Blob Storage authenticates via SAS token in the URL
      const response = await axiosBase.put(uploadUrl, file, {
        headers: {
          "Content-Type": file.type,
          "x-ms-blob-type": "BlockBlob",
        },
        timeout: 60000, // 60 second timeout
      });

      if (response.status !== 201) {
        throw new Error(
          `Upload failed with status ${response.status}: ${response.statusText}`,
        );
      }
    },
    onError: (error) => {
      console.error("Failed to upload to blob storage:", error);
    },
  });
};
