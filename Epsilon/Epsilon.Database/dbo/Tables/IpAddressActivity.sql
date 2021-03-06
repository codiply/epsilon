CREATE TABLE [dbo].[IpAddressActivity] (
    [Id]           BIGINT             IDENTITY (1, 1) NOT NULL,
    [UserId]       NVARCHAR (128)     NULL,
    [ActivityType] NVARCHAR (32)      NULL,
    [IpAddress]    NVARCHAR (39)      NULL,
    [RecordedOn]   DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_dbo.IpAddressActivity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.IpAddressActivity_dbo.User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
);












GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[IpAddressActivity]([UserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_IpAddressActivity_RecordedOn_ActivityType_IpAddress]
    ON [dbo].[IpAddressActivity]([RecordedOn] ASC, [ActivityType] ASC, [IpAddress] ASC);

