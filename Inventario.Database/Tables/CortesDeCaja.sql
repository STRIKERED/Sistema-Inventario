CREATE TABLE [dbo].[CortesDeCaja] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [MontoInicial]      DECIMAL (18, 2) NOT NULL,
    [MontoFinalContado] DECIMAL (18, 2) NOT NULL,
    [MontoFinalSistema] DECIMAL (18, 2) NOT NULL,
    [Diferencia]        DECIMAL (18, 2) NOT NULL,
    [Estado]            INT             NOT NULL,
    [FechaApertura]     DATETIME2 (7)   NOT NULL,
    [FechaCierre]       DATETIME2 (7)   NULL,
    [CajaId]            INT             NOT NULL,
    [UsuarioId]         INT             NOT NULL,
    CONSTRAINT [PK_CortesDeCaja] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CortesDeCaja_Cajas_CajaId] FOREIGN KEY ([CajaId]) REFERENCES [dbo].[Cajas] ([Id]),
    CONSTRAINT [FK_CortesDeCaja_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuarios] ([Id])
);

GO
CREATE NONCLUSTERED INDEX [IX_CortesDeCaja_CajaId] ON [dbo].[CortesDeCaja] ([CajaId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_CortesDeCaja_UsuarioId] ON [dbo].[CortesDeCaja] ([UsuarioId] ASC);
