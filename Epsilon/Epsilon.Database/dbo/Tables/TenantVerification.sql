CREATE TABLE [dbo].[TenantVerification] (
    [Id]                         BIGINT             IDENTITY (1, 1) NOT NULL,
    [UniqueId]                   UNIQUEIDENTIFIER   NOT NULL,
    [TenancyDetailsSubmissionId] BIGINT             NOT NULL,
    [SecretCode]                 NVARCHAR (8)       NOT NULL,
    [CreatedOn]                  DATETIMEOFFSET (7) NOT NULL,
    [MarkedAsSentOn]             DATETIMEOFFSET (7) NULL,
    [VerifiedOn]                 DATETIMEOFFSET (7) NULL,
    [MarkedAddressAsInvalidOn]   DATETIMEOFFSET (7) NULL,
    [SenderRewardedOn]           DATETIMEOFFSET (7) NULL,
    [AssignedToId]               NVARCHAR (128)     NOT NULL,
    [AssignedByIpAddress]        NVARCHAR (39)      NOT NULL,
    [Timestamp]                  ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.TenantVerification] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.TenantVerification_dbo.TenancyDetailsSubmission_TenancyDetailsSubmissionId] FOREIGN KEY ([TenancyDetailsSubmissionId]) REFERENCES [dbo].[TenancyDetailsSubmission] ([Id]),
    CONSTRAINT [FK_dbo.TenantVerification_dbo.User_AssignedToId] FOREIGN KEY ([AssignedToId]) REFERENCES [dbo].[User] ([Id]) ON DELETE CASCADE
);


























GO



GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TenantVerification_UniqueId]
    ON [dbo].[TenantVerification]([UniqueId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenantVerification_UniqueId_AssignedToId]
    ON [dbo].[TenantVerification]([UniqueId] ASC, [AssignedToId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenantVerification_CreatedOn_AssignedToId]
    ON [dbo].[TenantVerification]([CreatedOn] ASC, [AssignedToId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenantVerification_CreatedOn_AssignedByIpAddress]
    ON [dbo].[TenantVerification]([CreatedOn] ASC, [AssignedByIpAddress] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenantVerification_AssignedToId_AssignedByIpAddress]
    ON [dbo].[TenantVerification]([AssignedToId] ASC, [AssignedByIpAddress] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenantVerification_CreatedOn]
    ON [dbo].[TenantVerification]([CreatedOn] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenantVerification_AssignedToId_VerifiedOn]
    ON [dbo].[TenantVerification]([AssignedToId] ASC, [VerifiedOn] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TenantVerification_TenancyDetailsSubmissionId_SecretCode]
    ON [dbo].[TenantVerification]([TenancyDetailsSubmissionId] ASC, [SecretCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenantVerification_TenancyDetailsSubmissionId]
    ON [dbo].[TenantVerification]([TenancyDetailsSubmissionId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TenantVerification_CreatedOn_AssignedToId_VerifiedOn]
    ON [dbo].[TenantVerification]([CreatedOn] ASC, [AssignedToId] ASC, [VerifiedOn] ASC);

