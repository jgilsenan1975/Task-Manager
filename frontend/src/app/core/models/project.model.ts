export interface Project {
  id: number;
  name: string;
  description?: string;
  createdAt: string;
  ownerId: number;
}

export interface CreateProjectRequest {
  name: string;
  description?: string;
}
