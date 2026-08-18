export enum TaskStatus {
  ToDo = 'ToDo',
  InProgress = 'InProgress',
  Done = 'Done'
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High'
}

export interface TaskItem {
  id: number;
  projectId: number;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string;
  assigneeId?: number;
  createdAt: string;
}

export interface CreateTaskRequest {
  projectId: number;
  title: string;
  description?: string;
  priority: TaskPriority;
  dueDate?: string;
  assigneeId?: number;
}

export interface UpdateTaskStatusRequest {
  status: TaskStatus;
}
