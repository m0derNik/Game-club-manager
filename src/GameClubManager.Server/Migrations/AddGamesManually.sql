-- Создание таблицы Games
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Games')
BEGIN
    CREATE TABLE [Games] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Genre] int NOT NULL,
        [Developer] nvarchar(50) NOT NULL,
        [Publisher] nvarchar(50) NOT NULL,
        [ReleaseDate] datetime2 NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        [InstallationPath] nvarchar(max) NOT NULL,
        [IsMultiplayer] bit NOT NULL,
        [IsAvailable] bit NOT NULL,
        [RequiredAgeRating] int NOT NULL,
        [PopularityRating] int NOT NULL,
        CONSTRAINT [PK_Games] PRIMARY KEY ([Id])
    );
    
    PRINT 'Таблица Games создана успешно';
END
ELSE
BEGIN
    PRINT 'Таблица Games уже существует';
END
GO

-- Создание или обновление таблицы GamePreferences
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GamePreferences')
BEGIN
    CREATE TABLE [GamePreferences] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [GameId] int NOT NULL,
        [LastPlayed] datetime2 NOT NULL,
        [TotalPlaytime] time NOT NULL,
        [IsFavorite] bit NOT NULL,
        CONSTRAINT [PK_GamePreferences] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GamePreferences_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GamePreferences_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_GamePreferences_GameId] ON [GamePreferences] ([GameId]);
    CREATE INDEX [IX_GamePreferences_UserId] ON [GamePreferences] ([UserId]);
    
    PRINT 'Таблица GamePreferences создана успешно';
END
ELSE
BEGIN
    -- Проверяем существование столбца GameId
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'GameId' AND object_id = OBJECT_ID('GamePreferences'))
    BEGIN
        -- Добавляем недостающие столбцы
        ALTER TABLE [GamePreferences] ADD [GameId] int NOT NULL DEFAULT 0;
        ALTER TABLE [GamePreferences] ADD [TotalPlaytime] time NOT NULL DEFAULT '00:00:00';
        ALTER TABLE [GamePreferences] ADD [IsFavorite] bit NOT NULL DEFAULT 0;
        
        -- Добавляем внешний ключ
        ALTER TABLE [GamePreferences] ADD CONSTRAINT [FK_GamePreferences_Games_GameId] 
            FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION;
        
        -- Создаем индекс
        CREATE INDEX [IX_GamePreferences_GameId] ON [GamePreferences] ([GameId]);
        
        PRINT 'Таблица GamePreferences успешно обновлена';
    END
    ELSE
    BEGIN
        PRINT 'Таблица GamePreferences уже содержит все необходимые столбцы';
    END
END
GO

-- Добавляем запись в __EFMigrationsHistory
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250515153417_AddGamesTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250515153417_AddGamesTable', N'8.0.8');
    
    PRINT 'Миграция добавлена в историю миграций';
END
ELSE
BEGIN
    PRINT 'Миграция уже есть в истории миграций';
END
GO 