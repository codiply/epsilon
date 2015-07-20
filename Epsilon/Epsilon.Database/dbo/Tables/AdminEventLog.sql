CREATE TABLE [dbo].[AdminEventLog] (
    [Id]         BIGINT             IDENTITY (1, 1) NOT NULL,
    [Type]       NVARCHAR (128)     NOT NULL,
    [ExtraInfo]  NVARCHAR (256)     NULL,
    [RecordedOn] DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_dbo.AdminEventLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_AdminEventLog_Type_RecordedOn]
    ON [dbo].[AdminEventLog]([Type] ASC, [RecordedOn] ASC);

