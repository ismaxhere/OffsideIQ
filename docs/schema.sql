-- =============================================================================
-- Offside IQ — PostgreSQL Schema Reference
-- Auto-generated via EF Core migrations. This is for documentation only.
-- Run: dotnet ef migrations add InitialCreate && dotnet ef database update
-- =============================================================================

-- Users
CREATE TABLE "Users" (
    "Id"            UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    "Email"         VARCHAR(256)  NOT NULL UNIQUE,
    "PasswordHash"  TEXT          NOT NULL,
    "DisplayName"   VARCHAR(100)  NOT NULL,
    "Role"          VARCHAR(20)   NOT NULL DEFAULT 'User',
    "CreatedAt"     TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    "LastLoginAt"   TIMESTAMPTZ
);

-- Teams
CREATE TABLE "Teams" (
    "Id"               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name"             VARCHAR(100) NOT NULL,
    "ShortCode"        VARCHAR(5)   NOT NULL UNIQUE,
    "LogoUrl"          TEXT,
    "Stadium"          VARCHAR(150),
    "League"           VARCHAR(100),
    "Country"          VARCHAR(100),
    "CreatedAt"        TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    "CreatedByUserId"  UUID         NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT
);

-- Players
CREATE TABLE "Players" (
    "Id"            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    "TeamId"        UUID         NOT NULL REFERENCES "Teams"("Id") ON DELETE CASCADE,
    "Name"          VARCHAR(100) NOT NULL,
    "Position"      VARCHAR(10),        -- GK | DEF | MID | FWD
    "JerseyNumber"  INT,
    "Nationality"   VARCHAR(100),
    "DateOfBirth"   DATE,
    "CreatedAt"     TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Matches
CREATE TABLE "Matches" (
    "Id"               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    "HomeTeamId"       UUID         NOT NULL REFERENCES "Teams"("Id") ON DELETE RESTRICT,
    "AwayTeamId"       UUID         NOT NULL REFERENCES "Teams"("Id") ON DELETE RESTRICT,
    "HomeScore"        INT          NOT NULL DEFAULT 0,
    "AwayScore"        INT          NOT NULL DEFAULT 0,
    "MatchDate"        TIMESTAMPTZ  NOT NULL,
    "Competition"      VARCHAR(100),
    "Venue"            VARCHAR(150),
    "Status"           SMALLINT     NOT NULL DEFAULT 0,  -- 0=Scheduled 1=Live 2=Completed 3=Postponed 4=Cancelled
    "CreatedAt"        TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    "CreatedByUserId"  UUID         NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT
);
CREATE INDEX idx_matches_home_team  ON "Matches"("HomeTeamId");
CREATE INDEX idx_matches_away_team  ON "Matches"("AwayTeamId");
CREATE INDEX idx_matches_date       ON "Matches"("MatchDate" DESC);

-- Match Stats (1-to-1 with Matches)
CREATE TABLE "MatchStats" (
    "Id"                  UUID           PRIMARY KEY DEFAULT gen_random_uuid(),
    "MatchId"             UUID           NOT NULL UNIQUE REFERENCES "Matches"("Id") ON DELETE CASCADE,
    "HomePossession"      NUMERIC(5,2)   NOT NULL DEFAULT 50,
    "AwayPossession"      NUMERIC(5,2)   NOT NULL DEFAULT 50,
    "HomeShotsTotal"      INT            NOT NULL DEFAULT 0,
    "HomeShotsOnTarget"   INT            NOT NULL DEFAULT 0,
    "AwayShotsTotal"      INT            NOT NULL DEFAULT 0,
    "AwayShotsOnTarget"   INT            NOT NULL DEFAULT 0,
    "HomePasses"          INT            NOT NULL DEFAULT 0,
    "HomePassAccuracy"    INT            NOT NULL DEFAULT 0,
    "AwayPasses"          INT            NOT NULL DEFAULT 0,
    "AwayPassAccuracy"    INT            NOT NULL DEFAULT 0,
    "HomeYellowCards"     INT            NOT NULL DEFAULT 0,
    "HomeRedCards"        INT            NOT NULL DEFAULT 0,
    "AwayYellowCards"     INT            NOT NULL DEFAULT 0,
    "AwayRedCards"        INT            NOT NULL DEFAULT 0,
    "HomeCorners"         INT            NOT NULL DEFAULT 0,
    "AwayCorners"         INT            NOT NULL DEFAULT 0,
    "HomeFouls"           INT            NOT NULL DEFAULT 0,
    "AwayFouls"           INT            NOT NULL DEFAULT 0,
    "HomeXg"              NUMERIC(4,2),
    "AwayXg"              NUMERIC(4,2)
);

-- Match Notes
CREATE TABLE "MatchNotes" (
    "Id"         UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    "MatchId"    UUID          NOT NULL REFERENCES "Matches"("Id") ON DELETE CASCADE,
    "UserId"     UUID          NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
    "Content"    VARCHAR(2000) NOT NULL,
    "IsPublic"   BOOLEAN       NOT NULL DEFAULT FALSE,
    "CreatedAt"  TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    "UpdatedAt"  TIMESTAMPTZ
);

-- Player Ratings (per match)
CREATE TABLE "PlayerRatings" (
    "Id"        UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    "MatchId"   UUID          NOT NULL REFERENCES "Matches"("Id") ON DELETE CASCADE,
    "PlayerId"  UUID          NOT NULL REFERENCES "Players"("Id") ON DELETE CASCADE,
    "Rating"    NUMERIC(3,1)  NOT NULL CHECK ("Rating" BETWEEN 1.0 AND 10.0),
    "Notes"     TEXT,
    UNIQUE ("MatchId", "PlayerId")
);
