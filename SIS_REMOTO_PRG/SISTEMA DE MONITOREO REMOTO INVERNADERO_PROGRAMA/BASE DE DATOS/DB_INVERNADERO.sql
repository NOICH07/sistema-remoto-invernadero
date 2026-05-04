CREATE DATABASE BD_INVERNADERO;

USE BD_INVERNADERO;


CREATE TABLE SENSORES (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50),
    tipo VARCHAR(50),
    ubicacion VARCHAR(50),
    estado ENUM('ACTIVO', 'INACTIVO') DEFAULT 'ACTIVO'
);

CREATE TABLE REGISTROS (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fuente ENUM('SENSOR', 'PLC') NOT NULL,
    nombre_registro VARCHAR(100),
    direccion_plc VARCHAR(50) NULL,  -- Dirección de memoria del PLC (si aplica)
    sensor_id INT NULL,  -- Se llena solo si el registro proviene de un sensor
    valor DECIMAL(10,2),
    unidad VARCHAR(25),
    fecha_hora DATETIME,
    fecha_hora_recuperacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    tipo_acceso ENUM('READ', 'READ_WRITE') DEFAULT 'READ',  -- Nuevo campo para tipo de acceso
    FOREIGN KEY(sensor_id) REFERENCES SENSORES(id)
);
CREATE TABLE ESTADOS_CONTROL (
    id_estado INT AUTO_INCREMENT PRIMARY KEY,
    nombre_estado VARCHAR(50),
    direccion_plc VARCHAR(10),
    valor FLOAT,
    fecha_hora DATETIME
);
-- Crear tabla de alertas
CREATE TABLE ALERTAS (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sensor_id INT NULL,
    plc_direccion VARCHAR(50) NULL,  -- Dirección del PLC si la alerta proviene de ahí
    mensaje VARCHAR(255),
    nivel ENUM('Información', 'Advertencia', 'Crítico') DEFAULT 'Información',
    fecha_hora DATETIME,
    FOREIGN KEY(sensor_id) REFERENCES SENSORES(id)
);
-- Insertar datos iniciales en SENSORES
INSERT INTO sensores (id, nombre,tipo) VALUES
(1, 'Temperatura','Temperatura'),
(2, 'Humedad promedio 1','Humedad'),
(3, 'Humedad promedio 2','Humedad'),
(4,	'Encendido','Luz piloto'),
(6,'Consumo Rna','//'),
(7,'Consumo ONOFF','//'),
(8, 'Humedad ambiente','Humedad ambiente'),
(9,'Histeresis Superior','Histeresis'),
(10,'Histeresis Inferior','Histeresis');

-- Eliminar el trigger si ya existe
DROP TRIGGER IF EXISTS trigger_alertas_temperatura;

DELIMITER //
CREATE TRIGGER trigger_alertas_temperatura
AFTER INSERT ON REGISTROS
FOR EACH ROW
BEGIN
    IF NEW.unidad = '°C' AND NEW.valor > 30 THEN
        INSERT INTO ALERTAS (sensor_id, plc_direccion, mensaje, nivel, fecha_hora)
        VALUES (NEW.sensor_id, NEW.direccion_plc, 'Temperatura alta detectada', 'Crítico', NEW.fecha_hora);
    END IF;
END;
//
DELIMITER ;
DROP TRIGGER IF EXISTS trigger_alerta_histeresis_inferior;

DELIMITER //
CREATE TRIGGER trigger_alerta_histeresis_inferior
AFTER INSERT ON REGISTROS
FOR EACH ROW
BEGIN
    -- Verifica si el registro pertenece al sensor de Histeresis Inferior
    IF NEW.sensor_id = 10 AND NEW.valor BETWEEN 72.3 AND 74.3 THEN
        INSERT INTO ALERTAS (sensor_id, plc_direccion, mensaje, nivel, fecha_hora)
        VALUES (NEW.sensor_id, NEW.direccion_plc, 'Histeresis inferior por debajo del umbral crítico', 'Advertencia', NEW.fecha_hora);
    END IF;
END;
//
DELIMITER ;

INSERT INTO ESTADOS_CONTROL (nombre_estado, direccion_plc)
VALUES
('Encendido', 'M6000'),
('Control_RNA', 'M6001'),
('Control_ONOFF', 'M6002'),
('Consumo_ONOFF', 'D20026');