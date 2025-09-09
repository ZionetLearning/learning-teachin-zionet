import {
  useMutation,
  UseMutationResult,
  useQuery,
  useQueryClient,
  UseQueryResult,
} from "@tanstack/react-query";

import { apiClient as axios, User, UserDto } from "@app-providers";


interface UpdateUserInput {
  email?: string;
  firstName?: string;
  lastName?: string;
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


export const getUserById = async (userId: string): Promise<UserDto> => {
  const { data } = await axios.get<UserDto>(
    `${import.meta.env.VITE_USERS_URL}/user/${userId}`
  );

  return data;
};


export const updateUserByUserId = async (
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

export const useGetUserById = (
  userId: string | undefined
): UseQueryResult<UserDto, Error> => {
  return useQuery<UserDto, Error>({
    queryKey: ["user", userId],
    queryFn: () => {
      if (!userId) throw new Error("Missing userId");
      return getUserById(userId);
    },
    enabled: !!userId, // only runs if userId is truthy
    staleTime: 5_000,
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
