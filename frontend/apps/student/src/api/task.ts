import { useMutation } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";

export type TaskInput = {
  id: number;
  name: string;
  payload: string;
};

export const usePostTask = () => {
  const TASKS_BASE_URL = import.meta.env.VITE_TASKS_URL!;

  return useMutation<void, Error, TaskInput>({
    mutationFn: async ({ id, name, payload }) => {
      await axios.post(
        //local server endpoint URL:
        // "http://localhost:5280/tasks-manager/task",
        //cloud server endpoint URL:
        `${TASKS_BASE_URL}/task`,
        { id, name, payload },
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );
    },

    onSuccess: () => {
      toast.success("Task posted");
    },

    onError: (err) => {
      console.error("POST /task failed:", err);
      toast.error("Failed to post task");
    },
  });
};
