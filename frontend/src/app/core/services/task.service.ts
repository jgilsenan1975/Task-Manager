import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateTaskRequest, TaskItem, UpdateTaskStatusRequest } from '../models/task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly baseUrl = `${environment.apiUrl}/tasks`;

  constructor(private readonly http: HttpClient) {}

  getByProject(projectId: number): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(`${this.baseUrl}?projectId=${projectId}`);
  }

  create(request: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(this.baseUrl, request);
  }

  updateStatus(id: number, request: UpdateTaskStatusRequest): Observable<TaskItem> {
    return this.http.patch<TaskItem>(`${this.baseUrl}/${id}/status`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
