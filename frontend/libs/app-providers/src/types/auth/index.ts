export const AppRole = {
  student: "student",
  teacher: "teacher",
} as const;

export type AppRoleType = (typeof AppRole)[keyof typeof AppRole];

export interface Credentials {
  email: string;
  password: string;
  sessionExpiry: number;
  role: AppRoleType;
}
