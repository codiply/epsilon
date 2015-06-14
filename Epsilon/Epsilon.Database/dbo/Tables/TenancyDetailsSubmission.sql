CREATE TABLE [dbo].[TenancyDetailsSubmission] (
    [Id]                    UNIQUEIDENTIFIER   DEFAULT (newsequentialid()) NOT NULL,
    [UserId]                NVARCHAR (128)     NOT NULL,
    [AddressId]             NVARCHAR (128)     NOT NULL,
    [Rent]                  DECIMAL (18, 2)    NOT NULL,
    [BedroomNumber]         INT                NULL,
    [RentingPartOfProperty] BIT                NULL,
    [CreatedOn]             DATETIMEOFFSET (7) NOT NULL,
    [SubmittedOn]           DATETIMEOFFSET (7) NULL,
    [Timestamp]             ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.TenancyDetailsSubmission] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.TenancyDetailsSubmission_dbo.Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Address] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.TenancyDetailsSubmission_dbo.User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_AddressId]
    ON [dbo].[TenancyDetailsSubmission]([AddressId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[TenancyDetailsSubmission]([UserId] ASC);

