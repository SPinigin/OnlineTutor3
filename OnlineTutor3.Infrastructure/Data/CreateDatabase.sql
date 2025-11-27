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

-- =============================================
-- ТАБЛИЦА ASSIGNMENTS (задания)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Assignments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Assignments] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [SubjectId] INT NOT NULL,
        [TeacherId] NVARCHAR(450) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [DueDate] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Assignments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Assignments_Subjects] FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Assignments_AspNetUsers] FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_Assignments_SubjectId] ON [dbo].[Assignments]([SubjectId])
    CREATE INDEX [IX_Assignments_TeacherId] ON [dbo].[Assignments]([TeacherId])
    CREATE INDEX [IX_Assignments_IsActive] ON [dbo].[Assignments]([IsActive])
    
    PRINT 'Таблица Assignments создана'
END
GO

-- =============================================
-- ТАБЛИЦА ASSIGNMENTCLASSES (связь заданий и классов)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssignmentClasses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AssignmentClasses] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [AssignmentId] INT NOT NULL,
        [ClassId] INT NOT NULL,
        [AssignedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_AssignmentClasses] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AssignmentClasses_Assignments] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignments]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AssignmentClasses_Classes] FOREIGN KEY ([ClassId]) REFERENCES [dbo].[Classes]([Id]) ON DELETE CASCADE
    )
    
    CREATE UNIQUE INDEX [IX_AssignmentClasses_AssignmentId_ClassId] ON [dbo].[AssignmentClasses]([AssignmentId], [ClassId])
    CREATE INDEX [IX_AssignmentClasses_AssignmentId] ON [dbo].[AssignmentClasses]([AssignmentId])
    CREATE INDEX [IX_AssignmentClasses_ClassId] ON [dbo].[AssignmentClasses]([ClassId])
    
    PRINT 'Таблица AssignmentClasses создана'
END
GO

-- =============================================
-- ТАБЛИЦА SPELLINGTESTS (тесты по орфографии)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpellingTests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SpellingTests] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [AssignmentId] INT NOT NULL,
        [TeacherId] NVARCHAR(450) NOT NULL,
        [TimeLimit] INT NOT NULL DEFAULT 30,
        [MaxAttempts] INT NOT NULL DEFAULT 1,
        [StartDate] DATETIME2 NULL,
        [EndDate] DATETIME2 NULL,
        [ShowHints] BIT NOT NULL DEFAULT 1,
        [ShowCorrectAnswers] BIT NOT NULL DEFAULT 1,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_SpellingTests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_SpellingTests_Assignments] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignments]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SpellingTests_AspNetUsers] FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_SpellingTests_AssignmentId] ON [dbo].[SpellingTests]([AssignmentId])
    CREATE INDEX [IX_SpellingTests_TeacherId] ON [dbo].[SpellingTests]([TeacherId])
    CREATE INDEX [IX_SpellingTests_IsActive] ON [dbo].[SpellingTests]([IsActive])
    
    PRINT 'Таблица SpellingTests создана'
END
GO

-- =============================================
-- ТАБЛИЦА PUNCTUATIONTESTS (тесты по пунктуации)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PunctuationTests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PunctuationTests] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [AssignmentId] INT NOT NULL,
        [TeacherId] NVARCHAR(450) NOT NULL,
        [TimeLimit] INT NOT NULL DEFAULT 30,
        [MaxAttempts] INT NOT NULL DEFAULT 1,
        [StartDate] DATETIME2 NULL,
        [EndDate] DATETIME2 NULL,
        [ShowHints] BIT NOT NULL DEFAULT 1,
        [ShowCorrectAnswers] BIT NOT NULL DEFAULT 1,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_PunctuationTests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_PunctuationTests_Assignments] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignments]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PunctuationTests_AspNetUsers] FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_PunctuationTests_AssignmentId] ON [dbo].[PunctuationTests]([AssignmentId])
    CREATE INDEX [IX_PunctuationTests_TeacherId] ON [dbo].[PunctuationTests]([TeacherId])
    CREATE INDEX [IX_PunctuationTests_IsActive] ON [dbo].[PunctuationTests]([IsActive])
    
    PRINT 'Таблица PunctuationTests создана'
END
GO

