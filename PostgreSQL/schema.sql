-- TunRTC PostgreSQL Schema
-- Version: 1.0

-- Create Users table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "Email" VARCHAR(100) NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastLoginAt" TIMESTAMP NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

-- Create Sessions table
CREATE TABLE IF NOT EXISTS "Sessions" (
    "Id" SERIAL PRIMARY KEY,
    "SessionId" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(100) NOT NULL,
    "CreatorId" INTEGER NOT NULL,
    "MaxParticipants" INTEGER NOT NULL DEFAULT 10,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "EndedAt" TIMESTAMP NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "Type" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Sessions_Users" FOREIGN KEY ("CreatorId") 
        REFERENCES "Users"("Id") ON DELETE RESTRICT
);

-- Create SessionParticipants table
CREATE TABLE IF NOT EXISTS "SessionParticipants" (
    "Id" SERIAL PRIMARY KEY,
    "SessionId" INTEGER NOT NULL,
    "UserId" INTEGER NOT NULL,
    "ConnectionId" VARCHAR(100) NOT NULL,
    "JoinedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LeftAt" TIMESTAMP NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "IsMuted" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsVideoEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT "FK_SessionParticipants_Sessions" FOREIGN KEY ("SessionId") 
        REFERENCES "Sessions"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SessionParticipants_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- Create ConnectionLogs table
CREATE TABLE IF NOT EXISTS "ConnectionLogs" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "SessionId" INTEGER NULL,
    "ConnectionId" VARCHAR(100) NOT NULL,
    "IpAddress" VARCHAR(50) NOT NULL,
    "UserAgent" TEXT NOT NULL,
    "ConnectedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DisconnectedAt" TIMESTAMP NULL,
    "DisconnectReason" TEXT NULL
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_Sessions_SessionId" ON "Sessions"("SessionId");
CREATE INDEX IF NOT EXISTS "IX_Sessions_CreatorId" ON "Sessions"("CreatorId");
CREATE INDEX IF NOT EXISTS "IX_Sessions_Status" ON "Sessions"("Status");

CREATE INDEX IF NOT EXISTS "IX_SessionParticipants_SessionId" ON "SessionParticipants"("SessionId");
CREATE INDEX IF NOT EXISTS "IX_SessionParticipants_UserId" ON "SessionParticipants"("UserId");
CREATE INDEX IF NOT EXISTS "IX_SessionParticipants_ConnectionId" ON "SessionParticipants"("ConnectionId");

CREATE INDEX IF NOT EXISTS "IX_ConnectionLogs_UserId" ON "ConnectionLogs"("UserId");
CREATE INDEX IF NOT EXISTS "IX_ConnectionLogs_ConnectionId" ON "ConnectionLogs"("ConnectionId");
CREATE INDEX IF NOT EXISTS "IX_ConnectionLogs_ConnectedAt" ON "ConnectionLogs"("ConnectedAt");

-- Insert demo user (password: demo123)
INSERT INTO "Users" ("Username", "Email", "PasswordHash", "CreatedAt", "IsActive")
VALUES 
    ('demo', 'demo@tunrtc.com', 'XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=', CURRENT_TIMESTAMP, TRUE)
ON CONFLICT DO NOTHING;

-- Comments
COMMENT ON TABLE "Users" IS 'Stores registered users';
COMMENT ON TABLE "Sessions" IS 'Stores WebRTC sessions';
COMMENT ON TABLE "SessionParticipants" IS 'Tracks participants in each session';
COMMENT ON TABLE "ConnectionLogs" IS 'Logs all WebSocket connections for analytics';
