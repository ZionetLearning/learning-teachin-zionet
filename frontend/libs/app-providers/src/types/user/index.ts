import { AppRoleType } from "../auth";

export interface CreateUserRequest {
  userId: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role?: string;
}

export interface CreateUserResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
}

export type UserDto = User & {
  password?: string;
  role?: AppRoleType;
};

export interface User {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
}
