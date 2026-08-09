CREATE TABLE [dbo].[Cajas] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [Nombre]     NVARCHAR (50) NOT NULL,
    [SucursalId] INT           NOT NULL,
    CONSTRAINT [PK_Cajas] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Cajas_Sucursales_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [dbo].[Sucursales] ([Id])
);

GO
CREATE NONCLUSTERED INDEX [IX_Cajas_SucursalId] ON [dbo].[Cajas] ([SucursalId] ASC);
