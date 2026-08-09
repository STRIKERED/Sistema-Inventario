CREATE TABLE [dbo].[StockPorSucursal] (
    [Id]         INT IDENTITY (1, 1) NOT NULL,
    [Cantidad]   INT NOT NULL,
    [ProductoId] INT NOT NULL,
    [SucursalId] INT NOT NULL,
    CONSTRAINT [PK_StockPorSucursal] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StockPorSucursal_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [dbo].[Productos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_StockPorSucursal_Sucursales_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [dbo].[Sucursales] ([Id]) ON DELETE CASCADE
);

GO
CREATE UNIQUE INDEX [IX_StockPorSucursal_ProductoId_SucursalId] ON [dbo].[StockPorSucursal] ([ProductoId] ASC, [SucursalId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_StockPorSucursal_SucursalId] ON [dbo].[StockPorSucursal] ([SucursalId] ASC);
