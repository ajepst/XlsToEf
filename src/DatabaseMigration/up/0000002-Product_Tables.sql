SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[ProductCategories](
	[Id] int IDENTITY(1,1) PRIMARY KEY,
	[CategoryCode] [nvarchar] (20) NOT NULL,
	[CategoryName] [nvarchar] (100) NULL 
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[Products](
	[Id] int IDENTITY(1,1) PRIMARY KEY,
	[ProductCategoryId] [int] NOT NULL,
	[ProductName] [nvarchar] (100) NULL,
	CONSTRAINT FK_Products_ProductCategory_Id FOREIGN KEY ([ProductCategoryId])
	REFERENCES ProductCategories (ID)
) ON [PRIMARY]

GO
