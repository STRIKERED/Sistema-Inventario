CREATE TABLE [dbo].[MovimientosInventario] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [TipoMovimiento] INT            NOT NULL,
    [Cantidad]       INT            NOT NULL,
    [Motivo]         NVARCHAR (300) NULL,
    [Fecha]          DATETIME2 (7)  NOT NULL,
    [ProductoId]     INT            NOT NULL,
    [SucursalId]     INT            NOT NULL,
    [UsuarioId]      INT            NULL,
    CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MovimientosInventario_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [dbo].[Productos] ([Id]),
    CONSTRAINT [FK_MovimientosInventario_Sucursales_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [dbo].[Sucursales] ([Id]),
    CONSTRAINT [FK_MovimientosInventario_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuarios] ([Id]) ON DELETE SET NULL
);

GO
CREATE NONCLUSTERED INDEX [IX_MovimientosInventario_ProductoId] ON [dbo].[MovimientosInventario] ([ProductoId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_MovimientosInventario_SucursalId] ON [dbo].[MovimientosInventario] ([SucursalId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_MovimientosInventario_UsuarioId] ON [dbo].[MovimientosInventario] ([UsuarioId] ASC);
