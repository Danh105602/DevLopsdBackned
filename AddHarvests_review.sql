BEGIN TRANSACTION;
CREATE TABLE [Harvests] (
    [Id] int NOT NULL IDENTITY,
    [PlantingId] int NOT NULL,
    [HarvestDate] datetime2 NOT NULL,
    [QuantityKg] float NOT NULL,
    [Quality] nvarchar(30) NOT NULL,
    [Notes] nvarchar(500) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Harvests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Harvests_Plantings_PlantingId] FOREIGN KEY ([PlantingId]) REFERENCES [Plantings] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Harvests_PlantingId] ON [Harvests] ([PlantingId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261124212351_AddHarvests', N'10.0.14');

COMMIT;
GO

