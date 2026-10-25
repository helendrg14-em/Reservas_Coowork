-- SOLO para una base VACÍA seleccionada explícitamente en SSMS.
-- No ejecutar sobre la base de datos existente del equipo.
-- No contiene DROP, ALTER, datos de usuarios ni contraseñas.
SET XACT_ABORT ON;
IF DB_NAME() IN ('master', 'model', 'msdb', 'tempdb')
    THROW 50000, 'Seleccione una base de usuario vacía.', 1;
IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
    THROW 50001, 'La base contiene tablas. No se realizó ningún cambio.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
CREATE TABLE [EQUIPAMIENTO] (
    [id_equipo] int NOT NULL IDENTITY,
    [codigo] nvarchar(20) NOT NULL,
    [nombre] nvarchar(100) NOT NULL,
    [tipo] nvarchar(50) NOT NULL,
    [precio_hora] decimal(10,2) NOT NULL,
    CONSTRAINT [PK_EQUIPAMIENTO] PRIMARY KEY ([id_equipo])
);

CREATE TABLE [ESPACIO] (
    [id_espacio] int NOT NULL IDENTITY,
    [codigo] nvarchar(20) NOT NULL,
    [nombre] nvarchar(100) NOT NULL,
    [tipo] int NOT NULL,
    [capacidad] int NOT NULL,
    [precio_hora] decimal(10,2) NOT NULL,
    CONSTRAINT [PK_ESPACIO] PRIMARY KEY ([id_espacio])
);

CREATE TABLE [USUARIO] (
    [id_usuario] int NOT NULL IDENTITY,
    [nombre] nvarchar(100) NOT NULL,
    [correo] nvarchar(150) NOT NULL,
    [telefono] nvarchar(20) NOT NULL,
    [password_hash] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_USUARIO] PRIMARY KEY ([id_usuario])
);

CREATE TABLE [RESERVA] (
    [id_reserva] int NOT NULL IDENTITY,
    [folio] nvarchar(30) NOT NULL,
    [fecha_inicio] datetime2 NOT NULL,
    [fecha_fin] datetime2 NOT NULL,
    [fecha_creacion] datetime2 NOT NULL,
    [estado] int NOT NULL,
    [costo_total] decimal(10,2) NOT NULL,
    [id_usuario] int NOT NULL,
    [id_espacio] int NOT NULL,
    CONSTRAINT [PK_RESERVA] PRIMARY KEY ([id_reserva]),
    CONSTRAINT [FK_RESERVA_ESPACIO_id_espacio] FOREIGN KEY ([id_espacio]) REFERENCES [ESPACIO] ([id_espacio]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RESERVA_USUARIO_id_usuario] FOREIGN KEY ([id_usuario]) REFERENCES [USUARIO] ([id_usuario]) ON DELETE NO ACTION
);

CREATE TABLE [INCLUYE] (
    [id_reserva] int NOT NULL,
    [id_equipo] int NOT NULL,
    [cantidad] int NOT NULL,
    [precio_hora] decimal(10,2) NOT NULL,
    [fecha_hora] datetime2 NOT NULL,
    CONSTRAINT [PK_INCLUYE] PRIMARY KEY ([id_reserva], [id_equipo]),
    CONSTRAINT [FK_INCLUYE_EQUIPAMIENTO_id_equipo] FOREIGN KEY ([id_equipo]) REFERENCES [EQUIPAMIENTO] ([id_equipo]) ON DELETE NO ACTION,
    CONSTRAINT [FK_INCLUYE_RESERVA_id_reserva] FOREIGN KEY ([id_reserva]) REFERENCES [RESERVA] ([id_reserva]) ON DELETE CASCADE
);

CREATE TABLE [PAGO] (
    [id_pago] int NOT NULL IDENTITY,
    [codigo_transaccion] nvarchar(40) NOT NULL,
    [id_reserva] int NOT NULL,
    [monto] decimal(10,2) NOT NULL,
    [metodo_pago] int NOT NULL,
    [estado] int NOT NULL,
    [fecha_pago] datetime2 NOT NULL,
    CONSTRAINT [PK_PAGO] PRIMARY KEY ([id_pago]),
    CONSTRAINT [FK_PAGO_RESERVA_id_reserva] FOREIGN KEY ([id_reserva]) REFERENCES [RESERVA] ([id_reserva]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_EQUIPAMIENTO_codigo] ON [EQUIPAMIENTO] ([codigo]);

CREATE UNIQUE INDEX [IX_ESPACIO_codigo] ON [ESPACIO] ([codigo]);

CREATE INDEX [IX_INCLUYE_id_equipo] ON [INCLUYE] ([id_equipo]);

CREATE UNIQUE INDEX [IX_PAGO_codigo_transaccion] ON [PAGO] ([codigo_transaccion]);

CREATE UNIQUE INDEX [IX_PAGO_id_reserva] ON [PAGO] ([id_reserva]);

CREATE UNIQUE INDEX [IX_RESERVA_folio] ON [RESERVA] ([folio]);

CREATE INDEX [IX_RESERVA_id_espacio_fecha_inicio_fecha_fin] ON [RESERVA] ([id_espacio], [fecha_inicio], [fecha_fin]);

CREATE INDEX [IX_RESERVA_id_usuario] ON [RESERVA] ([id_usuario]);

CREATE UNIQUE INDEX [IX_USUARIO_correo] ON [USUARIO] ([correo]);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
