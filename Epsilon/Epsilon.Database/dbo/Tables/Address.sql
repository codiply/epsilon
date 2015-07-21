CREATE TABLE [dbo].[Address] (
    [Id]                  BIGINT             IDENTITY (1, 1) NOT NULL,
    [UniqueId]            UNIQUEIDENTIFIER   NOT NULL,
    [DistinctAddressCode] NVARCHAR (32)      NULL,
    [Line1]               NVARCHAR (256)     NOT NULL,
    [Line2]               NVARCHAR (256)     NULL,
    [Line3]               NVARCHAR (256)     NULL,
    [Line4]               NVARCHAR (256)     NULL,
    [Locality]            NVARCHAR (64)      NOT NULL,
    [Region]              NVARCHAR (64)      NULL,
    [Postcode]            NVARCHAR (16)      NOT NULL,
    [CountryId]           NCHAR (2)          NOT NULL,
    [CreatedOn]           DATETIMEOFFSET (7) NOT NULL,
    [CreatedById]         NVARCHAR (128)     NOT NULL,
    [CreatedByIpAddress]  NVARCHAR (39)      NULL,
    [Timestamp]           ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.Address] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Address_dbo.Country_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([Id]),
    CONSTRAINT [FK_dbo.Address_dbo.PostcodeGeometry_CountryId_Postcode] FOREIGN KEY ([CountryId], [Postcode]) REFERENCES [dbo].[PostcodeGeometry] ([CountryId], [Postcode]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.Address_dbo.User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[User] ([Id])
);
































GO
CREATE NONCLUSTERED INDEX [IX_CountryId]
    ON [dbo].[Address]([CountryId] ASC);


GO



GO



GO



GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Address_UniqueId]
    ON [dbo].[Address]([UniqueId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Address_Postcode]
    ON [dbo].[Address]([Postcode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CountryId_Postcode]
    ON [dbo].[Address]([CountryId] ASC, [Postcode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Address_CreatedOn_CreatedByIpAddress]
    ON [dbo].[Address]([CreatedOn] ASC, [CreatedByIpAddress] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Address_CreatedOn_CreatedById]
    ON [dbo].[Address]([CreatedOn] ASC, [CreatedById] ASC);

