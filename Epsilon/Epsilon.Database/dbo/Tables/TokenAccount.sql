CREATE TABLE [dbo].[TokenAccount] (
    [Id]             NVARCHAR (128)     NOT NULL,
    [CreatedOn]      DATETIMEOFFSET (7) NOT NULL,
    [LastSnapshotOn] DATETIMEOFFSET (7) NOT NULL,
    [Timestamp]      ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.TokenAccount] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.TokenAccount_dbo.User_Id] FOREIGN KEY ([Id]) REFERENCES [dbo].[User] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Id]
    ON [dbo].[TokenAccount]([Id] ASC);

