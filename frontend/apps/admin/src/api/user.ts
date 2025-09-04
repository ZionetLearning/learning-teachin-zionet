import {
  useMutation,
  UseMutationResult,
  useQuery,
  useQueryClient,
  UseQueryResult,
} from "@tanstack/react-query";
import axios from "axios";
import { mapUser, User, UserDto } from "@app-providers";

interface UpdateUserInput {
  email?: string;
  firstName?: string;
  lastName?: string;
}

const USERS_URL = `${import.meta.env.VITE_USERS_URL}/user`;

const getAllUsers = async (): Promise<User[]> => {
  const response = await axios.get(`${USERS_URL}-list`);
  if (response.status !== 200) {
    throw new Error(response.data?.message || "Failed to fetch users");
  }
  return (response.data as UserDto[]).map(mapUser);
};

const updateUserByUserId = async (
  userId: string,
  userData: UpdateUserInput,
): Promise<User> => {
  const body: Record<string, unknown> = { userId };
  if (typeof userData.email === "string") body.email = userData.email;
  if (typeof userData.firstName === "string")
    body.firstName = userData.firstName;
  if (typeof userData.lastName === "string") body.lastName = userData.lastName;

  const response = await axios.put(`${USERS_URL}/${userId}`, body);
  if (response.status !== 200) {
    throw new Error(response.data?.message || "Failed to update user");
  }
  return mapUser(response.data as UserDto);
};

const deleteUserByUserId = async (userId: string): Promise<void> => {
  const response = await axios.delete(`${USERS_URL}/${userId}`);
  if (response.status !== 200) {
    const payload: unknown = await response.data;
    const message =
      payload && typeof payload === "object" && "message" in payload
        ? (payload as { message?: string }).message
        : undefined;
    throw new Error(message || "Failed to delete user");
  }
};

export const useGetAllUsers = (): UseQueryResult<User[], Error> => {
  return useQuery<User[], Error>({
    queryKey: ["users"],
    queryFn: getAllUsers,
    staleTime: 60_000,
  });
};

export const useUpdateUserByUserId = (
  userId: string,
): UseMutationResult<User, Error, UpdateUserInput> => {
  const qc = useQueryClient();
  return useMutation<User, Error, UpdateUserInput>({
    mutationFn: (data) => updateUserByUserId(userId, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["users"] });
    },
  });
};

export const useDeleteUserByUserId = (
  userId: string,
): UseMutationResult<void, Error, void> => {
  const qc = useQueryClient();
  return useMutation<void, Error, void>({
    mutationFn: () => deleteUserByUserId(userId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["users"] });
    },
  });
};
