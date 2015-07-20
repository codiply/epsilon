CREATE TABLE [dbo].[PostcodeGeometry] (
    [CountryId]                  NCHAR (2)          NOT NULL,
    [Postcode]                   NVARCHAR (16)      NOT NULL,
    [Latitude]                   FLOAT (53)         NOT NULL,
    [Longitude]                  FLOAT (53)         NOT NULL,
    [ViewportNortheastLatitude]  FLOAT (53)         NOT NULL,
    [ViewportNortheastLongitude] FLOAT (53)         NOT NULL,
    [ViewportSouthwestLatitude]  FLOAT (53)         NOT NULL,
    [ViewportSouthwestLongitude] FLOAT (53)         NOT NULL,
    [GeocodedOn]                 DATETIMEOFFSET (7) NOT NULL,
    [Timestamp]                  ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.PostcodeGeometry] PRIMARY KEY CLUSTERED ([CountryId] ASC, [Postcode] ASC),
    CONSTRAINT [FK_dbo.PostcodeGeometry_dbo.Country_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([Id])
);






GO
CREATE NONCLUSTERED INDEX [IX_CountryId]
    ON [dbo].[PostcodeGeometry]([CountryId] ASC);

