USE [RandomCode]
GO
CREATE TABLE [dbo].[Batch](
    [ID] UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
	[BatchName] varchar(50) NOT NULL,
    [BatchSize] int NOT NULL,
	[DateActive] DATETIME NOT NULL,
	[DateExpires] DATETIME NOT NULL)