CREATE TABLE [dbo].[Usuarios] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [NombreUsuario]  NVARCHAR (50)  NOT NULL,
    [PasswordHash]   NVARCHAR (MAX) NOT NULL,
    [NombreCompleto] NVARCHAR (150) NULL,
    [Rol]            INT            NOT NULL,
    [Activo]         BIT            NOT NULL,
    [SucursalId]     INT            NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Usuarios_Sucursales_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [dbo].[Sucursales] ([Id]) ON DELETE SET NULL
);

GO
CREATE UNIQUE INDEX [IX_Usuarios_NombreUsuario] ON [dbo].[Usuarios] ([NombreUsuario] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Usuarios_SucursalId] ON [dbo].[Usuarios] ([SucursalId] ASC);