-- =============================================
-- ТАБЛИЦА ORTHOEOBYTESTS (тесты по орфоэпии)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrthoeopyTests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrthoeopyTests] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [AssignmentId] INT NOT NULL,
        [TeacherId] NVARCHAR(450) NOT NULL,
        [TimeLimit] INT NOT NULL DEFAULT 30,
        [MaxAttempts] INT NOT NULL DEFAULT 1,
        [StartDate] DATETIME2 NULL,
        [EndDate] DATETIME2 NULL,
        [ShowHints] BIT NOT NULL DEFAULT 1,
        [ShowCorrectAnswers] BIT NOT NULL DEFAULT 1,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_OrthoeopyTests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_OrthoeopyTests_Assignments] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignments]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrthoeopyTests_AspNetUsers] FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_OrthoeopyTests_AssignmentId] ON [dbo].[OrthoeopyTests]([AssignmentId])
    CREATE INDEX [IX_OrthoeopyTests_TeacherId] ON [dbo].[OrthoeopyTests]([TeacherId])
    CREATE INDEX [IX_OrthoeopyTests_IsActive] ON [dbo].[OrthoeopyTests]([IsActive])
    
    PRINT 'Таблица OrthoeopyTests создана'
END
GO

-- =============================================
-- ТАБЛИЦА REGULARTESTS (классические тесты)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegularTests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RegularTests] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [AssignmentId] INT NOT NULL,
        [TeacherId] NVARCHAR(450) NOT NULL,
        [TimeLimit] INT NOT NULL DEFAULT 30,
        [MaxAttempts] INT NOT NULL DEFAULT 1,
        [StartDate] DATETIME2 NULL,
        [EndDate] DATETIME2 NULL,
        [ShowHints] BIT NOT NULL DEFAULT 1,
        [ShowCorrectAnswers] BIT NOT NULL DEFAULT 1,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [Type] INT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_RegularTests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_RegularTests_Assignments] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignments]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RegularTests_AspNetUsers] FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_RegularTests_AssignmentId] ON [dbo].[RegularTests]([AssignmentId])
    CREATE INDEX [IX_RegularTests_TeacherId] ON [dbo].[RegularTests]([TeacherId])
    CREATE INDEX [IX_RegularTests_IsActive] ON [dbo].[RegularTests]([IsActive])
    
    PRINT 'Таблица RegularTests создана'
END
GO

-- =============================================
-- ТАБЛИЦА SPELLINGQUESTIONS (вопросы по орфографии)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpellingQuestions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SpellingQuestions] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SpellingTestId] INT NOT NULL,
        [WordWithGap] NVARCHAR(200) NOT NULL,
        [CorrectLetter] NVARCHAR(10) NOT NULL,
        [FullWord] NVARCHAR(200) NOT NULL,
        [Hint] NVARCHAR(500) NULL,
        [OrderIndex] INT NOT NULL,
        [Points] INT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_SpellingQuestions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_SpellingQuestions_SpellingTests] FOREIGN KEY ([SpellingTestId]) REFERENCES [dbo].[SpellingTests]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_SpellingQuestions_SpellingTestId] ON [dbo].[SpellingQuestions]([SpellingTestId])
    CREATE INDEX [IX_SpellingQuestions_OrderIndex] ON [dbo].[SpellingQuestions]([OrderIndex])
    
    PRINT 'Таблица SpellingQuestions создана'
END
GO

-- =============================================
-- ТАБЛИЦА PUNCTUATIONQUESTIONS (вопросы по пунктуации)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PunctuationQuestions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PunctuationQuestions] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PunctuationTestId] INT NOT NULL,
        [SentenceWithNumbers] NVARCHAR(1000) NOT NULL,
        [CorrectPositions] NVARCHAR(50) NOT NULL,
        [PlainSentence] NVARCHAR(1000) NULL,
        [Hint] NVARCHAR(500) NULL,
        [OrderIndex] INT NOT NULL,
        [Points] INT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_PunctuationQuestions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_PunctuationQuestions_PunctuationTests] FOREIGN KEY ([PunctuationTestId]) REFERENCES [dbo].[PunctuationTests]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_PunctuationQuestions_PunctuationTestId] ON [dbo].[PunctuationQuestions]([PunctuationTestId])
    CREATE INDEX [IX_PunctuationQuestions_OrderIndex] ON [dbo].[PunctuationQuestions]([OrderIndex])
    
    PRINT 'Таблица PunctuationQuestions создана'
