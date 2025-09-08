import {
  useMutation,
  UseMutationResult,
  useQueryClient,
} from "@tanstack/react-query";
import { apiClient as axios, User, UserDto } from "@app-providers";

export interface UserData {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface UpdateUserInput {
  userId: string;
  firstName: string;
  lastName: string;
}

export const mapUser = (dto: UserDto): User => ({
  userId: dto.userId,
  email: dto.email,
  firstName: dto.firstName,
  lastName: dto.lastName,
});

export const USERS_URL = `${import.meta.env.VITE_USERS_URL}/user`;

const createUser = async (userData: Partial<UserDto>): Promise<User> => {
  const response = await axios.post(USERS_URL, userData);
  if (response.status !== 201) {
    throw new Error(response.data?.message || "Failed to create user");
  }
  return mapUser(response.data as UserDto);
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

export const useUpdateUser = (): UseMutationResult<
  void,
  Error,
  UpdateUserInput
> => {
  const qc = useQueryClient();
  return useMutation<void, Error, UpdateUserInput>({
    mutationFn: updateUser,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["users"] });
    },
  });
};

export const getUserById = async (userId: string): Promise<UserData> => {
  const raw = localStorage.getItem("credentials");
  const token = raw ? JSON.parse(raw).accessToken : null;

  if (!token) throw new Error("Missing access token in localStorage");

  const { data } = await axios.get<UserData>(
    `${import.meta.env.VITE_USERS_URL}/user/${userId}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  return data;
};

export const updateUser = async ({
  userId,
  firstName,
  lastName,
}: UpdateUserInput): Promise<void> => {
  const raw = localStorage.getItem("credentials");
  const token = raw ? JSON.parse(raw).accessToken : null;

  if (!token) throw new Error("Missing access token in localStorage");

  await axios.put(
    `${import.meta.env.VITE_USERS_URL}/user/${userId}`,
    { firstName, lastName },
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
};

