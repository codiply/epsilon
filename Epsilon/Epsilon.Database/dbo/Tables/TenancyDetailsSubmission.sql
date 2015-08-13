CREATE TABLE [dbo].[TenancyDetailsSubmission] (
    [Id]                     BIGINT             IDENTITY (1, 1) NOT NULL,
    [UniqueId]               UNIQUEIDENTIFIER   NOT NULL,
    [UserId]                 NVARCHAR (128)     NOT NULL,
    [AddressId]              BIGINT             NOT NULL,
    [RentPerMonth]           DECIMAL (18, 2)    NULL,
    [CurrencyId]             NCHAR (3)          NULL,
    [NumberOfBedrooms]       TINYINT            NULL,
    [IsPartOfProperty]       BIT                NULL,
	[IsFurnished]            BIT                NULL,
	[LandlordRating]           TINYINT          NULL,
	[PropertyConditionRating]  TINYINT          NULL,
	[NeighboursRating]         TINYINT          NULL,
    [CreatedOn]              DATETIMEOFFSET (7) NOT NULL,
    [SubmittedOn]            DATETIMEOFFSET (7) NULL,
    [CreatedByIpAddress]     NVARCHAR (39)      NOT NULL,
	[IsHidden]               BIT                NULL,
    [Timestamp]              ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.TenancyDetailsSubmission] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.TenancyDetailsSubmission_dbo.Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Address] ([Id]),
    CONSTRAINT [FK_dbo.TenancyDetailsSubmission_dbo.Currency_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [dbo].[Currency] ([Id]),
    CONSTRAINT [FK_dbo.TenancyDetailsSubmission_dbo.User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
);




























GO
CREATE NONCLUSTERED INDEX [IX_AddressId]
    ON [dbo].[TenancyDetailsSubmission]([AddressId] ASC);


GO



GO
CREATE NONCLUSTERED INDEX [IX_CurrencyId]
    ON [dbo].[TenancyDetailsSubmission]([CurrencyId] ASC);


GO



GO



GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TenancyDetailsSubmission_UniqueId]
    ON [dbo].[TenancyDetailsSubmission]([UniqueId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenancyDetailsSubmission_CreatedOn_UserId]
    ON [dbo].[TenancyDetailsSubmission]([CreatedOn] ASC, [UserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenancyDetailsSubmission_CreatedOn_CreatedByIpAddress]
    ON [dbo].[TenancyDetailsSubmission]([CreatedOn] ASC, [CreatedByIpAddress] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenancyDetailsSubmission_UniqueId_UserId]
    ON [dbo].[TenancyDetailsSubmission]([UniqueId] ASC, [UserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenancyDetailsSubmission_UserId_CreatedByIpAddress]
    ON [dbo].[TenancyDetailsSubmission]([UserId] ASC, [CreatedByIpAddress] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenancyDetailsSubmission_CreatedOn]
    ON [dbo].[TenancyDetailsSubmission]([CreatedOn] ASC);

