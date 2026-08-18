import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'projects' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register)
  },
  {
    path: 'projects',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/projects/project-list/project-list').then((m) => m.ProjectList)
  },
  {
    path: 'projects/:projectId/board',
    canActivate: [authGuard],
    loadComponent: () => import('./features/board/board').then((m) => m.Board)
  },
  { path: '**', redirectTo: 'projects' }
];