END
GO

-- =============================================
-- ТАБЛИЦА ORTHOEOBYQUESTIONS (вопросы по орфоэпии)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrthoeopyQuestions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrthoeopyQuestions] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [OrthoeopyTestId] INT NOT NULL,
        [Word] NVARCHAR(200) NOT NULL,
        [StressPosition] INT NOT NULL,
        [WordWithStress] NVARCHAR(200) NOT NULL,
        [WrongStressPositions] NVARCHAR(100) NULL,
        [Hint] NVARCHAR(500) NULL,
        [OrderIndex] INT NOT NULL,
        [Points] INT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_OrthoeopyQuestions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_OrthoeopyQuestions_OrthoeopyTests] FOREIGN KEY ([OrthoeopyTestId]) REFERENCES [dbo].[OrthoeopyTests]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_OrthoeopyQuestions_OrthoeopyTestId] ON [dbo].[OrthoeopyQuestions]([OrthoeopyTestId])
    CREATE INDEX [IX_OrthoeopyQuestions_OrderIndex] ON [dbo].[OrthoeopyQuestions]([OrderIndex])
    
    PRINT 'Таблица OrthoeopyQuestions создана'
END
GO

-- =============================================
-- ТАБЛИЦА REGULARQUESTIONS (вопросы классических тестов)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegularQuestions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RegularQuestions] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RegularTestId] INT NOT NULL,
        [Text] NVARCHAR(1000) NOT NULL,
        [Type] INT NOT NULL DEFAULT 1,
        [Points] INT NOT NULL DEFAULT 1,
        [OrderIndex] INT NOT NULL,
        [Hint] NVARCHAR(500) NULL,
        [Explanation] NVARCHAR(1000) NULL,
        CONSTRAINT [PK_RegularQuestions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_RegularQuestions_RegularTests] FOREIGN KEY ([RegularTestId]) REFERENCES [dbo].[RegularTests]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_RegularQuestions_RegularTestId] ON [dbo].[RegularQuestions]([RegularTestId])
    CREATE INDEX [IX_RegularQuestions_OrderIndex] ON [dbo].[RegularQuestions]([OrderIndex])
    
    PRINT 'Таблица RegularQuestions создана'
END
GO

-- =============================================
-- ТАБЛИЦА REGULARQUESTIONOPTIONS (варианты ответов)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegularQuestionOptions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RegularQuestionOptions] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RegularQuestionId] INT NOT NULL,
        [Text] NVARCHAR(500) NOT NULL,
        [IsCorrect] BIT NOT NULL DEFAULT 0,
        [OrderIndex] INT NOT NULL,
        CONSTRAINT [PK_RegularQuestionOptions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_RegularQuestionOptions_RegularQuestions] FOREIGN KEY ([RegularQuestionId]) REFERENCES [dbo].[RegularQuestions]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_RegularQuestionOptions_RegularQuestionId] ON [dbo].[RegularQuestionOptions]([RegularQuestionId])
    CREATE INDEX [IX_RegularQuestionOptions_OrderIndex] ON [dbo].[RegularQuestionOptions]([OrderIndex])
    
    PRINT 'Таблица RegularQuestionOptions создана'
END
GO

-- =============================================
-- ТАБЛИЦА SPELLINGTESTRESULTS (результаты тестов по орфографии)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpellingTestResults]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SpellingTestResults] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SpellingTestId] INT NOT NULL,
        [StudentId] INT NOT NULL,
        [AttemptNumber] INT NOT NULL DEFAULT 1,
        [StartedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [CompletedAt] DATETIME2 NULL,
        [Score] INT NOT NULL DEFAULT 0,
        [MaxScore] INT NOT NULL DEFAULT 0,
        [IsCompleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_SpellingTestResults] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_SpellingTestResults_SpellingTests] FOREIGN KEY ([SpellingTestId]) REFERENCES [dbo].[SpellingTests]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SpellingTestResults_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_SpellingTestResults_SpellingTestId] ON [dbo].[SpellingTestResults]([SpellingTestId])
    CREATE INDEX [IX_SpellingTestResults_StudentId] ON [dbo].[SpellingTestResults]([StudentId])
    
    PRINT 'Таблица SpellingTestResults создана'
