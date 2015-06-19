CREATE TABLE [dbo].[UserPreference] (
    [Id]         UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [LanguageId] NVARCHAR (8)     NULL,
    [Timestamp]  ROWVERSION       NOT NULL,
    [UserId]     NVARCHAR (128)   NOT NULL,
    CONSTRAINT [PK_dbo.UserPreference] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.UserPreference_dbo.Language_LanguageId] FOREIGN KEY ([LanguageId]) REFERENCES [dbo].[Language] ([Id]),
    CONSTRAINT [FK_dbo.UserPreference_dbo.User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[UserPreference]([UserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_LanguageId]
    ON [dbo].[UserPreference]([LanguageId] ASC);

