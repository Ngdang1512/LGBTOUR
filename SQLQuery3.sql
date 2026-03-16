DROP TABLE IF EXISTS Audios
DROP TABLE IF EXISTS Logs
DROP TABLE IF EXISTS Narration
DROP TABLE IF EXISTS Tour_POI
DROP TABLE IF EXISTS Tour
DROP TABLE IF EXISTS POI
CREATE TABLE POI(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200),
    Description NVARCHAR(MAX),
    Lat FLOAT,
    Lng FLOAT,
    Radius INT,
    Image NVARCHAR(255),
    AudioPath NVARCHAR(255)
)
GO

CREATE TABLE Tour(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200),
    Description NVARCHAR(MAX),
    Price DECIMAL(10,2)
)
GO

CREATE TABLE Tour_POI(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TourId INT,
    POI_Id INT,
    DisplayOrder INT,
    
    FOREIGN KEY (TourId) REFERENCES Tour(Id),
    FOREIGN KEY (POI_Id) REFERENCES POI(Id)
)
GO

CREATE TABLE Narration(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    POI_Id INT,
    LanguageCode NVARCHAR(10),
    Content NVARCHAR(MAX),

    FOREIGN KEY (POI_Id) REFERENCES POI(Id)
)
GO

CREATE TABLE Logs(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(100),
    POI_Id INT,
    ListenTime DATETIME,
    Lat FLOAT,
    Lng FLOAT,

    FOREIGN KEY (POI_Id) REFERENCES POI(Id)
)
GO

CREATE TABLE Audios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    POI_Id INT NOT NULL,
    LanguageCode NVARCHAR(10) NOT NULL,
    AudioUrl NVARCHAR(500) NOT NULL,
    VoiceType NVARCHAR(50),
    Duration INT,
    FileSize INT,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (POI_Id) REFERENCES POI(Id)
)
GO
CREATE TABLE UserLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY, -- Dùng BIGINT vì số lượng log sẽ rất lớn
    UserId NVARCHAR(100) NOT NULL,       -- ID thiết bị hoặc ID người dùng
    POI_Id INT NULL,                     -- Điểm POI (NULL nếu người dùng đang di chuyển ngoài vùng)
    Lat FLOAT NOT NULL,                  -- Vĩ độ thực tế lúc ghi log
    Lng FLOAT NOT NULL,                  -- Kinh độ thực tế lúc ghi log
    
    -- Phân loại hành động (Ví dụ: 'MOVE', 'ENTER_ZONE', 'START_AUDIO', 'STOP_AUDIO', 'EXIT_ZONE')
    EventType NVARCHAR(50) NOT NULL,     
    
    -- Thời gian người dùng nghe (chỉ ghi khi EventType là 'STOP_AUDIO')
    DurationSeconds INT DEFAULT 0,       
    
    -- Thời điểm ghi log
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (POI_Id) REFERENCES POI(Id)
);

-- Tạo Index để sau này truy vấn Analytics không bị chậm
CREATE INDEX IX_UserLogs_POI ON UserLogs(POI_Id);
CREATE INDEX IX_UserLogs_CreatedAt ON UserLogs(CreatedAt);
