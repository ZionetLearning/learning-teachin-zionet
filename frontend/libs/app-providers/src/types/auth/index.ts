export const AppRole = {
  student: "student",
  teacher: "teacher",
  admin: "admin"
} as const;

export type AppRoleType = (typeof AppRole)[keyof typeof AppRole];

export interface Credentials {
  email: string;
  password: string;
  sessionExpiry: number;
  role: AppRoleType;
}

export interface SignupData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: AppRoleType;
}
