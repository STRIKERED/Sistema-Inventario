CREATE TABLE [dbo].[Productos] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [Sku]          NVARCHAR (50)   NOT NULL,
    [CodigoBarras] NVARCHAR (50)   NOT NULL,
    [Nombre]       NVARCHAR (200)  NOT NULL,
    [Categoria]    NVARCHAR (100)  NULL,
    [Unidad]       NVARCHAR (20)   NULL,
    [PrecioCosto]  DECIMAL (18, 2) NOT NULL,
    [PrecioVenta]  DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE UNIQUE INDEX [IX_Productos_Sku] ON [dbo].[Productos] ([Sku] ASC);

GO
CREATE UNIQUE INDEX [IX_Productos_CodigoBarras] ON [dbo].[Productos] ([CodigoBarras] ASC);