END
GO

-- =============================================
-- ТАБЛИЦА PUNCTUATIONTESTRESULTS (результаты тестов по пунктуации)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PunctuationTestResults]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PunctuationTestResults] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PunctuationTestId] INT NOT NULL,
        [StudentId] INT NOT NULL,
        [AttemptNumber] INT NOT NULL DEFAULT 1,
        [StartedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [CompletedAt] DATETIME2 NULL,
        [Score] INT NOT NULL DEFAULT 0,
        [MaxScore] INT NOT NULL DEFAULT 0,
        [IsCompleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_PunctuationTestResults] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_PunctuationTestResults_PunctuationTests] FOREIGN KEY ([PunctuationTestId]) REFERENCES [dbo].[PunctuationTests]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PunctuationTestResults_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_PunctuationTestResults_PunctuationTestId] ON [dbo].[PunctuationTestResults]([PunctuationTestId])
    CREATE INDEX [IX_PunctuationTestResults_StudentId] ON [dbo].[PunctuationTestResults]([StudentId])
    
    PRINT 'Таблица PunctuationTestResults создана'
END
GO

-- =============================================
-- ТАБЛИЦА ORTHOEOBYTESTRESULTS (результаты тестов по орфоэпии)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrthoeopyTestResults]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrthoeopyTestResults] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [OrthoeopyTestId] INT NOT NULL,
        [StudentId] INT NOT NULL,
        [AttemptNumber] INT NOT NULL DEFAULT 1,
        [StartedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [CompletedAt] DATETIME2 NULL,
        [Score] INT NOT NULL DEFAULT 0,
        [MaxScore] INT NOT NULL DEFAULT 0,
        [IsCompleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_OrthoeopyTestResults] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_OrthoeopyTestResults_OrthoeopyTests] FOREIGN KEY ([OrthoeopyTestId]) REFERENCES [dbo].[OrthoeopyTests]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrthoeopyTestResults_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_OrthoeopyTestResults_OrthoeopyTestId] ON [dbo].[OrthoeopyTestResults]([OrthoeopyTestId])
    CREATE INDEX [IX_OrthoeopyTestResults_StudentId] ON [dbo].[OrthoeopyTestResults]([StudentId])
    
    PRINT 'Таблица OrthoeopyTestResults создана'
END
GO

-- =============================================
-- ТАБЛИЦА REGULARTESTRESULTS (результаты классических тестов)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegularTestResults]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RegularTestResults] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RegularTestId] INT NOT NULL,
        [StudentId] INT NOT NULL,
        [AttemptNumber] INT NOT NULL DEFAULT 1,
        [StartedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [CompletedAt] DATETIME2 NULL,
        [Score] INT NOT NULL DEFAULT 0,
        [MaxScore] INT NOT NULL DEFAULT 0,
        [IsCompleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_RegularTestResults] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_RegularTestResults_RegularTests] FOREIGN KEY ([RegularTestId]) REFERENCES [dbo].[RegularTests]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RegularTestResults_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_RegularTestResults_RegularTestId] ON [dbo].[RegularTestResults]([RegularTestId])
    CREATE INDEX [IX_RegularTestResults_StudentId] ON [dbo].[RegularTestResults]([StudentId])
    
    PRINT 'Таблица RegularTestResults создана'
END
GO

-- =============================================
-- ТАБЛИЦА SPELLINGANSWERS (ответы по орфографии)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpellingAnswers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SpellingAnswers] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SpellingQuestionId] INT NOT NULL,
        [TestResultId] INT NOT NULL,
        [StudentAnswer] NVARCHAR(10) NOT NULL,
        [IsCorrect] BIT NOT NULL DEFAULT 0,
        [Points] INT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_SpellingAnswers] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_SpellingAnswers_SpellingQuestions] FOREIGN KEY ([SpellingQuestionId]) REFERENCES [dbo].[SpellingQuestions]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SpellingAnswers_SpellingTestResults] FOREIGN KEY ([TestResultId]) REFERENCES [dbo].[SpellingTestResults]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_SpellingAnswers_SpellingQuestionId] ON [dbo].[SpellingAnswers]([SpellingQuestionId])
    CREATE INDEX [IX_SpellingAnswers_TestResultId] ON [dbo].[SpellingAnswers]([TestResultId])
    
    PRINT 'Таблица SpellingAnswers создана'
