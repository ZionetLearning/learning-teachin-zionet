import {
  useMutation,
  useQuery,
  useQueryClient,
  UseMutationResult,
  UseQueryResult,
} from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import {
  TaskModel,
  TaskSummaryDto,
  CreateTaskInput,
  TaskWithETag,
} from "../types";

const TASKS_BASE_URL = `${import.meta.env.VITE_TASKS_URL}/task`;

export const taskKeys = {
  all: ['tasks'] as const,
  lists: () => [...taskKeys.all, 'list'] as const,
  list: (filters: string) => [...taskKeys.lists(), { filters }] as const,
  details: () => [...taskKeys.all, 'detail'] as const,
  detail: (id: number) => [...taskKeys.details(), id] as const,
};

// API Functions
const getAllTasks = async (): Promise<TaskSummaryDto[]> => {
  const response = await axios.get(`${TASKS_BASE_URL}s`);
  if (response.status !== 200) {
    throw new Error(response.data?.message || "Failed to fetch tasks");
  }
  return response.data as TaskSummaryDto[];
};

const getTaskById = async (id: number): Promise<TaskWithETag> => {
  const response = await axios.get(`${TASKS_BASE_URL}/${id}`);
  if (response.status !== 200) {
    throw new Error(response.data?.message || "Failed to fetch task");
  }
  
  const etag = response.headers.etag || response.headers.ETag;
  return {
    task: response.data as TaskModel,
    etag: etag?.replace(/"/g, '')
  };
};

const createTask = async (task: CreateTaskInput): Promise<void> => {
  const response = await axios.post(TASKS_BASE_URL, task, {
    headers: {
      "Content-Type": "application/json",
    },
  });
  
  if (response.status !== 202) {
    throw new Error(response.data?.message || "Failed to create task");
  }
};

const updateTaskName = async (
  id: number,
  name: string,
  etag?: string
): Promise<void> => {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };
  
  if (etag) {
    headers["If-Match"] = `"${etag}"`;
  }

  const response = await axios.put(`${TASKS_BASE_URL}/${id}/${encodeURIComponent(name)}`, {}, {
    headers,
  });
  
  if (response.status !== 200) {
    throw new Error(response.data?.message || "Failed to update task");
  }
};

const deleteTask = async (id: number): Promise<void> => {
  const response = await axios.delete(`${TASKS_BASE_URL}/${id}`);
  
  if (response.status !== 200) {
    throw new Error(response.data?.message || "Failed to delete task");
  }
};

export const useGetAllTasks = (): UseQueryResult<TaskSummaryDto[], Error> => {
  return useQuery<TaskSummaryDto[], Error>({
    queryKey: taskKeys.lists(),
    queryFn: getAllTasks,
    staleTime: 30_000, // 30 seconds
    refetchOnWindowFocus: false,
  });
};

export const useGetTaskById = (id: number): UseQueryResult<TaskWithETag, Error> => {
  return useQuery<TaskWithETag, Error>({
    queryKey: taskKeys.detail(id),
    queryFn: () => getTaskById(id),
    enabled: id > 0,
    staleTime: 60_000,
  });
};

export const useCreateTask = (): UseMutationResult<void, Error, CreateTaskInput> => {
  const queryClient = useQueryClient();
  
  return useMutation<void, Error, CreateTaskInput>({
    mutationFn: createTask,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
    },
  });
};

export const useUpdateTaskName = (): UseMutationResult<
  void,
  Error,
  { id: number; name: string; etag?: string }
> => {
  const queryClient = useQueryClient();
  
  return useMutation<void, Error, { id: number; name: string; etag?: string }>({
    mutationFn: ({ id, name, etag }) => updateTaskName(id, name, etag),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      queryClient.invalidateQueries({ queryKey: taskKeys.detail(variables.id) });
    },
  });
};

export const useDeleteTask = (): UseMutationResult<void, Error, number> => {
  const queryClient = useQueryClient();
  
  return useMutation<void, Error, number>({
    mutationFn: deleteTask,
    onSuccess: (_, taskId) => {
      queryClient.removeQueries({ queryKey: taskKeys.detail(taskId) });
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
    },
  });
};