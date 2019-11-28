USE [Random-Code]
GO

/****** Object:  Table [dbo].[Offset]    Script Date: 11/22/2019 8:42:11 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Offset](
	[ID] [int] NOT NULL,
	[OffsetValue] [bigint] NOT NULL,
 CONSTRAINT [PK_Offset] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

