CREATE TABLE [dbo].[DetallesVenta] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [Cantidad]          INT             NOT NULL,
    [PrecioUnitario]    DECIMAL (18, 2) NOT NULL,
    [DescuentoUnitario] DECIMAL (18, 2) NOT NULL,
    [VentaId]           INT             NOT NULL,
    [ProductoId]        INT             NOT NULL,
    CONSTRAINT [PK_DetallesVenta] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DetallesVenta_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [dbo].[Productos] ([Id]),
    CONSTRAINT [FK_DetallesVenta_Ventas_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [dbo].[Ventas] ([Id]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_DetallesVenta_ProductoId] ON [dbo].[DetallesVenta] ([ProductoId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_DetallesVenta_VentaId] ON [dbo].[DetallesVenta] ([VentaId] ASC);
