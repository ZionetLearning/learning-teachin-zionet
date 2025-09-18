import * as Yup from "yup";

export const validationSchema = Yup.object({
  name: Yup.string()
    .min(1, "Name must be at least 1 character")
    .max(200, "Name cannot exceed 200 characters")
    .required("Required"),
  payload: Yup.string()
    .min(1, "Payload must be at least 1 character")
    .required("Required"),
});

export interface CreateTaskFormValues {
  id?: string;
  name: string;
  payload: string;
}