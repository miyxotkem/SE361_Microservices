CREATE DATABASE ELEARNINGSYSTEM
GO
USE ELEARNINGSYSTEM
GO

-- Tables
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    Email VARCHAR(100) UNIQUE,
    FullName NVARCHAR(100),
    PhoneNumber VARCHAR(15) UNIQUE,
    Role VARCHAR(20) NOT NULL CHECK (Role IN ('Student', 'Instructor', 'Admin'))
);

CREATE TABLE Courses (
    CourseID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    InstructorID INT FOREIGN KEY REFERENCES Users(UserID),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Enrollments (
    EnrollmentID INT PRIMARY KEY IDENTITY(1,1),
    StudentID INT FOREIGN KEY REFERENCES Users(UserID),
    CourseID INT FOREIGN KEY REFERENCES Courses(CourseID),
    EnrollmentDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT UQ_Student_Course UNIQUE (StudentID, CourseID)
);

CREATE TABLE Lessons (
    LessonID INT PRIMARY KEY IDENTITY(1,1),
    CourseID INT FOREIGN KEY REFERENCES Courses(CourseID),
    Title NVARCHAR(200) NOT NULL,
    OrderIndex INT
);

CREATE TABLE LessonMedia (
    LessonMediaID INT PRIMARY KEY IDENTITY(1,1),
    LessonID INT FOREIGN KEY REFERENCES Lessons(LessonID),
    MediaType VARCHAR(20) CHECK (MediaType IN ('Image', 'Video')),
    FilePath VARCHAR(255) NOT NULL,
    OrderIndex INT
);

CREATE TABLE Materials (
    MaterialID INT PRIMARY KEY IDENTITY(1,1),
    LessonID INT FOREIGN KEY REFERENCES Lessons(LessonID),
    Title NVARCHAR(200) NOT NULL,
    FilePath VARCHAR(255) NOT NULL
);

CREATE TABLE Assignments (
    AssignmentID INT PRIMARY KEY IDENTITY(1,1),
    CourseID INT FOREIGN KEY REFERENCES Courses(CourseID),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    FilePath VARCHAR(255),
    DueDate DATETIME NOT NULL
);

CREATE TABLE Submissions (
    SubmissionID INT PRIMARY KEY IDENTITY(1,1),
    AssignmentID INT FOREIGN KEY REFERENCES Assignments(AssignmentID),
    StudentID INT FOREIGN KEY REFERENCES Users(UserID),
    FilePath VARCHAR(255) NOT NULL,
    SubmissionDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    Score DECIMAL(5,2),
    InstructorComment NVARCHAR(MAX)
);

CREATE TABLE Quizzes (
    QuizID INT PRIMARY KEY IDENTITY(1,1),
    CourseID INT FOREIGN KEY REFERENCES Courses(CourseID),
    Title NVARCHAR(200) NOT NULL,
    DurationMinutes INT,
    MaxAttempts INT,
    Marks DECIMAL(5,2)
);

CREATE TABLE Questions (
    QuestionID INT PRIMARY KEY IDENTITY(1,1),
    QuizID INT FOREIGN KEY REFERENCES Quizzes(QuizID),
    QuestionText NVARCHAR(MAX) NOT NULL,
    QuestionType VARCHAR(20) DEFAULT 'MultipleChoice'
);

CREATE TABLE Answers (
    AnswerID INT PRIMARY KEY IDENTITY(1,1),
    QuestionID INT FOREIGN KEY REFERENCES Questions(QuestionID),
    AnswerText NVARCHAR(MAX) NOT NULL,
    IsCorrect BIT NOT NULL
);

CREATE TABLE QuizAttempts (
    QuizAttemptID INT PRIMARY KEY IDENTITY(1,1),
    QuizID INT FOREIGN KEY REFERENCES Quizzes(QuizID),
    StudentID INT FOREIGN KEY REFERENCES Users(UserID),
    AttemptDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    Score DECIMAL(5,2),
    InstructorComment NVARCHAR(MAX)
);

CREATE TABLE StudentAnswers (
    StudentAnswerID INT PRIMARY KEY IDENTITY(1,1),
    QuizAttemptID INT FOREIGN KEY REFERENCES QuizAttempts(QuizAttemptID),
    QuestionID INT FOREIGN KEY REFERENCES Questions(QuestionID),
    AnswerID INT FOREIGN KEY REFERENCES Answers(AnswerID)
);

CREATE TABLE Schedules (
    ScheduleID INT PRIMARY KEY IDENTITY(1,1),
    CourseID INT FOREIGN KEY REFERENCES Courses(CourseID),
    DayOfWeek INT CHECK (DayOfWeek BETWEEN 0 AND 6),
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Location NVARCHAR(100),
    OnlineURL NVARCHAR(255),
    LearningFormat NVARCHAR(20) CHECK (LearningFormat in ('Online', 'Offline', 'Hybrid', 'Other'))
);

CREATE TABLE Notifications (
    NotificationID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(100),
    Message NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE UserNotifications (
    UserNotificationID INT PRIMARY KEY IDENTITY(1,1),
    NotificationID INT FOREIGN KEY REFERENCES Notifications(NotificationID),
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    IsRead BIT DEFAULT 0
);

-- Insertions

-- Views & Procedures