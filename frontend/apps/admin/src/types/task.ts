export interface TaskModel {
  id: number;
  name: string;
  payload: string;
}

export interface CreateTaskInput {
  id?: number;
  name: string;
  payload: string;
}

export interface UpdateTaskInput {
  name: string;
  payload: string;
}

export interface TaskWithETag {
  task: TaskModel;
  etag?: string;
}

export interface TaskFormData {
  id?: string;
  name: string;
  payload: string;
}

export interface TaskValidationErrors {
  id?: string;
  name?: string;
  payload?: string;
}

export type TaskActionMode = 'create' | 'edit' | 'view';

export interface TasksListItem extends TaskModel {
  isLoading?: boolean;
}