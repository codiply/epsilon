CREATE TABLE [dbo].[GeoipInfo] (
    [IpAddress]         NVARCHAR (128)     NOT NULL,
    [CountryCode]       NVARCHAR (2)       NOT NULL,
    [Latitude]          FLOAT (53)         NULL,
    [Longitude]         FLOAT (53)         NULL,
    [GeoipProviderName] NVARCHAR (32)      NOT NULL,
    [RecordedOn]        DATETIMEOFFSET (7) NOT NULL,
    [Timestamp]         ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.GeoipInfo] PRIMARY KEY CLUSTERED ([IpAddress] ASC)
);



