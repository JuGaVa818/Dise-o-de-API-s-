BEGIN;

INSERT INTO "PRQ_Automoviles" (id, color, anio, fabricante, tipo) VALUES
    (1, 'Blanco', 2020, 'Toyota', 'sedán'),
    (2, 'Gris', 2022, 'Suzuki', '4x4'),
    (3, 'Azul', 2021, 'Yamaha', 'moto'),
    (4, 'Rojo', 2019, 'Hyundai', 'sedán'),
    (5, 'Negro', 2023, 'Toyota', '4x4')
ON CONFLICT (id) DO UPDATE SET
    color = EXCLUDED.color,
    anio = EXCLUDED.anio,
    fabricante = EXCLUDED.fabricante,
    tipo = EXCLUDED.tipo;

INSERT INTO "PRQ_Parqueo" (id, nombre_provincia, nombre, precio_por_hora) VALUES
    (1, 'San José', 'Parqueo Central SJ', 1200.00),
    (2, 'Alajuela', 'Parqueo Central Alajuela', 900.00)
ON CONFLICT (id) DO UPDATE SET
    nombre_provincia = EXCLUDED.nombre_provincia,
    nombre = EXCLUDED.nombre,
    precio_por_hora = EXCLUDED.precio_por_hora;

INSERT INTO "PRQ_Ingreso_Automoviles"
    (consecutivo, id_parqueo, id_automovil, fecha_hora_entrada, fecha_hora_salida)
VALUES
    (1,  1, 1, '2026-10-01 07:15:00', '2026-10-01 10:45:00'),
    (2,  2, 2, '2026-10-01 08:00:00', '2026-10-01 09:30:00'),
    (3,  1, 3, '2026-10-02 09:00:00', '2026-10-02 11:00:00'),
    (4,  2, 4, '2026-10-02 10:00:00', '2026-10-02 13:30:00'),
    (5,  1, 5, '2026-10-03 07:45:00', '2026-10-03 12:15:00'),
    (6,  2, 1, '2026-10-04 14:00:00', '2026-10-04 16:00:00'),
    (7,  1, 2, '2026-10-05 18:00:00', '2026-10-05 20:00:00'),
    (8,  2, 3, '2026-10-06 11:00:00', '2026-10-06 15:00:00'),
    (9,  1, 4, '2026-10-07 08:30:00', '2026-10-07 12:00:00'),
    (10, 2, 5, '2026-10-08 16:00:00', '2026-10-08 18:30:00'),
    (11, 1, 1, '2026-10-09 07:00:00', '2026-10-09 08:15:00'),
    (12, 2, 2, '2026-10-10 09:30:00', '2026-10-10 12:30:00'),
    (13, 1, 3, '2026-10-23 16:00:00', NULL),
    (14, 2, 4, '2026-10-23 17:00:00', NULL),
    (15, 1, 5, '2026-10-23 18:45:00', NULL)
ON CONFLICT (consecutivo) DO UPDATE SET
    id_parqueo = EXCLUDED.id_parqueo,
    id_automovil = EXCLUDED.id_automovil,
    fecha_hora_entrada = EXCLUDED.fecha_hora_entrada,
    fecha_hora_salida = EXCLUDED.fecha_hora_salida;

SELECT setval(
    pg_get_serial_sequence('"PRQ_Automoviles"', 'id'),
    (SELECT MAX(id) FROM "PRQ_Automoviles"),
    true
);
SELECT setval(
    pg_get_serial_sequence('"PRQ_Parqueo"', 'id'),
    (SELECT MAX(id) FROM "PRQ_Parqueo"),
    true
);
SELECT setval(
    pg_get_serial_sequence('"PRQ_Ingreso_Automoviles"', 'consecutivo'),
    (SELECT MAX(consecutivo) FROM "PRQ_Ingreso_Automoviles"),
    true
);

COMMIT;
