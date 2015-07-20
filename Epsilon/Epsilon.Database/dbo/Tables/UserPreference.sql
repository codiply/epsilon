CREATE TABLE [dbo].[UserPreference] (
    [Id]         NVARCHAR (128)     NOT NULL,
    [LanguageId] NVARCHAR (8)       NULL,
    [UpdatedOn]  DATETIMEOFFSET (7) NOT NULL,
    [Timestamp]  ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.UserPreference] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.UserPreference_dbo.Language_LanguageId] FOREIGN KEY ([LanguageId]) REFERENCES [dbo].[Language] ([Id]),
    CONSTRAINT [FK_dbo.UserPreference_dbo.User_Id] FOREIGN KEY ([Id]) REFERENCES [dbo].[User] ([Id]) ON DELETE CASCADE
);








GO



GO
CREATE NONCLUSTERED INDEX [IX_LanguageId]
    ON [dbo].[UserPreference]([LanguageId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Id]
    ON [dbo].[UserPreference]([Id] ASC);

