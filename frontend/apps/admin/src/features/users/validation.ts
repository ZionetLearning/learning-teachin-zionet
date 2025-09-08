import * as Yup from "yup";
import { AppRoleType } from "@app-providers/types";

export const validationSchema = Yup.object({
  email: Yup.string().email("Invalid email").required("Required"),
  password: Yup.string().min(6, "Min 6 chars").required("Required"),
  firstName: Yup.string().required("Required"),
  lastName: Yup.string().required("Required"),
  role: Yup.mixed<AppRoleType>()
    .oneOf(["student", "teacher", "admin"])
    .required("Required"),
});

export interface CreateUserFormValues {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: AppRoleType;
}
