BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260620005213_AddFlowTransitionLog'
)
BEGIN
    CREATE TABLE [FlowTransitionLog] (
        [FlowTransitionLogId] int NOT NULL IDENTITY,
        [From] int NOT NULL,
        [To] int NOT NULL,
        [FromCycleStatus] int NOT NULL,
        [Trigger] int NOT NULL,
        [Path] int NOT NULL,
        [MachineId] int NOT NULL,
        [BarCodeId] int NOT NULL,
        [CycleId] int NOT NULL,
        [ResultValidation] int NOT NULL,
        [TimeStamp] datetime2 NOT NULL,
        CONSTRAINT [PK_FlowTransitionLog_FlowTransitionLogId] PRIMARY KEY ([FlowTransitionLogId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260620005213_AddFlowTransitionLog'
)
BEGIN
    CREATE INDEX [IDX_FlowTransitionLog_BarCodeId] ON [FlowTransitionLog] ([BarCodeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260620005213_AddFlowTransitionLog'
)
BEGIN
    CREATE INDEX [IDX_FlowTransitionLog_MachineId] ON [FlowTransitionLog] ([MachineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260620005213_AddFlowTransitionLog'
)
BEGIN
    CREATE INDEX [IDX_FlowTransitionLog_TimeStamp] ON [FlowTransitionLog] ([TimeStamp]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260620005213_AddFlowTransitionLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260620005213_AddFlowTransitionLog', N'10.0.9');
END;

COMMIT;
GO

