CREATE TABLE [dbo].[Sucursales] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Nombre]    NVARCHAR (150) NOT NULL,
    [Direccion] NVARCHAR (300) NULL,
    CONSTRAINT [PK_Sucursales] PRIMARY KEY CLUSTERED ([Id] ASC)
);
