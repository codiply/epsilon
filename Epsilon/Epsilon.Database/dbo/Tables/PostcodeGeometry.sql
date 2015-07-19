CREATE TABLE [dbo].[PostcodeGeometry] (
    [CountryId]                  NVARCHAR (128)     NOT NULL,
    [Postcode]                   NVARCHAR (128)     NOT NULL,
    [Latitude]                   FLOAT (53)         NOT NULL,
    [Longitude]                  FLOAT (53)         NOT NULL,
    [ViewportNortheastLatitude]  FLOAT (53)         NOT NULL,
    [ViewportNortheastLongitude] FLOAT (53)         NOT NULL,
    [ViewportSouthwestLatitude]  FLOAT (53)         NOT NULL,
    [ViewportSouthwestLongitude] FLOAT (53)         NOT NULL,
    [GeocodedOn]                 DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_dbo.PostcodeGeometry] PRIMARY KEY CLUSTERED ([CountryId] ASC, [Postcode] ASC)
);

