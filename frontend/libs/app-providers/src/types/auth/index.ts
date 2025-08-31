export type Role = "student" | "teacher";

export interface Credentials {
  email: string;
  password: string;
  sessionExpiry: number;
  role: Role;
}
