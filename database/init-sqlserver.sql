-- ============================================================
-- SaigonAudioTour — SQL Server Init Script
-- Dùng cho Docker container (Azure SQL Edge)
-- Tương đương migrations/001_init.sql nhưng T-SQL syntax
-- ============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SaigonAudioTour')
BEGIN
    CREATE DATABASE SaigonAudioTour;
END
GO

USE SaigonAudioTour;
GO

-- ── Roles ──────────────────────────────────────────────────
IF OBJECT_ID('Roles', 'U') IS NULL
CREATE TABLE Roles (
    Id          NVARCHAR(50)  NOT NULL PRIMARY KEY,
    Name        NVARCHAR(50)  NOT NULL UNIQUE,
    Description NVARCHAR(200)
);
GO

IF OBJECT_ID('RolePermissions', 'U') IS NULL
CREATE TABLE RolePermissions (
    RoleId     NVARCHAR(50) NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    Permission NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, Permission)
);
GO

-- ── Admins ─────────────────────────────────────────────────
IF OBJECT_ID('Admins', 'U') IS NULL
CREATE TABLE Admins (
    Id               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Username         NVARCHAR(100) NOT NULL UNIQUE,
    Email            NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash     NVARCHAR(500) NOT NULL,
    RoleId           NVARCHAR(50)  NOT NULL REFERENCES Roles(Id),
    IsActive         BIT NOT NULL DEFAULT 1,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    TwoFactorSecret  NVARCHAR(200),
    CreatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- ── AppUsers ────────────────────────────────────────────────
IF OBJECT_ID('AppUsers', 'U') IS NULL
CREATE TABLE AppUsers (
    Id                 UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Email              NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash       NVARCHAR(500) NOT NULL,
    FullName           NVARCHAR(200) NOT NULL,
    Phone              NVARCHAR(20),
    SubscriptionStatus NVARCHAR(20)  NOT NULL DEFAULT 'Free',
    PremiumUntil       DATETIME2,
    TwoFactorEnabled   BIT NOT NULL DEFAULT 0,
    TwoFactorSecret    NVARCHAR(200),
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE INDEX IF NOT EXISTS IX_AppUsers_Email    ON AppUsers(Email);
CREATE INDEX IF NOT EXISTS IX_AppUsers_SubStatus ON AppUsers(SubscriptionStatus);
GO

-- ── Refresh Tokens ──────────────────────────────────────────
IF OBJECT_ID('RefreshTokens', 'U') IS NULL
CREATE TABLE RefreshTokens (
    Id        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId    UNIQUEIDENTIFIER NOT NULL REFERENCES AppUsers(Id) ON DELETE CASCADE,
    Token     NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

IF OBJECT_ID('AdminRefreshTokens', 'U') IS NULL
CREATE TABLE AdminRefreshTokens (
    Id        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    AdminId   UNIQUEIDENTIFIER NOT NULL REFERENCES Admins(Id) ON DELETE CASCADE,
    Token     NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- ── POIs ────────────────────────────────────────────────────
IF OBJECT_ID('POIs', 'U') IS NULL
CREATE TABLE POIs (
    Id            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name          NVARCHAR(200) NOT NULL,
    Description   NVARCHAR(2000),
    Latitude      FLOAT NOT NULL,
    Longitude     FLOAT NOT NULL,
    RadiusMeters  INT   NOT NULL DEFAULT 80,
    Priority      INT   NOT NULL DEFAULT 5,
    IsStopStation BIT   NOT NULL DEFAULT 0,
    ImageUrl      NVARCHAR(500),
    IsActive      BIT   NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- ── Tours ───────────────────────────────────────────────────
IF OBJECT_ID('Tours', 'U') IS NULL
CREATE TABLE Tours (
    Id               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name             NVARCHAR(200) NOT NULL,
    Description      NVARCHAR(2000),
    EstimatedMinutes INT,
    TotalDistanceKm  FLOAT,
    TicketPrice      DECIMAL(18,2) NOT NULL DEFAULT 0,
    CoverImageUrl    NVARCHAR(500),
    IsActive         BIT NOT NULL DEFAULT 1,
    CreatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- ── TourPOIs ────────────────────────────────────────────────
IF OBJECT_ID('TourPOIs', 'U') IS NULL
CREATE TABLE TourPOIs (
    TourId     UNIQUEIDENTIFIER NOT NULL REFERENCES Tours(Id) ON DELETE CASCADE,
    POIId      UNIQUEIDENTIFIER NOT NULL REFERENCES POIs(Id)  ON DELETE CASCADE,
    OrderIndex INT NOT NULL DEFAULT 0,
    CONSTRAINT PK_TourPOIs PRIMARY KEY (TourId, POIId)
);
GO

-- ── Narrations ──────────────────────────────────────────────
IF OBJECT_ID('Narrations', 'U') IS NULL
CREATE TABLE Narrations (
    Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    POIId           UNIQUEIDENTIFIER NOT NULL REFERENCES POIs(Id) ON DELETE CASCADE,
    LanguageCode    NVARCHAR(5)   NOT NULL DEFAULT 'vi',
    VoiceType       NVARCHAR(10)  NOT NULL DEFAULT 'female',
    ContentText     NVARCHAR(MAX) NOT NULL,
    AudioUrl        NVARCHAR(500),
    DurationSeconds INT,
    IsPremium       BIT NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Narration UNIQUE (POIId, LanguageCode, VoiceType)
);
GO

CREATE INDEX IX_Narrations_POI ON Narrations(POIId, LanguageCode);
GO

-- ── Subscription Plans ──────────────────────────────────────
IF OBJECT_ID('SubscriptionPlans', 'U') IS NULL
CREATE TABLE SubscriptionPlans (
    Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name         NVARCHAR(100)  NOT NULL,
    Price        DECIMAL(18,2)  NOT NULL,
    DurationDays INT            NOT NULL,
    Description  NVARCHAR(500),
    IsActive     BIT NOT NULL DEFAULT 1
);
GO

-- ── Payment Transactions ─────────────────────────────────────
IF OBJECT_ID('PaymentTransactions', 'U') IS NULL
CREATE TABLE PaymentTransactions (
    Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId       UNIQUEIDENTIFIER NOT NULL REFERENCES AppUsers(Id),
    PlanId       UNIQUEIDENTIFIER NOT NULL REFERENCES SubscriptionPlans(Id),
    VNPayOrderId NVARCHAR(100)  NOT NULL UNIQUE,
    Amount       DECIMAL(18,2)  NOT NULL,
    Currency     NVARCHAR(10)   NOT NULL DEFAULT 'VND',
    Status       NVARCHAR(20)   NOT NULL DEFAULT 'Pending',
    Provider     NVARCHAR(50)   NOT NULL DEFAULT 'VNPay',
    RawResponse  NVARCHAR(MAX),
    CreatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE INDEX IX_Payments_User    ON PaymentTransactions(UserId);
CREATE INDEX IX_Payments_OrderId ON PaymentTransactions(VNPayOrderId);
CREATE INDEX IX_Payments_Status  ON PaymentTransactions(Status);
GO

-- ── User Logs ────────────────────────────────────────────────
IF OBJECT_ID('UserLogs', 'U') IS NULL
CREATE TABLE UserLogs (
    Id        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId    UNIQUEIDENTIFIER NOT NULL REFERENCES AppUsers(Id) ON DELETE CASCADE,
    POIId     UNIQUEIDENTIFIER REFERENCES POIs(Id),
    Latitude  FLOAT,
    Longitude FLOAT,
    EventType NVARCHAR(50) NOT NULL,
    Metadata  NVARCHAR(MAX),
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE INDEX IX_UserLogs_User ON UserLogs(UserId, Timestamp);
CREATE INDEX IX_UserLogs_POI  ON UserLogs(POIId,  Timestamp);
GO

-- ── QR Login Tokens ──────────────────────────────────────────
IF OBJECT_ID('QrLoginTokens', 'U') IS NULL
CREATE TABLE QrLoginTokens (
    Id        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Token     NVARCHAR(100)    NOT NULL UNIQUE,
    UserId    UNIQUEIDENTIFIER REFERENCES AppUsers(Id),
    Status    NVARCHAR(20)     NOT NULL DEFAULT 'Pending',
    ExpiresAt DATETIME2        NOT NULL,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE INDEX IX_QrTokens ON QrLoginTokens(Token, Status);
GO

-- ── Seed: Roles ─────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = 'role-super')
INSERT INTO Roles (Id, Name, Description) VALUES
    ('role-super',  'SuperAdmin', N'Toàn quyền hệ thống'),
    ('role-editor', 'Editor',     N'Quản lý nội dung POI, Tour, Narration'),
    ('role-viewer', 'Viewer',     N'Chỉ xem dashboard và báo cáo');
GO

INSERT INTO RolePermissions (RoleId, Permission)
SELECT v.RoleId, v.Permission
FROM (VALUES
    ('role-super',  'pois:write'),
    ('role-super',  'tours:write'),
    ('role-super',  'narrations:write'),
    ('role-super',  'analytics:read'),
    ('role-super',  'users:manage'),
    ('role-editor', 'pois:write'),
    ('role-editor', 'tours:write'),
    ('role-editor', 'narrations:write'),
    ('role-viewer', 'analytics:read')
) AS v(RoleId, Permission)
WHERE NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.RoleId = v.RoleId AND rp.Permission = v.Permission
);
GO

-- ── Seed: Subscription Plans ─────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM SubscriptionPlans)
INSERT INTO SubscriptionPlans (Id, Name, Price, DurationDays, Description) VALUES
    (NEWID(), N'Gói 1 Tháng',  49000,  30,  N'Truy cập không giới hạn trong 1 tháng'),
    (NEWID(), N'Gói 3 Tháng', 119000,  90,  N'Tiết kiệm 19% so với gói tháng'),
    (NEWID(), N'Gói 1 Năm',   399000, 365, N'Tiết kiệm 32% — tốt nhất cho du khách thường xuyên');
GO

-- ── Seed: POIs — Quận 1, TP.HCM ─────────────────────────────
IF NOT EXISTS (SELECT 1 FROM POIs)
INSERT INTO POIs (Name, Description, Latitude, Longitude, RadiusMeters, Priority, IsStopStation) VALUES
(N'Nhà thờ Đức Bà',
 N'Công trình kiến trúc Pháp nổi tiếng, xây dựng 1863–1880. Biểu tượng của TP.HCM.',
 10.7797, 106.6990, 80, 1, 1),
(N'Bưu điện Trung tâm Sài Gòn',
 N'Tòa nhà do kiến trúc sư Gustave Eiffel thiết kế, hoàn thành năm 1891.',
 10.7796, 106.6997, 70, 2, 0),
(N'Dinh Độc Lập',
 N'Dinh thự lịch sử, nơi diễn ra sự kiện thống nhất đất nước 30/4/1975.',
 10.7769, 106.6956, 100, 1, 1),
(N'Bảo tàng Chứng tích Chiến tranh',
 N'Bảo tàng lưu giữ hiện vật về cuộc kháng chiến, thu hút hàng triệu khách mỗi năm.',
 10.7796, 106.6921, 80, 2, 0),
(N'Chợ Bến Thành',
 N'Biểu tượng thương mại của Sài Gòn từ thế kỷ 20.',
 10.7722, 106.6983, 100, 1, 1),
(N'Phố đi bộ Nguyễn Huệ',
 N'Tuyến phố đi bộ dài 670m, điểm tụ họp văn hóa và sự kiện lớn của thành phố.',
 10.7741, 106.7032, 90, 2, 0),
(N'Nhà hát Thành phố',
 N'Nhà hát Opera phong cách Pháp, xây năm 1900. Tọa lạc cuối đường Nguyễn Huệ.',
 10.7762, 106.7030, 70, 2, 0),
(N'Bảo tàng TP. Hồ Chí Minh',
 N'Tòa nhà mang dấu ấn Pháp, từng là Dinh Gia Long. Trưng bày lịch sử thành phố.',
 10.7773, 106.7002, 80, 3, 0),
(N'Công viên 30/4',
 N'Công viên lịch sử đối diện Dinh Độc Lập, nơi diễn ra nhiều sự kiện văn hóa lớn.',
 10.7780, 106.6960, 80, 3, 0),
(N'Nhà thờ Tân Định',
 N'Nhà thờ màu hồng nổi tiếng xây năm 1876, một trong những công trình Công giáo lâu đời nhất Sài Gòn.',
 10.7893, 106.6875, 70, 3, 0);
GO
