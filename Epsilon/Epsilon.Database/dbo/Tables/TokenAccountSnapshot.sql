CREATE TABLE [dbo].[CoinAccountSnapshot] (
    [Id]          BIGINT             IDENTITY (1, 1) NOT NULL,
    [AccountId]   NVARCHAR (128)     NOT NULL,
    [Balance]     DECIMAL (18, 2)    NOT NULL,
    [MadeOn]      DATETIMEOFFSET (7) NOT NULL,
    [IsFinalised] BIT                NOT NULL,
    [Timestamp]   ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.CoinAccountSnapshot] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.CoinAccountSnapshot_dbo.CoinAccount_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[CoinAccount] ([Id]) ON DELETE CASCADE
);










GO
CREATE NONCLUSTERED INDEX [IX_CoinAccountSnapshot_AccountId_IsFinalised_MadeOn]
    ON [dbo].[CoinAccountSnapshot]([AccountId] ASC, [IsFinalised] ASC, [MadeOn] ASC);

