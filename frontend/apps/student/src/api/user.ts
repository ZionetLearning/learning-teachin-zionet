import {
  useMutation,
  UseMutationResult,
  useQuery,
  useQueryClient,
  UseQueryResult,
} from "@tanstack/react-query";
import axios from "axios";

type UserDto = User & {
  passwordHash?: string;
};

interface UpdateUserInput {
  email: string;
  passwordHash?: string;
}

export interface User {
  userId: string;
  email: string;
}

const USERS_URL = `${import.meta.env.VITE_USERS_URL}/user`;

const mapUser = (dto: UserDto): User => ({
  userId: dto.userId,
  email: dto.email,
});

const getAllUsers = async (): Promise<User[]> => {
  const response = await axios.get(`${USERS_URL}-list`);
  if (response.status !== 200) {
    throw new Error(response.data?.message || "Failed to fetch users");
  }
  return (response.data as UserDto[]).map(mapUser);
};

export const getUserByUserId = async (userId: string): Promise<UserDto> => {
  const response = await axios.get(`${USERS_URL}/${userId}`);
  if (response.status !== 200) {
    throw new Error(response.data?.message || "Failed to fetch user");
  }
  return response.data;
};

const updateUserByUserId = async (
  userId: string,
  userData: UpdateUserInput,
): Promise<User> => {
  const body: UserDto = {
    userId,
    email: userData.email,
    passwordHash: userData.passwordHash,
  };
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

const createUser = async (userData: Partial<UserDto>): Promise<User> => {
  const response = await axios.post(USERS_URL, userData);
  if (response.status !== 201) {
    throw new Error(response.data?.message || "Failed to create user");
  }
  return mapUser(response.data as UserDto);
};

export const useGetAllUsers = (): UseQueryResult<User[], Error> => {
  return useQuery<User[], Error>({
    queryKey: ["users"],
    queryFn: getAllUsers,
    staleTime: 60_000,
  });
};

export const useGetUserByUserId = (
  userId: string,
): UseQueryResult<User, Error> => {
  return useQuery<User, Error>({
    queryKey: ["users", userId],
    queryFn: () => getUserByUserId(userId),
    staleTime: 60_000,
  });
};

export const useCreateUser = (): UseMutationResult<
  User,
  Error,
  Partial<UserDto>
> => {
  const qc = useQueryClient();
  return useMutation<User, Error, Partial<UserDto>>({
    mutationFn: createUser,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["users"] });
    },
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
