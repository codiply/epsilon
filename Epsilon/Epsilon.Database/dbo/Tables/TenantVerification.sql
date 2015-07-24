CREATE TABLE [dbo].[TenantVerification] (
    [Id]                         BIGINT             IDENTITY (1, 1) NOT NULL,
    [UniqueId]                   UNIQUEIDENTIFIER   NOT NULL,
    [TenancyDetailsSubmissionId] BIGINT             NOT NULL,
    [Code]                       NVARCHAR (16)      NULL,
    [CreatedOn]                  DATETIMEOFFSET (7) NOT NULL,
    [SentOn]                     DATETIMEOFFSET (7) NULL,
    [VerifiedOn]                 DATETIMEOFFSET (7) NULL,
    [CreatedByIpAddress]         NVARCHAR (39)      NULL,
    [Timestamp]                  ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.TenantVerification] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.TenantVerification_dbo.TenancyDetailsSubmission_TenancyDetailsSubmissionId] FOREIGN KEY ([TenancyDetailsSubmissionId]) REFERENCES [dbo].[TenancyDetailsSubmission] ([Id]) ON DELETE CASCADE
);








GO
CREATE NONCLUSTERED INDEX [IX_TenancyDetailsSubmissionId]
    ON [dbo].[TenantVerification]([TenancyDetailsSubmissionId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TenantVerification_UniqueId]
    ON [dbo].[TenantVerification]([UniqueId] ASC);

