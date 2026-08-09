CREATE TABLE [dbo].[DetallesCotizacion] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [Cantidad]       INT             NOT NULL,
    [PrecioUnitario] DECIMAL (18, 2) NOT NULL,
    [CotizacionId]   INT             NOT NULL,
    [ProductoId]     INT             NOT NULL,
    CONSTRAINT [PK_DetallesCotizacion] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DetallesCotizacion_Cotizaciones_CotizacionId] FOREIGN KEY ([CotizacionId]) REFERENCES [dbo].[Cotizaciones] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DetallesCotizacion_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [dbo].[Productos] ([Id])
);

GO
CREATE NONCLUSTERED INDEX [IX_DetallesCotizacion_CotizacionId] ON [dbo].[DetallesCotizacion] ([CotizacionId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_DetallesCotizacion_ProductoId] ON [dbo].[DetallesCotizacion] ([ProductoId] ASC);
