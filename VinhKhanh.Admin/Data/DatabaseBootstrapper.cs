using Microsoft.EntityFrameworkCore;
using System.Data;

namespace VinhKhanh.Admin.Data;

public static class DatabaseBootstrapper
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();

        if (await HasLegacySchemaAsync(db))
        {
            await EnsureLegacyCompatibilityAsync(db);
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        await EnsureMonitoringCompatibilityAsync(db);
        await ClearPresenceSessionsAsync(db);
        await DbSeeder.SeedRolesAsync(services);
    }

    private static async Task<bool> HasLegacySchemaAsync(AppDbContext db)
    {
        try
        {
            var result = await db.Database
                .GetDbConnection()
                .ExecuteScalarIntAsync("""
                    DECLARE @hasMigration BIT = 0;
                    IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
                       AND EXISTS (
                            SELECT 1
                            FROM dbo.__EFMigrationsHistory
                            WHERE MigrationId = N'20260417150948_InitialCleanDB'
                       )
                        SET @hasMigration = 1;

                    SELECT CASE
                        WHEN OBJECT_ID(N'dbo.Restaurants', N'U') IS NOT NULL
                        THEN 1 ELSE 0 END AS [Value]
                    """);

            return result == 1;
        }
        catch
        {
            return false;
        }
    }

    private static async Task EnsureLegacyCompatibilityAsync(AppDbContext db)
    {
        foreach (var sql in LegacyCompatibilityScripts)
        {
            await db.Database.ExecuteSqlRawAsync(sql);
        }
    }

    private static async Task ClearPresenceSessionsAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.ActiveSessions', N'U') IS NOT NULL
                DELETE FROM dbo.ActiveSessions;
            """);
    }

    private static async Task EnsureMonitoringCompatibilityAsync(AppDbContext db)
    {
        foreach (var sql in MonitoringCompatibilityScripts)
        {
            await db.Database.ExecuteSqlRawAsync(sql);
        }
    }

    private static readonly string[] MonitoringCompatibilityScripts =
    [
        """
        IF OBJECT_ID(N'dbo.Restaurants', N'U') IS NOT NULL
           AND COL_LENGTH('dbo.Restaurants', 'QrSlug') IS NULL
            ALTER TABLE dbo.Restaurants ADD QrSlug NVARCHAR(120) NULL;
        """,
        """
        IF OBJECT_ID(N'dbo.Restaurants', N'U') IS NOT NULL
           AND NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'IX_Restaurants_QrSlug'
                  AND object_id = OBJECT_ID(N'dbo.Restaurants')
           )
            CREATE UNIQUE INDEX IX_Restaurants_QrSlug ON dbo.Restaurants(QrSlug)
            WHERE QrSlug IS NOT NULL;
        """,
        """
        IF OBJECT_ID(N'dbo.ListenLogs', N'U') IS NOT NULL
           AND COL_LENGTH('dbo.ListenLogs', 'AnonymousSessionId') IS NULL
            ALTER TABLE dbo.ListenLogs ADD AnonymousSessionId NVARCHAR(64) NULL;
        """,
        """
        IF OBJECT_ID(N'dbo.ListenLogs', N'U') IS NOT NULL
           AND NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'IX_ListenLogs_AnonymousSessionId'
                  AND object_id = OBJECT_ID(N'dbo.ListenLogs')
           )
            CREATE INDEX IX_ListenLogs_AnonymousSessionId ON dbo.ListenLogs(AnonymousSessionId);
        """,
        """
        IF OBJECT_ID(N'dbo.QrScanLogs', N'U') IS NULL
        CREATE TABLE dbo.QrScanLogs (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_QrScanLogs PRIMARY KEY,
            RestaurantId INT NOT NULL,
            QrCode NVARCHAR(200) NOT NULL,
            DevicePlatform NVARCHAR(32) NOT NULL CONSTRAINT DF_QrScanLogs_DevicePlatform DEFAULT N'unknown',
            AnonymousSessionId NVARCHAR(64) NULL,
            ScannedAt DATETIME2 NOT NULL CONSTRAINT DF_QrScanLogs_ScannedAt DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_QrScanLogs_Restaurants_RestaurantId
                FOREIGN KEY (RestaurantId) REFERENCES dbo.Restaurants(Id) ON DELETE CASCADE
        );
        """,
        """
        IF OBJECT_ID(N'dbo.QrScanLogs', N'U') IS NOT NULL
           AND NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'IX_QrScanLogs_ScannedAt'
                  AND object_id = OBJECT_ID(N'dbo.QrScanLogs')
           )
            CREATE INDEX IX_QrScanLogs_ScannedAt ON dbo.QrScanLogs(ScannedAt);
        """,
        """
        IF OBJECT_ID(N'dbo.QrScanLogs', N'U') IS NOT NULL
           AND NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'IX_QrScanLogs_RestaurantId'
                  AND object_id = OBJECT_ID(N'dbo.QrScanLogs')
           )
            CREATE INDEX IX_QrScanLogs_RestaurantId ON dbo.QrScanLogs(RestaurantId);
        """,
        """
        IF OBJECT_ID(N'dbo.QrScanLogs', N'U') IS NOT NULL
           AND NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'IX_QrScanLogs_AnonymousSessionId'
                  AND object_id = OBJECT_ID(N'dbo.QrScanLogs')
           )
            CREATE INDEX IX_QrScanLogs_AnonymousSessionId ON dbo.QrScanLogs(AnonymousSessionId);
        """
    ];

    private static readonly string[] LegacyCompatibilityScripts =
    [
        """
        IF COL_LENGTH('dbo.Restaurants', 'UpdatedAt') IS NULL
            ALTER TABLE dbo.Restaurants
            ADD UpdatedAt DATETIME NOT NULL
                CONSTRAINT DF_Restaurants_UpdatedAt DEFAULT GETDATE() WITH VALUES;
        """,
        """
        IF COL_LENGTH('dbo.Restaurants', 'Description_kr') IS NULL
            ALTER TABLE dbo.Restaurants ADD Description_kr NVARCHAR(MAX) NULL;
        """,
        """
        IF COL_LENGTH('dbo.Restaurants', 'AudioContent_kr') IS NULL
            ALTER TABLE dbo.Restaurants ADD AudioContent_kr NVARCHAR(MAX) NULL;
        """,
        """
        IF COL_LENGTH('dbo.Restaurants', 'Description_cn') IS NULL
            ALTER TABLE dbo.Restaurants ADD Description_cn NVARCHAR(MAX) NULL;
        """,
        """
        IF COL_LENGTH('dbo.Restaurants', 'AudioContent_cn') IS NULL
            ALTER TABLE dbo.Restaurants ADD AudioContent_cn NVARCHAR(MAX) NULL;
        """,
        """
        IF COL_LENGTH('dbo.QRCodes', 'IsActive') IS NULL
            ALTER TABLE dbo.QRCodes
            ADD IsActive BIT NOT NULL
                CONSTRAINT DF_QRCodes_IsActive DEFAULT CAST(1 AS BIT) WITH VALUES;
        """,
        """
        IF COL_LENGTH('dbo.QRCodes', 'ImagePath') IS NULL
            ALTER TABLE dbo.QRCodes ADD ImagePath NVARCHAR(500) NULL;
        """,
        """
        IF OBJECT_ID(N'dbo.AudioFiles', N'U') IS NULL
        CREATE TABLE dbo.AudioFiles (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AudioFiles PRIMARY KEY,
            RestaurantId INT NOT NULL,
            Language NVARCHAR(10) NOT NULL,
            FileName NVARCHAR(260) NOT NULL,
            FilePath NVARCHAR(500) NOT NULL,
            FileSizeBytes BIGINT NOT NULL,
            IsPublished BIT NOT NULL CONSTRAINT DF_AudioFiles_IsPublished DEFAULT CAST(1 AS BIT),
            UploadedAt DATETIME NOT NULL CONSTRAINT DF_AudioFiles_UploadedAt DEFAULT GETDATE(),
            CONSTRAINT FK_AudioFiles_Restaurants_RestaurantId
                FOREIGN KEY (RestaurantId) REFERENCES dbo.Restaurants(Id) ON DELETE CASCADE
        );
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AudioFiles_RestaurantId_Language' AND object_id = OBJECT_ID(N'dbo.AudioFiles'))
            CREATE INDEX IX_AudioFiles_RestaurantId_Language ON dbo.AudioFiles(RestaurantId, Language);
        """,
        """
        IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
        CREATE TABLE dbo.AspNetRoles (
            Id NVARCHAR(450) NOT NULL CONSTRAINT PK_AspNetRoles PRIMARY KEY,
            Name NVARCHAR(256) NULL,
            NormalizedName NVARCHAR(256) NULL,
            ConcurrencyStamp NVARCHAR(MAX) NULL
        );
        """,
        """
        IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
        CREATE TABLE dbo.AspNetUsers (
            Id NVARCHAR(450) NOT NULL CONSTRAINT PK_AspNetUsers PRIMARY KEY,
            UserName NVARCHAR(256) NULL,
            NormalizedUserName NVARCHAR(256) NULL,
            Email NVARCHAR(256) NULL,
            NormalizedEmail NVARCHAR(256) NULL,
            EmailConfirmed BIT NOT NULL,
            PasswordHash NVARCHAR(MAX) NULL,
            SecurityStamp NVARCHAR(MAX) NULL,
            ConcurrencyStamp NVARCHAR(MAX) NULL,
            PhoneNumber NVARCHAR(MAX) NULL,
            PhoneNumberConfirmed BIT NOT NULL,
            TwoFactorEnabled BIT NOT NULL,
            LockoutEnd DATETIMEOFFSET NULL,
            LockoutEnabled BIT NOT NULL,
            AccessFailedCount INT NOT NULL
        );
        """,
        """
        IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NULL
        CREATE TABLE dbo.AspNetRoleClaims (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY,
            RoleId NVARCHAR(450) NOT NULL,
            ClaimType NVARCHAR(MAX) NULL,
            ClaimValue NVARCHAR(MAX) NULL,
            CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId
                FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
        );
        """,
        """
        IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NULL
        CREATE TABLE dbo.AspNetUserClaims (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AspNetUserClaims PRIMARY KEY,
            UserId NVARCHAR(450) NOT NULL,
            ClaimType NVARCHAR(MAX) NULL,
            ClaimValue NVARCHAR(MAX) NULL,
            CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId
                FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
        );
        """,
        """
        IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NULL
        CREATE TABLE dbo.AspNetUserLogins (
            LoginProvider NVARCHAR(128) NOT NULL,
            ProviderKey NVARCHAR(128) NOT NULL,
            ProviderDisplayName NVARCHAR(MAX) NULL,
            UserId NVARCHAR(450) NOT NULL,
            CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
            CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId
                FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
        );
        """,
        """
        IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NULL
        CREATE TABLE dbo.AspNetUserRoles (
            UserId NVARCHAR(450) NOT NULL,
            RoleId NVARCHAR(450) NOT NULL,
            CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
            CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId
                FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
            CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId
                FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
        );
        """,
        """
        IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NULL
        CREATE TABLE dbo.AspNetUserTokens (
            UserId NVARCHAR(450) NOT NULL,
            LoginProvider NVARCHAR(128) NOT NULL,
            Name NVARCHAR(128) NOT NULL,
            Value NVARCHAR(MAX) NULL,
            CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
            CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId
                FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
        );
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'RoleNameIndex' AND object_id = OBJECT_ID(N'dbo.AspNetRoles'))
            CREATE UNIQUE INDEX RoleNameIndex ON dbo.AspNetRoles(NormalizedName)
            WHERE NormalizedName IS NOT NULL;
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'EmailIndex' AND object_id = OBJECT_ID(N'dbo.AspNetUsers'))
            CREATE INDEX EmailIndex ON dbo.AspNetUsers(NormalizedEmail);
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UserNameIndex' AND object_id = OBJECT_ID(N'dbo.AspNetUsers'))
            CREATE UNIQUE INDEX UserNameIndex ON dbo.AspNetUsers(NormalizedUserName)
            WHERE NormalizedUserName IS NOT NULL;
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID(N'dbo.AspNetRoleClaims'))
            CREATE INDEX IX_AspNetRoleClaims_RoleId ON dbo.AspNetRoleClaims(RoleId);
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID(N'dbo.AspNetUserClaims'))
            CREATE INDEX IX_AspNetUserClaims_UserId ON dbo.AspNetUserClaims(UserId);
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID(N'dbo.AspNetUserLogins'))
            CREATE INDEX IX_AspNetUserLogins_UserId ON dbo.AspNetUserLogins(UserId);
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID(N'dbo.AspNetUserRoles'))
            CREATE INDEX IX_AspNetUserRoles_RoleId ON dbo.AspNetUserRoles(RoleId);
        """,
        """
        IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
        CREATE TABLE dbo.__EFMigrationsHistory (
            MigrationId NVARCHAR(150) NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
            ProductVersion NVARCHAR(32) NOT NULL
        );
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260417150948_InitialCleanDB')
            INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
            VALUES (N'20260417150948_InitialCleanDB', N'9.0.0');
        """
    ];
}

file static class DbConnectionExtensions
{
    public static async Task<int> ExecuteScalarIntAsync(this System.Data.Common.DbConnection connection, string sql)
    {
        var shouldClose = connection.State == ConnectionState.Closed;
        if (shouldClose)
            await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            var value = await command.ExecuteScalarAsync();
            return Convert.ToInt32(value);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }
}
