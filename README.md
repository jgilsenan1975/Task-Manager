# Task Manager

A full-stack kanban-style task and project manager, built as a portfolio project demonstrating **Angular**, **C# / ASP.NET Core**, and **SQL Server** working together end to end.

Users can register and sign in, create projects, and manage tasks on a drag-and-drop board with three columns: To Do, In Progress, and Done.

## Tech stack

- **Frontend:** Angular 21 (standalone components, signals, Angular CDK drag-and-drop), TypeScript, SCSS
- **Backend:** ASP.NET Core 8 Web API (C#), JWT authentication, Swagger/OpenAPI
- **Database:** SQL Server, accessed via Entity Framework Core (code-first migrations)

## Architecture

```
task-manager-portfolio/
├── backend/                  ASP.NET Core Web API
│   └── TaskManager.Api/
│       ├── Controllers/      Auth, Projects, Tasks endpoints
│       ├── Data/             EF Core DbContext
│       ├── Dtos/             Request/response contracts
│       ├── Models/           Entity classes (User, Project, TaskItem, TaskComment)
│       └── Services/         Password hashing, JWT token issuing
├── database/                 Reference SQL scripts (schema mirrors the EF model)
└── frontend/                 Angular application
    └── src/app/
        ├── core/             Services, models, route guard, HTTP interceptor
        └── features/         Login, register, project list, kanban board
```

The frontend talks to the API over HTTP with a bearer token attached by an interceptor. The API is stateless (no server-side sessions) — auth state lives in the JWT, and authorization is enforced per-request by checking resource ownership (e.g. a project's `OwnerId`) against the token's user id.

## Features

- Email/password registration and login, with passwords hashed using PBKDF2 (per-user salt, 100,000 iterations) — no plaintext or reversible storage
- JWT-based authentication, validated on every API request
- Create projects and see them as a card grid
- Kanban board per project with drag-and-drop between To Do / In Progress / Done (Angular CDK), backed by an API call that persists the new status
- Task priority (Low / Medium / High) shown as a colored badge
- Ownership-scoped data access — a user can only see and modify their own projects and tasks

## Getting started

### Prerequisites

- [.NET SDK 8](https://dotnet.microsoft.com/download) or later
- [Node.js 20+](https://nodejs.org/) and npm
- SQL Server (LocalDB, a full instance, or SQL Server in Docker) — [Angular CLI](https://angular.dev/tools/cli) is installed automatically via `npm install`

### 1. Database

The EF Core model is the source of truth for the schema. From `backend/TaskManager.Api`:

```bash
dotnet tool install --global dotnet-ef   # first time only
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This creates the `TaskManagerDb` database and all tables. `database/schema.sql` is kept as a human-readable reference to the same schema (useful for reviewing the design without running the app), but migrations are the actual source of truth — don't run both against the same database.

By default the connection string in `appsettings.json` points at SQL Server LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=TaskManagerDb;Trusted_Connection=True;...
```

Update `ConnectionStrings:DefaultConnection` if you're pointing at a different SQL Server instance.

### 2. Backend API

```bash
cd backend/TaskManager.Api
dotnet restore
dotnet run
```

The API starts on `https://localhost:5001` (and `http://localhost:5000`) with Swagger UI at `/swagger` in development.

**Before running anywhere beyond your own machine**, replace the placeholder `Jwt:Key` in `appsettings.json` with a real random secret (at least 32 characters), and prefer environment variables or `dotnet user-secrets` over committing real secrets to source control.

### 3. Frontend

```bash
cd frontend
npm install
npm start
```

The app runs at `http://localhost:4200` and expects the API at `https://localhost:5001/api` (see `src/environments/environment.ts`).

## Possible next steps

A few natural extensions if you want to keep building this out for your portfolio:

- Real-time board updates across browser tabs/users with SignalR
- Task comments and file attachments (the `TaskComment` entity already exists in the model)
- Role-based project membership instead of single-owner projects
- A dashboard view with charts (e.g. tasks completed per week)
- CI pipeline (GitHub Actions) running `dotnet test` and `ng test` on push
- Deploying the API to Azure App Service and the frontend to a static host, with the SQL Server database in Azure SQL

## License

MIT — see [LICENSE](LICENSE).
