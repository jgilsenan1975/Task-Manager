-- Task Manager reference schema (SQL Server / T-SQL)
-- This mirrors the EF Core model in backend/TaskManager.Api/Models.
-- In normal development you would NOT run this by hand — instead run:
--   dotnet ef migrations add InitialCreate
--   dotnet ef database update
-- from backend/TaskManager.Api, which generates and applies migrations
-- automatically. This file is kept as a readable reference to the shape
-- of the database and as a fallback if you want to create it manually.

IF DB_ID('TaskManagerDb') IS NULL
BEGIN
    CREATE DATABASE TaskManagerDb;
END
GO

USE TaskManagerDb;
GO

IF OBJECT_ID('dbo.TaskComments', 'U') IS NOT NULL DROP TABLE dbo.TaskComments;
IF OBJECT_ID('dbo.Tasks', 'U') IS NOT NULL DROP TABLE dbo.Tasks;
IF OBJECT_ID('dbo.Projects', 'U') IS NOT NULL DROP TABLE dbo.Projects;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    DisplayName   NVARCHAR(100)  NOT NULL,
    Email         NVARCHAR(256)  NOT NULL,
    PasswordHash  NVARCHAR(256)  NOT NULL,
    PasswordSalt  NVARCHAR(256)  NOT NULL,
    CreatedAt     DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.Projects (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(120)  NOT NULL,
    Description  NVARCHAR(1000) NULL,
    CreatedAt    DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    OwnerId      INT            NOT NULL,
    CONSTRAINT FK_Projects_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(Id)
);
GO

CREATE TABLE dbo.Tasks (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Title        NVARCHAR(200)  NOT NULL,
    Description  NVARCHAR(2000) NULL,
    Status       NVARCHAR(20)   NOT NULL DEFAULT 'ToDo',      -- ToDo | InProgress | Done
    Priority     NVARCHAR(20)   NOT NULL DEFAULT 'Medium',    -- Low | Medium | High
    DueDate      DATETIME2      NULL,
    CreatedAt    DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    ProjectId    INT            NOT NULL,
    AssigneeId   INT            NULL,
    CONSTRAINT FK_Tasks_Project  FOREIGN KEY (ProjectId)  REFERENCES dbo.Projects(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Tasks_Assignee FOREIGN KEY (AssigneeId) REFERENCES dbo.Users(Id)    ON DELETE SET NULL
);
GO

CREATE TABLE dbo.TaskComments (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    Body       NVARCHAR(2000) NOT NULL,
    CreatedAt  DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    TaskItemId INT            NOT NULL,
    AuthorId   INT            NOT NULL,
    CONSTRAINT FK_Comments_Task   FOREIGN KEY (TaskItemId) REFERENCES dbo.Tasks(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Author FOREIGN KEY (AuthorId)   REFERENCES dbo.Users(Id)
);
GO

CREATE INDEX IX_Tasks_ProjectId ON dbo.Tasks(ProjectId);
CREATE INDEX IX_Tasks_AssigneeId ON dbo.Tasks(AssigneeId);
CREATE INDEX IX_Projects_OwnerId ON dbo.Projects(OwnerId);
GO
