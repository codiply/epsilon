CREATE TABLE [dbo].[Address] (
    [Id]                  NVARCHAR (128) NOT NULL,
    [UniqueAddressCode]   NVARCHAR (32)  NULL,
    [Line1]               NVARCHAR (256) NOT NULL,
    [Line2]               NVARCHAR (256) NULL,
    [Line3]               NVARCHAR (256) NULL,
    [CityTown]            NVARCHAR (64)  NOT NULL,
    [CountyStateProvince] NVARCHAR (64)  NULL,
    [PostcodeOrZip]       NVARCHAR (16)  NOT NULL,
    [CountryId]           NCHAR (2)      NOT NULL,
    [Timestamp]           ROWVERSION     NOT NULL,
    CONSTRAINT [PK_dbo.Address] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Address_dbo.Country_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([Id]) ON DELETE CASCADE
);






GO
CREATE NONCLUSTERED INDEX [IX_CountryId]
    ON [dbo].[Address]([CountryId] ASC);

