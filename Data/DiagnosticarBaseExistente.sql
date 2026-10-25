-- SOLO LECTURA. Seleccione en SSMS la base existente del equipo.
-- Compara nombres y tipos; revise además tamaños, índices y claves en los resultados.
SELECT DB_NAME() AS BaseSeleccionada;
WITH Esperadas(Tabla, Columna, Tipo) AS (
    SELECT * FROM (VALUES
    (N'EQUIPAMIENTO', N'id_equipo', N'int'),
    (N'EQUIPAMIENTO', N'codigo', N'nvarchar'),
    (N'EQUIPAMIENTO', N'nombre', N'nvarchar'),
    (N'EQUIPAMIENTO', N'tipo', N'nvarchar'),
    (N'EQUIPAMIENTO', N'precio_hora', N'decimal'),
    (N'ESPACIO', N'id_espacio', N'int'),
    (N'ESPACIO', N'codigo', N'nvarchar'),
    (N'ESPACIO', N'nombre', N'nvarchar'),
    (N'ESPACIO', N'tipo', N'int'),
    (N'ESPACIO', N'capacidad', N'int'),
    (N'ESPACIO', N'precio_hora', N'decimal'),
    (N'USUARIO', N'id_usuario', N'int'),
    (N'USUARIO', N'nombre', N'nvarchar'),
    (N'USUARIO', N'correo', N'nvarchar'),
    (N'USUARIO', N'telefono', N'nvarchar'),
    (N'USUARIO', N'password_hash', N'nvarchar'),
    (N'RESERVA', N'id_reserva', N'int'),
    (N'RESERVA', N'folio', N'nvarchar'),
    (N'RESERVA', N'fecha_inicio', N'datetime2'),
    (N'RESERVA', N'fecha_fin', N'datetime2'),
    (N'RESERVA', N'fecha_creacion', N'datetime2'),
    (N'RESERVA', N'estado', N'int'),
    (N'RESERVA', N'costo_total', N'decimal'),
    (N'RESERVA', N'id_usuario', N'int'),
    (N'RESERVA', N'id_espacio', N'int'),
    (N'INCLUYE', N'id_reserva', N'int'),
    (N'INCLUYE', N'id_equipo', N'int'),
    (N'INCLUYE', N'cantidad', N'int'),
    (N'INCLUYE', N'precio_hora', N'decimal'),
    (N'INCLUYE', N'fecha_hora', N'datetime2'),
    (N'PAGO', N'id_pago', N'int'),
    (N'PAGO', N'codigo_transaccion', N'nvarchar'),
    (N'PAGO', N'id_reserva', N'int'),
    (N'PAGO', N'monto', N'decimal'),
    (N'PAGO', N'metodo_pago', N'int'),
    (N'PAGO', N'estado', N'int'),
    (N'PAGO', N'fecha_pago', N'datetime2')
    ) AS V(Tabla, Columna, Tipo)
)
SELECT e.Tabla, e.Columna, e.Tipo AS TipoEsperado, c.DATA_TYPE AS TipoReal,
       c.CHARACTER_MAXIMUM_LENGTH AS Longitud, c.NUMERIC_PRECISION AS Precision,
       c.NUMERIC_SCALE AS Escala, c.IS_NULLABLE AS AdmiteNulos,
       CASE WHEN c.COLUMN_NAME IS NULL THEN 'FALTA'
            WHEN c.DATA_TYPE <> e.Tipo THEN 'TIPO DIFERENTE' ELSE 'NOMBRE Y TIPO OK' END AS Revision
FROM Esperadas e LEFT JOIN INFORMATION_SCHEMA.COLUMNS c
    ON c.TABLE_SCHEMA='dbo' AND c.TABLE_NAME=e.Tabla AND c.COLUMN_NAME=e.Columna
ORDER BY e.Tabla, e.Columna;
SELECT OBJECT_NAME(i.object_id) AS Tabla, i.name AS Indice, i.is_unique AS Unico,
       c.name AS Columna, ic.key_ordinal AS Orden
FROM sys.indexes i JOIN sys.index_columns ic ON i.object_id=ic.object_id AND i.index_id=ic.index_id
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
WHERE OBJECT_NAME(i.object_id) IN ('USUARIO','ESPACIO','RESERVA','PAGO','EQUIPAMIENTO','INCLUYE')
ORDER BY Tabla, Indice, Orden;
SELECT OBJECT_NAME(parent_object_id) AS Tabla, name AS ClaveForanea,
       OBJECT_NAME(referenced_object_id) AS TablaReferenciada, delete_referential_action_desc AS AlEliminar
FROM sys.foreign_keys ORDER BY Tabla, ClaveForanea;
