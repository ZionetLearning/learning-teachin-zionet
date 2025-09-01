export const AppRole = {
  student: "student",
  teacher: "teacher",
} as const;

export type AppRoleType = (typeof AppRole)[keyof typeof AppRole];

export interface Credentials {
  email: string;
  accessToken: string;
  accessTokenExpiry: number;
  role?: AppRoleType;
}

export interface SignupData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: AppRoleType;
}
