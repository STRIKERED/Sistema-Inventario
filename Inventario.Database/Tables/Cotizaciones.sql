CREATE TABLE [dbo].[Cotizaciones] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [Folio]           NVARCHAR (20)   NOT NULL,
    [ClienteNombre]   NVARCHAR (200)  NULL,
    [ClienteContacto] NVARCHAR (150)  NULL,
    [Fecha]           DATETIME2 (7)   NOT NULL,
    [FechaVigencia]   DATETIME2 (7)   NULL,
    [Estado]          INT             NOT NULL,
    [Subtotal]        DECIMAL (18, 2) NOT NULL,
    [Descuento]       DECIMAL (18, 2) NOT NULL,
    [Impuestos]       DECIMAL (18, 2) NOT NULL,
    [Total]           DECIMAL (18, 2) NOT NULL,
    [SucursalId]      INT             NOT NULL,
    [UsuarioId]       INT             NOT NULL,
    CONSTRAINT [PK_Cotizaciones] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Cotizaciones_Sucursales_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [dbo].[Sucursales] ([Id]),
    CONSTRAINT [FK_Cotizaciones_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuarios] ([Id])
);

GO
CREATE UNIQUE INDEX [IX_Cotizaciones_Folio] ON [dbo].[Cotizaciones] ([Folio] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Cotizaciones_SucursalId] ON [dbo].[Cotizaciones] ([SucursalId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Cotizaciones_UsuarioId] ON [dbo].[Cotizaciones] ([UsuarioId] ASC);
