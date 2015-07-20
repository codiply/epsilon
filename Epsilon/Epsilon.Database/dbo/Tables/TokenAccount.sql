CREATE TABLE [dbo].[CoinAccount] (
    [Id]             NVARCHAR (128)     NOT NULL,
    [CreatedOn]      DATETIMEOFFSET (7) NOT NULL,
    [LastSnapshotOn] DATETIMEOFFSET (7) NOT NULL,
    [Timestamp]      ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.CoinAccount] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.CoinAccount_dbo.User_Id] FOREIGN KEY ([Id]) REFERENCES [dbo].[User] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Id]
    ON [dbo].[CoinAccount]([Id] ASC);

