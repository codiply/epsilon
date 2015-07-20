﻿CREATE TABLE [dbo].[TokenAccountSnapshot] (
    [Id]          BIGINT             IDENTITY (1, 1) NOT NULL,
    [AccountId]   NVARCHAR (128)     NOT NULL,
    [Balance]     DECIMAL (18, 2)    NOT NULL,
    [MadeOn]      DATETIMEOFFSET (7) NOT NULL,
    [IsFinalised] BIT                NOT NULL,
    [Timestamp]   ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.TokenAccountSnapshot] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.TokenAccountSnapshot_dbo.TokenAccount_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[TokenAccount] ([Id]) ON DELETE CASCADE
);










GO
CREATE NONCLUSTERED INDEX [IX_TokenAccountSnapshot_AccountId_IsFinalised_MadeOn]
    ON [dbo].[TokenAccountSnapshot]([AccountId] ASC, [IsFinalised] ASC, [MadeOn] ASC);

