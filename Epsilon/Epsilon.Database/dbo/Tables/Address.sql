CREATE TABLE [dbo].[Address] (
    [Id]                 UNIQUEIDENTIFIER   DEFAULT (newsequentialid()) NOT NULL,
    [UniqueAddressCode]  NVARCHAR (32)      NULL,
    [Line1]              NVARCHAR (256)     NOT NULL,
    [Line2]              NVARCHAR (256)     NULL,
    [Line3]              NVARCHAR (256)     NULL,
    [Line4]              NVARCHAR (256)     NULL,
    [Locality]           NVARCHAR (64)      NOT NULL,
    [Region]             NVARCHAR (64)      NULL,
    [Postcode]           NVARCHAR (16)      NOT NULL,
    [CountryId]          NCHAR (2)          NOT NULL,
    [Latitude]           DECIMAL (18, 9)    NULL,
    [Longitude]          DECIMAL (18, 9)    NULL,
    [CreatedOn]          DATETIMEOFFSET (7) NOT NULL,
    [CreatedById]        NVARCHAR (128)     NOT NULL,
    [CreatedByIpAddress] NVARCHAR (39)      NULL,
    [Timestamp]          ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.Address] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Address_dbo.Country_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([Id]),
    CONSTRAINT [FK_dbo.Address_dbo.User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[User] ([Id])
);




















GO
CREATE NONCLUSTERED INDEX [IX_CountryId]
    ON [dbo].[Address]([CountryId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Postcode]
    ON [dbo].[Address]([Postcode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CreatedById]
    ON [dbo].[Address]([CreatedById] ASC);

