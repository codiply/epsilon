CREATE TABLE [dbo].[PropertyInfoAccess] (
    [Id]                 BIGINT             IDENTITY (1, 1) NOT NULL,
    [UniqueId]           UNIQUEIDENTIFIER   NOT NULL,
    [UserId]             NVARCHAR (128)     NOT NULL,
    [AddressId]          BIGINT             NOT NULL,
    [CreatedOn]          DATETIMEOFFSET (7) NOT NULL,
    [CreatedByIpAddress] NVARCHAR (39)      NOT NULL,
    [Timestamp]          ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.PropertyInfoAccess] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.PropertyInfoAccess_dbo.Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Address] ([Id]),
    CONSTRAINT [FK_dbo.PropertyInfoAccess_dbo.User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_PropertyInfoAccess_CreatedOn_CreatedByIpAddress]
    ON [dbo].[PropertyInfoAccess]([CreatedOn] ASC, [CreatedByIpAddress] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PropertyInfoAccess_CreatedOn]
    ON [dbo].[PropertyInfoAccess]([CreatedOn] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_AddressId]
    ON [dbo].[PropertyInfoAccess]([AddressId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PropertyInfoAccess_UserId_CreatedByIpAddress]
    ON [dbo].[PropertyInfoAccess]([UserId] ASC, [CreatedByIpAddress] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PropertyInfoAccess_CreatedOn_UserId]
    ON [dbo].[PropertyInfoAccess]([CreatedOn] ASC, [UserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PropertyInfoAccess_UniqueId_UserId]
    ON [dbo].[PropertyInfoAccess]([UniqueId] ASC, [UserId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PropertyInfoAccess_UniqueId]
    ON [dbo].[PropertyInfoAccess]([UniqueId] ASC);

