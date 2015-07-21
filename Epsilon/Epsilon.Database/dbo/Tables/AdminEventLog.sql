CREATE TABLE [dbo].[AdminEventLog] (
    [Id]         BIGINT             IDENTITY (1, 1) NOT NULL,
    [Key]       NVARCHAR (128)     NOT NULL,
    [ExtraInfo]  NVARCHAR (256)     NULL,
    [RecordedOn] DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_dbo.AdminEventLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_AdminEventLog_Key_RecordedOn]
    ON [dbo].[AdminEventLog]([Key] ASC, [RecordedOn] ASC);

