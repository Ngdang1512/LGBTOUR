-- ============================================================
-- Migration 001 — Initial Schema
-- SaigonAudioTour Database
-- ============================================================

-- Roles
CREATE TABLE IF NOT EXISTS Roles (
    Id          TEXT PRIMARY KEY,
    Name        TEXT NOT NULL UNIQUE,     -- SuperAdmin | Editor | Viewer
    Description TEXT
);

-- Role Permissions
CREATE TABLE IF NOT EXISTS RolePermissions (
    RoleId     TEXT NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    Permission TEXT NOT NULL,             -- pois:write | tours:write | narrations:write | analytics:read | users:manage
    PRIMARY KEY (RoleId, Permission)
);

-- Admin accounts
CREATE TABLE IF NOT EXISTS Admins (
    Id                TEXT PRIMARY KEY,
    Username          TEXT NOT NULL UNIQUE,
    Email             TEXT NOT NULL UNIQUE,
    PasswordHash      TEXT NOT NULL,
    RoleId            TEXT NOT NULL REFERENCES Roles(Id),
    IsActive          INTEGER NOT NULL DEFAULT 1,
    TwoFactorEnabled  INTEGER NOT NULL DEFAULT 0,
    TwoFactorSecret   TEXT,
    CreatedAt         TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt         TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Tourist users
CREATE TABLE IF NOT EXISTS AppUsers (
    Id                  TEXT PRIMARY KEY,
    Email               TEXT NOT NULL UNIQUE,
    PasswordHash        TEXT NOT NULL,
    FullName            TEXT NOT NULL,
    Phone               TEXT,
    SubscriptionStatus  TEXT NOT NULL DEFAULT 'Free',  -- Free | Premium | Expired
    PremiumUntil        TEXT,
    TwoFactorEnabled    INTEGER NOT NULL DEFAULT 0,
    TwoFactorSecret     TEXT,
    CreatedAt           TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt           TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_users_email ON AppUsers(Email);
CREATE INDEX IF NOT EXISTS idx_users_subscription ON AppUsers(SubscriptionStatus);

-- Refresh tokens (tourist)
CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id         TEXT PRIMARY KEY,
    UserId     TEXT NOT NULL REFERENCES AppUsers(Id) ON DELETE CASCADE,
    Token      TEXT NOT NULL UNIQUE,
    ExpiresAt  TEXT NOT NULL,
    IsRevoked  INTEGER NOT NULL DEFAULT 0,
    CreatedAt  TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Admin refresh tokens
CREATE TABLE IF NOT EXISTS AdminRefreshTokens (
    Id         TEXT PRIMARY KEY,
    AdminId    TEXT NOT NULL REFERENCES Admins(Id) ON DELETE CASCADE,
    Token      TEXT NOT NULL UNIQUE,
    ExpiresAt  TEXT NOT NULL,
    IsRevoked  INTEGER NOT NULL DEFAULT 0,
    CreatedAt  TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Points of Interest
CREATE TABLE IF NOT EXISTS POIs (
    Id           TEXT PRIMARY KEY,
    Name         TEXT NOT NULL,
    Description  TEXT,
    Latitude     REAL NOT NULL,
    Longitude    REAL NOT NULL,
    RadiusMeters INTEGER NOT NULL DEFAULT 80,
    Priority     INTEGER NOT NULL DEFAULT 5,
    IsStopStation INTEGER NOT NULL DEFAULT 0,
    ImageUrl     TEXT,
    IsActive     INTEGER NOT NULL DEFAULT 1,
    CreatedAt    TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt    TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Tours
CREATE TABLE IF NOT EXISTS Tours (
    Id               TEXT PRIMARY KEY,
    Name             TEXT NOT NULL,
    Description      TEXT,
    EstimatedMinutes INTEGER,
    TotalDistanceKm  REAL,
    TicketPrice      REAL NOT NULL DEFAULT 0,
    CoverImageUrl    TEXT,
    IsActive         INTEGER NOT NULL DEFAULT 1,
    CreatedAt        TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt        TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Tour–POI ordering
CREATE TABLE IF NOT EXISTS TourPOIs (
    TourId     TEXT NOT NULL REFERENCES Tours(Id) ON DELETE CASCADE,
    POIId      TEXT NOT NULL REFERENCES POIs(Id) ON DELETE CASCADE,
    OrderIndex INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (TourId, POIId)
);

-- Narrations (audio content per POI)
CREATE TABLE IF NOT EXISTS Narrations (
    Id              TEXT PRIMARY KEY,
    POIId           TEXT NOT NULL REFERENCES POIs(Id) ON DELETE CASCADE,
    LanguageCode    TEXT NOT NULL DEFAULT 'vi',   -- vi | en
    VoiceType       TEXT NOT NULL DEFAULT 'female', -- male | female
    ContentText     TEXT NOT NULL,
    AudioUrl        TEXT,
    DurationSeconds INTEGER,
    IsPremium       INTEGER NOT NULL DEFAULT 0,
    CreatedAt       TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt       TEXT NOT NULL DEFAULT (datetime('now')),
    UNIQUE (POIId, LanguageCode, VoiceType)
);

CREATE INDEX IF NOT EXISTS idx_narrations_poi ON Narrations(POIId, LanguageCode);

-- Subscription plans
CREATE TABLE IF NOT EXISTS SubscriptionPlans (
    Id           TEXT PRIMARY KEY,
    Name         TEXT NOT NULL,
    Price        REAL NOT NULL,
    DurationDays INTEGER NOT NULL,
    Description  TEXT,
    IsActive     INTEGER NOT NULL DEFAULT 1
);

-- Payment transactions
CREATE TABLE IF NOT EXISTS PaymentTransactions (
    Id            TEXT PRIMARY KEY,
    UserId        TEXT NOT NULL REFERENCES AppUsers(Id),
    PlanId        TEXT NOT NULL REFERENCES SubscriptionPlans(Id),
    VNPayOrderId  TEXT NOT NULL UNIQUE,
    Amount        REAL NOT NULL,
    Currency      TEXT NOT NULL DEFAULT 'VND',
    Status        TEXT NOT NULL DEFAULT 'Pending', -- Pending | Success | Failed | Expired | Refunded
    Provider      TEXT NOT NULL DEFAULT 'VNPay',
    RawResponse   TEXT,
    CreatedAt     TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt     TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_payments_user ON PaymentTransactions(UserId);
CREATE INDEX IF NOT EXISTS idx_payments_orderid ON PaymentTransactions(VNPayOrderId);
CREATE INDEX IF NOT EXISTS idx_payments_status ON PaymentTransactions(Status);

-- User activity log (GPS events, playback events)
CREATE TABLE IF NOT EXISTS UserLogs (
    Id         TEXT PRIMARY KEY,
    UserId     TEXT NOT NULL REFERENCES AppUsers(Id) ON DELETE CASCADE,
    POIId      TEXT REFERENCES POIs(Id),
    Latitude   REAL,
    Longitude  REAL,
    EventType  TEXT NOT NULL, -- GpsUpdate | PoiEntered | PoiExited | NarrationPlay | NarrationStop | NarrationComplete
    Metadata   TEXT,          -- JSON string for extra data
    Timestamp  TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_userlogs_user ON UserLogs(UserId, Timestamp);
CREATE INDEX IF NOT EXISTS idx_userlogs_poi ON UserLogs(POIId, Timestamp);

-- QR login tokens
CREATE TABLE IF NOT EXISTS QrLoginTokens (
    Id        TEXT PRIMARY KEY,
    Token     TEXT NOT NULL UNIQUE,
    UserId    TEXT REFERENCES AppUsers(Id),  -- NULL until scanned
    Status    TEXT NOT NULL DEFAULT 'Pending', -- Pending | Scanned | Confirmed | Expired
    ExpiresAt TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_qr_token ON QrLoginTokens(Token, Status);
