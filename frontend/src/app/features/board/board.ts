import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  CdkDragDrop,
  DragDropModule,
  moveItemInArray,
  transferArrayItem
} from '@angular/cdk/drag-drop';
import { TaskService } from '../../core/services/task.service';
import { TaskItem, TaskPriority, TaskStatus } from '../../core/models/task.model';

interface Column {
  status: TaskStatus;
  title: string;
  tasks: TaskItem[];
}

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule, DragDropModule, ReactiveFormsModule],
  templateUrl: './board.html',
  styleUrl: './board.scss'
})
export class Board implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly taskService = inject(TaskService);
  private readonly fb = inject(FormBuilder);

  readonly projectId = signal<number>(0);
  readonly columns = signal<Column[]>([
    { status: TaskStatus.ToDo, title: 'To Do', tasks: [] },
    { status: TaskStatus.InProgress, title: 'In Progress', tasks: [] },
    { status: TaskStatus.Done, title: 'Done', tasks: [] }
  ]);

  readonly connectedLists = computed(() => this.columns().map((c) => c.status));
  readonly priorities = Object.values(TaskPriority);

  readonly form = this.fb.group({
    title: ['', Validators.required],
    priority: [TaskPriority.Medium, Validators.required]
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('projectId'));
    this.projectId.set(id);
    this.load();
  }

  load(): void {
    this.taskService.getByProject(this.projectId()).subscribe((tasks) => {
      this.columns.update((cols) =>
        cols.map((col) => ({
          ...col,
          tasks: tasks.filter((t) => t.status === col.status)
        }))
      );
    });
  }

  addTask(): void {
    if (this.form.invalid) {
      return;
    }

    const { title, priority } = this.form.getRawValue();

    this.taskService
      .create({
        projectId: this.projectId(),
        title: title!,
        priority: priority!
      })
      .subscribe(() => {
        this.form.reset({ priority: TaskPriority.Medium });
        this.load();
      });
  }

  drop(event: CdkDragDrop<TaskItem[]>, targetStatus: TaskStatus): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      return;
    }

    const task = event.previousContainer.data[event.previousIndex];
    transferArrayItem(
      event.previousContainer.data,
      event.container.data,
      event.previousIndex,
      event.currentIndex
    );

    this.taskService.updateStatus(task.id, { status: targetStatus }).subscribe();
  }
}
