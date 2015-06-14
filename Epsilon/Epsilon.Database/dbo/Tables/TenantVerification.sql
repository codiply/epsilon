CREATE TABLE [dbo].[TenantVerification] (
    [Id]                         UNIQUEIDENTIFIER   DEFAULT (newsequentialid()) NOT NULL,
    [TenancyDetailsSubmissionId] UNIQUEIDENTIFIER   NOT NULL,
    [Code]                       NVARCHAR (16)      NULL,
    [CreatedOn]                  DATETIMEOFFSET (7) NOT NULL,
    [VerifiedOn]                 DATETIMEOFFSET (7) NULL,
    [Timestamp]                  ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.TenantVerification] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.TenantVerification_dbo.TenancyDetailsSubmission_TenancyDetailsSubmissionId] FOREIGN KEY ([TenancyDetailsSubmissionId]) REFERENCES [dbo].[TenancyDetailsSubmission] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_TenancyDetailsSubmissionId]
    ON [dbo].[TenantVerification]([TenancyDetailsSubmissionId] ASC);

