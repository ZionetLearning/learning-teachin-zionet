import * as Yup from "yup";

export const validationSchema = Yup.object({
  email: Yup.string().email("Invalid email").required("Required"),
  password: Yup.string().min(6, "Min 6 chars").required("Required"),
});

export interface CreateUserFormValues {
  email: string;
  password: string;
}
