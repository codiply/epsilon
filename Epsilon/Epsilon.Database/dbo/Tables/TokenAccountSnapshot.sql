CREATE TABLE [dbo].[TokenAccountSnapshot] (
    [Id]          BIGINT             IDENTITY (1, 1) NOT NULL,
    [AccountId]   NVARCHAR (128)     NOT NULL,
    [Balance]     DECIMAL (16, 4)    NOT NULL,
    [MadeOn]      DATETIMEOFFSET (7) NOT NULL,
    [IsFinalised] BIT                NOT NULL,
    [Timestamp]   ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.TokenAccountSnapshot] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.TokenAccountSnapshot_dbo.TokenAccount_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[TokenAccount] ([Id]) ON DELETE CASCADE
);














GO
CREATE NONCLUSTERED INDEX [IX_TokenAccountSnapshot_MadeOn_AccountId_IsFinalised]
    ON [dbo].[TokenAccountSnapshot]([MadeOn] ASC, [AccountId] ASC, [IsFinalised] ASC);

