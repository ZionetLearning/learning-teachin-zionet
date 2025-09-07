import {
  useMutation,
  UseMutationResult,
  useQueryClient,
} from "@tanstack/react-query";
import { apiClient as axios, User, UserDto } from "@app-providers";

export const mapUser = (dto: UserDto): User => ({
  userId: dto.userId,
  email: dto.email,
  firstName: dto.firstName,
  lastName: dto.lastName,
});

// Safe resolution of USERS_URL for environments (like Cypress) where import.meta.env may be undefined.
const resolveUsersUrlBase = (): string => {
  try {
    const base = (
      import.meta as unknown as { env?: Record<string, string | undefined> }
    )?.env?.VITE_USERS_URL;
    if (base) return base;
  } catch (e) {
    console.error("Error resolving VITE_USERS_URL:", e);
  }
  const g = globalThis as unknown as {
    Cypress?: { env?: (k: string) => string | undefined };
    CYPRESS_VITE_USERS_URL?: string;
  };
  const cypressEnv = g?.Cypress?.env?.("VITE_USERS_URL");
  if (cypressEnv) return cypressEnv;
  if (g?.CYPRESS_VITE_USERS_URL) return g.CYPRESS_VITE_USERS_URL;
  return "https://localhost:5001/users-manager";
};
export const USERS_URL = `${resolveUsersUrlBase()}/user`;

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
