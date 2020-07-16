USE [RandomCode]
GO
CREATE TABLE [dbo].[Codes](
    ID int PRIMARY KEY IDENTITY(1,1) NOT NULL,
    SeedValue int UNIQUE NOT NULL,
    BatchID UNIQUEIDENTIFIER NOT NULL,
    [State] tinyint NOT NULL,
    RedemptionID UNIQUEIDENTIFIER NULL,
    RedeemedBy NVARCHAR(50) NULL,
    RedeemedOn DATETIME NULL,
    DeactivatedBy NVARCHAR(50) NULL,
    DeactivatedOn DATETIME NULL,
    FOREIGN KEY(BatchID) REFERENCES Batch(ID))
   
   