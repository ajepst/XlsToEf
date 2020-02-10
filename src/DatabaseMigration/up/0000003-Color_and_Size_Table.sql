SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ColorOptions](
	[Id] int IDENTITY(1,1) PRIMARY KEY,
	[Color] [nvarchar] (100) NULL,
) ON [PRIMARY]

GO


CREATE TABLE [dbo].[SizeOptions](
	[Id] int IDENTITY(1,1) PRIMARY KEY,
	[ProductId] [int] NOT NULL,
	[Size] [nvarchar] (100) NULL,
	CONSTRAINT FK_ProductSizeOptions_Products_Id FOREIGN KEY ([ProductId])
	REFERENCES [dbo].[Products] (ID)
) ON [PRIMARY]

GO

GO