END
GO

-- =============================================
-- ТАБЛИЦА PUNCTUATIONANSWERS (ответы по пунктуации)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PunctuationAnswers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PunctuationAnswers] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PunctuationQuestionId] INT NOT NULL,
        [TestResultId] INT NOT NULL,
        [StudentAnswer] NVARCHAR(50) NOT NULL,
        [IsCorrect] BIT NOT NULL DEFAULT 0,
        [Points] INT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_PunctuationAnswers] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_PunctuationAnswers_PunctuationQuestions] FOREIGN KEY ([PunctuationQuestionId]) REFERENCES [dbo].[PunctuationQuestions]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PunctuationAnswers_PunctuationTestResults] FOREIGN KEY ([TestResultId]) REFERENCES [dbo].[PunctuationTestResults]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_PunctuationAnswers_PunctuationQuestionId] ON [dbo].[PunctuationAnswers]([PunctuationQuestionId])
    CREATE INDEX [IX_PunctuationAnswers_TestResultId] ON [dbo].[PunctuationAnswers]([TestResultId])
    
    PRINT 'Таблица PunctuationAnswers создана'
END
GO

-- =============================================
-- ТАБЛИЦА ORTHOEOBYANSWERS (ответы по орфоэпии)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrthoeopyAnswers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrthoeopyAnswers] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [OrthoeopyQuestionId] INT NOT NULL,
        [TestResultId] INT NOT NULL,
        [StudentAnswer] INT NOT NULL,
        [IsCorrect] BIT NOT NULL DEFAULT 0,
        [Points] INT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_OrthoeopyAnswers] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_OrthoeopyAnswers_OrthoeopyQuestions] FOREIGN KEY ([OrthoeopyQuestionId]) REFERENCES [dbo].[OrthoeopyQuestions]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrthoeopyAnswers_OrthoeopyTestResults] FOREIGN KEY ([TestResultId]) REFERENCES [dbo].[OrthoeopyTestResults]([Id]) ON DELETE CASCADE
    )
    
    CREATE INDEX [IX_OrthoeopyAnswers_OrthoeopyQuestionId] ON [dbo].[OrthoeopyAnswers]([OrthoeopyQuestionId])
    CREATE INDEX [IX_OrthoeopyAnswers_TestResultId] ON [dbo].[OrthoeopyAnswers]([TestResultId])
    
    PRINT 'Таблица OrthoeopyAnswers создана'
END
GO

-- =============================================
-- ТАБЛИЦА REGULARANSWERS (ответы на классические тесты)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegularAnswers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RegularAnswers] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RegularQuestionId] INT NOT NULL,
        [TestResultId] INT NOT NULL,
        [StudentAnswer] NVARCHAR(1000) NULL,
        [SelectedOptionId] INT NULL,
        [IsCorrect] BIT NOT NULL DEFAULT 0,
        [Points] INT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_RegularAnswers] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_RegularAnswers_RegularQuestions] FOREIGN KEY ([RegularQuestionId]) REFERENCES [dbo].[RegularQuestions]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RegularAnswers_RegularTestResults] FOREIGN KEY ([TestResultId]) REFERENCES [dbo].[RegularTestResults]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RegularAnswers_RegularQuestionOptions] FOREIGN KEY ([SelectedOptionId]) REFERENCES [dbo].[RegularQuestionOptions]([Id]) ON DELETE NO ACTION
    )
    
    CREATE INDEX [IX_RegularAnswers_RegularQuestionId] ON [dbo].[RegularAnswers]([RegularQuestionId])
    CREATE INDEX [IX_RegularAnswers_TestResultId] ON [dbo].[RegularAnswers]([TestResultId])
    CREATE INDEX [IX_RegularAnswers_SelectedOptionId] ON [dbo].[RegularAnswers]([SelectedOptionId])
    
    PRINT 'Таблица RegularAnswers создана'
END
GO

PRINT 'Скрипт создания базы данных выполнен успешно!'
GO

