import {
  useMutation,
  UseMutationResult,
  useQuery,
  useQueryClient,
  UseQueryResult,
} from "@tanstack/react-query";

interface UserDto {
  userId: string;
  email: string;
  passwordHash?: string;
}

interface UpdateUserInput {
  email: string;
  passwordHash: string;
}

export interface User {
  userId: string;
  email: string;
}

const USERS_URL = `${import.meta.env.VITE_BASE_URL}/user`;

const mapUser = (dto: UserDto): User => ({
  userId: dto.userId,
  email: dto.email,
});

const getAllUsers = async (): Promise<User[]> => {
  const response = await fetch(`${USERS_URL}-list`);
  const payload = await response.json();
  if (!response.ok)
    throw new Error(payload?.message || "Failed to fetch users");
  return (payload as UserDto[]).map(mapUser);
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
  const response = await fetch(`${USERS_URL}/${userId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  const payload = await response.json();
  if (!response.ok)
    throw new Error(payload?.message || "Failed to update user");
  return mapUser(payload as UserDto);
};

const deleteUserByUserId = async (userId: string): Promise<void> => {
  const response = await fetch(`${USERS_URL}/${userId}`, { method: "DELETE" });
  if (!response.ok) {
    const payload: unknown = await response.json().catch(() => null);
    const message =
      payload && typeof payload === "object" && "message" in payload
        ? (payload as { message?: string }).message
        : undefined;
    throw new Error(message || "Failed to delete user");
  }
};

const createUser = async (userData: Partial<UserDto>): Promise<User> => {
  const response = await fetch(USERS_URL, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(userData),
  });
  const payload = await response.json();
  if (!response.ok)
    throw new Error(payload?.message || "Failed to create user");
  return mapUser(payload as UserDto);
};

export const useGetAllUsers = (): UseQueryResult<User[], Error> => {
  return useQuery<User[], Error>({
    queryKey: ["users"],
    queryFn: getAllUsers,
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
