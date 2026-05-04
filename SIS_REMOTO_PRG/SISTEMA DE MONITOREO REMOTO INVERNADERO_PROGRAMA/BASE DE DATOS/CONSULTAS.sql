SELECT * FROM REGISTROS;


-- 1. Alertas últimas 24 horas
SELECT * 
FROM ALERTAS 
WHERE fecha_hora >= NOW() - INTERVAL 1 DAY
ORDER BY fecha_hora DESC;

-- 2. Temperatura > 30 °C
SELECT R.*, S.nombre 
FROM REGISTROS R
JOIN SENSORES S ON R.sensor_id = S.id
WHERE S.tipo = 'Temperatura' AND R.valor > 30
ORDER BY R.fecha_hora DESC;

-- 3. Histeresis Inferior crítica
SHOW DATABASES;

-- Cambia el conjunto de caracteres predeterminado de la base de datos
ALTER DATABASE bd_invernadero CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;



SELECT R.*, S.nombre 
FROM REGISTROS R
JOIN SENSORES S ON R.sensor_id = S.id
WHERE S.nombre = 'Histeresis Inferior' AND R.valor BETWEEN 72.3 AND 74.3
ORDER BY R.fecha_hora DESC;

-- 4. Alertas críticas
SELECT A.*, S.nombre AS nombre_sensor 
FROM ALERTAS A
LEFT JOIN SENSORES S ON A.sensor_id = S.id
WHERE A.nivel = 'Crítico'
ORDER BY A.fecha_hora DESC;

-- 5. Promedio de humedad en el último día
SELECT S.nombre, AVG(R.valor) AS promedio_humedad
FROM REGISTROS R
JOIN SENSORES S ON R.sensor_id = S.id
WHERE S.tipo = 'Humedad' AND R.fecha_hora >= NOW() - INTERVAL 1 DAY
GROUP BY S.nombre;

-- 6. Registros de un sensor específico
SELECT R.* 
FROM REGISTROS R
JOIN SENSORES S ON R.sensor_id = S.id
WHERE S.nombre = 'Humedad promedio 1'
ORDER BY R.fecha_hora DESC;

-- 7. Cantidad de alertas por nivel
SELECT nivel, COUNT(*) AS cantidad
FROM ALERTAS
GROUP BY nivel;

USE BD_INVERNADERO;
-- Fuerza un registro de temperatura crítica
INSERT INTO REGISTROS (fuente, nombre_registro, direccion_plc, sensor_id, valor, unidad, fecha_hora)
VALUES ('SENSOR', 'TempAlarma', NULL, 1, 35.0, '°C', NOW());

-- Fuerza un registro de histeresis baja
INSERT INTO REGISTROS (fuente, nombre_registro, direccion_plc, sensor_id, valor, unidad, fecha_hora)
VALUES ('SENSOR', 'HisteresisBaja', NULL, 10, 73.0, '', NOW());
