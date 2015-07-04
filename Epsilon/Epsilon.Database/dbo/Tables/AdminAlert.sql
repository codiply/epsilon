CREATE TABLE [dbo].[AdminAlert] (
    [Id]     UNIQUEIDENTIFIER   DEFAULT (newsequentialid()) NOT NULL,
    [Key]    NVARCHAR (256)     NOT NULL,
    [SentOn] DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_dbo.AdminAlert] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE NONCLUSTERED INDEX [IX_AdminAlert_Key_Sent_On]
    ON [dbo].[AdminAlert]([Key] ASC, [SentOn] ASC);

