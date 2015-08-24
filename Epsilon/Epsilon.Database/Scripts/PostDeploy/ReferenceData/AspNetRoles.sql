-- PostDeploy\ReferenceData\AspNetRoles.sql START

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

-- I copy the schema of the source table into #TMP.
SELECT * INTO #TMP
FROM [dbo].[AspNetRoles]
WHERE 1 = 0

INSERT INTO #TMP
([Id], [Name])
VALUES
-- Edit the values below to update the target table.
(N'Admin', N'Admin'),
(N'Translator', N'Translator');
GO

MERGE [dbo].[AspNetRoles] AS T -- Target
USING #TMP AS S -- Source
    ON T.Id = S.Id
WHEN MATCHED
    THEN UPDATE SET
	    T.[Name] = S.[Name]
WHEN NOT MATCHED
    THEN INSERT ([Id], [Name])
	VALUES (S.[Id], S.[Name])
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;
GO

DROP TABLE #TMP;
GO

-- PostDeploy\ReferenceData\AspNetRoles.sql END