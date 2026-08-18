import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProjectService } from '../../../core/services/project.service';
import { Project } from '../../../core/models/project.model';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss'
})
export class ProjectList implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly projectService = inject(ProjectService);

  readonly projects = signal<Project[]>([]);
  readonly isLoading = signal(true);

  readonly form = this.fb.group({
    name: ['', Validators.required],
    description: ['']
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.projectService.getAll().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  createProject(): void {
    if (this.form.invalid) {
      return;
    }

    this.projectService
      .create(this.form.getRawValue() as { name: string; description: string })
      .subscribe(() => {
        this.form.reset();
        this.load();
      });
  }
}
