-- =============================================
-- Скрипт создания базы данных OnlineTutor3
-- =============================================

-- Создание базы данных
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'OnlineTutor3')
BEGIN
    CREATE DATABASE [OnlineTutor3]
    PRINT 'База данных OnlineTutor3 создана'
END
ELSE
BEGIN
    PRINT 'База данных OnlineTutor3 уже существует'
END
GO

USE [OnlineTutor3]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- ТАБЛИЦА SUBJECTS (предметы)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Subjects]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Subjects] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [OrderIndex] INT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Subjects] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE UNIQUE INDEX [IX_Subjects_Name] ON [dbo].[Subjects]([Name])
    CREATE INDEX [IX_Subjects_IsActive] ON [dbo].[Subjects]([IsActive])
    CREATE INDEX [IX_Subjects_OrderIndex] ON [dbo].[Subjects]([OrderIndex])
    
    PRINT 'Таблица Subjects создана'
END
GO

-- Заполнение предметов
IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'Русский язык')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'Русский язык', N'Русский язык и литература', 1, 1)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'Литература')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'Литература', N'Литература', 1, 2)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'Математика')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'Математика', N'Математика', 1, 3)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'Информатика')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'Информатика', N'Информатика', 1, 4)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'Английский язык')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'Английский язык', N'Английский язык', 1, 5)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'Физика')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'Физика', N'Физика', 1, 6)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'Химия')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'Химия', N'Химия', 1, 7)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'Биология')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'Биология', N'Биология', 1, 8)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'География')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'География', N'География', 1, 9)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'История')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'История', N'История', 1, 10)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Subjects] WHERE Name = N'Обществознание')
BEGIN
    INSERT INTO [dbo].[Subjects] (Name, Description, IsActive, OrderIndex)
    VALUES (N'Обществознание', N'Обществознание', 1, 11)
END
GO

-- =============================================
-- ТАБЛИЦА CLASSES (классы)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Classes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Classes] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [TeacherId] NVARCHAR(450) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [IsActive] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Classes] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Classes_AspNetUsers] FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_Classes_TeacherId] ON [dbo].[Classes]([TeacherId])
    CREATE INDEX [IX_Classes_IsActive] ON [dbo].[Classes]([IsActive])
    
    PRINT 'Таблица Classes создана'
END
GO

-- =============================================
-- ТАБЛИЦА TEACHERS (учителя)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Teachers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Teachers] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [Education] NVARCHAR(500) NULL,
        [Experience] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [IsApproved] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Teachers] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Teachers_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    )
    
    CREATE UNIQUE INDEX [IX_Teachers_UserId] ON [dbo].[Teachers]([UserId])
    CREATE INDEX [IX_Teachers_IsApproved] ON [dbo].[Teachers]([IsApproved])
    
    PRINT 'Таблица Teachers создана'
END
GO

-- =============================================
-- ТАБЛИЦА STUDENTS (ученики)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Students] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [ClassId] INT NULL,
        [StudentNumber] NVARCHAR(50) NULL,
        [School] NVARCHAR(200) NULL,
        [Grade] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Students] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Students_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Students_Classes] FOREIGN KEY ([ClassId]) REFERENCES [dbo].[Classes]([Id]) ON DELETE SET NULL
    )
    
    CREATE UNIQUE INDEX [IX_Students_UserId] ON [dbo].[Students]([UserId])
    CREATE INDEX [IX_Students_ClassId] ON [dbo].[Students]([ClassId])
    CREATE INDEX [IX_Students_StudentNumber] ON [dbo].[Students]([StudentNumber]) WHERE [StudentNumber] IS NOT NULL
    
    PRINT 'Таблица Students создана'
END
GO

-- =============================================
-- ТАБЛИЦА TEACHERSUBJECTS (связь учителей и предметов)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TeacherSubjects]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TeacherSubjects] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [TeacherId] INT NOT NULL,
        [SubjectId] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_TeacherSubjects] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TeacherSubjects_Teachers] FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[Teachers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TeacherSubjects_Subjects] FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([Id]) ON DELETE CASCADE
    )
    
    CREATE UNIQUE INDEX [IX_TeacherSubjects_TeacherId_SubjectId] ON [dbo].[TeacherSubjects]([TeacherId], [SubjectId])
    CREATE INDEX [IX_TeacherSubjects_TeacherId] ON [dbo].[TeacherSubjects]([TeacherId])
    CREATE INDEX [IX_TeacherSubjects_SubjectId] ON [dbo].[TeacherSubjects]([SubjectId])
    
    PRINT 'Таблица TeacherSubjects создана'
END
GO

PRINT 'Скрипт создания базы данных выполнен успешно!'
GO

