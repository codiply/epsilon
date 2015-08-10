CREATE TABLE [dbo].[GeoipInfo] (
    [IpAddress]     NVARCHAR (128)     NOT NULL,
    [CountryCode]   NVARCHAR (2)       NOT NULL,
    [ContinentCode] NVARCHAR (2)       NULL,
    [Latitude]      FLOAT (53)         NULL,
    [Longitude]     FLOAT (53)         NULL,
    [RecordedOn]    DATETIMEOFFSET (7) NOT NULL,
    [Timestamp]     ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.GeoipInfo] PRIMARY KEY CLUSTERED ([IpAddress] ASC)
);

