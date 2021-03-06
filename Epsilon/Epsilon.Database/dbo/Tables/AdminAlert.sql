CREATE TABLE [dbo].[AdminAlert] (
    [Id]     INT                IDENTITY (1, 1) NOT NULL,
    [Key]    NVARCHAR (128)     NOT NULL,
    [SentOn] DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_dbo.AdminAlert] PRIMARY KEY CLUSTERED ([Id] ASC)
);












GO
CREATE NONCLUSTERED INDEX [IX_AdminAlert_SentOn_Key]
    ON [dbo].[AdminAlert]([SentOn] ASC, [Key] ASC);

