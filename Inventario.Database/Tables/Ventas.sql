CREATE TABLE [dbo].[Ventas] (
    [Id]            INT             IDENTITY (1, 1) NOT NULL,
    [Folio]         NVARCHAR (20)   NOT NULL,
    [Fecha]         DATETIME2 (7)   NOT NULL,
    [MetodoPago]    INT             NOT NULL,
    [Subtotal]      DECIMAL (18, 2) NOT NULL,
    [Descuento]     DECIMAL (18, 2) NOT NULL,
    [Impuestos]     DECIMAL (18, 2) NOT NULL,
    [Total]         DECIMAL (18, 2) NOT NULL,
    [SucursalId]    INT             NOT NULL,
    [CorteDeCajaId] INT             NOT NULL,
    [UsuarioId]     INT             NOT NULL,
    CONSTRAINT [PK_Ventas] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Ventas_CortesDeCaja_CorteDeCajaId] FOREIGN KEY ([CorteDeCajaId]) REFERENCES [dbo].[CortesDeCaja] ([Id]),
    CONSTRAINT [FK_Ventas_Sucursales_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [dbo].[Sucursales] ([Id]),
    CONSTRAINT [FK_Ventas_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuarios] ([Id])
);

GO
CREATE UNIQUE INDEX [IX_Ventas_Folio] ON [dbo].[Ventas] ([Folio] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Ventas_CorteDeCajaId] ON [dbo].[Ventas] ([CorteDeCajaId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Ventas_SucursalId] ON [dbo].[Ventas] ([SucursalId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Ventas_UsuarioId] ON [dbo].[Ventas] ([UsuarioId] ASC);
