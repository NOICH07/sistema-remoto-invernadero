using System;
using System.IO;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ShellProgressBar;
using BD.Common.Configuration;

namespace BD.Features.Processing
{
    /// <summary>
    /// Clase encargada de procesar datos de sensores y almacenarlos en la base de datos
    /// </summary>
    public class DataProcessor
    {
        private readonly string _connectionString;
        private readonly ProgressBarOptions _progressBarOptions;

        /// <summary>
        /// Constructor que inicializa el procesador con la cadena de conexión a la base de datos
        /// </summary>
        public DataProcessor(string connectionString)
        {
            _connectionString = connectionString;
            // Configura las opciones visuales de la barra de progreso
            _progressBarOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Blue,
                BackgroundColor = ConsoleColor.DarkBlue,
                ProgressCharacter = '─'
            };
        }

        /// <summary>
        /// Procesa todos los archivos de datos de los diferentes sensores
        /// </summary>
        public async Task ProcessAllFiles()
        {
            // Lista de configuración para cada tipo de archivo a procesar
            // Incluye: ruta, nombre, tipo de registro, dirección PLC, ID del sensor y unidad de medida
            var archivosAProcesar = new List<(string ruta, string nombre, string registro, string plc, int sensorId, string unidad)>
            {
                // Configuración para temperatura
                (AppSettings.DownloadPath + @"\TEMPERATURA", "TEMPERATURA", "Temperatura", "D20030", 1, "°C"),
                // Configuración para promedio de humedad 1-2
                (AppSettings.DownloadPath + @"\PROMEDIO HUMEDAD1-2", "PROMEDIO HUMEDAD1-2", "Humedad RNA", "D20014", 2, "REAL"),
                // Configuración para promedio de humedad 3-4
                (AppSettings.DownloadPath + @"\PROMEDIO DE HUMEDAD3-4", "PROMEDIO HUMEDAD3-4", "Humedad COF", "D20012", 3, "REAL"),
                // Configuración para encendido
                (AppSettings.DownloadPath + @"\ENCENDIDO", "ENCENDIDO", "Encendido", "M6000", 4, "ESTADO"),
                // Configuración para consumo RNA
                (AppSettings.DownloadPath + @"\CONSUMO RNA", "CONSUMO RNA", "Control RNA", "M6001", 6, "ESTADO"),
                // Configuración para consumo ON/OFF
                (AppSettings.DownloadPath + @"\CONSUMO_ONOFF", "CONSUMO_ONOFF", "Control ON/OFF", "M6002", 7, "ESTADO"),
                // Configuración para humedad ambiente
                (AppSettings.DownloadPath + @"\HUMEDAD AMBIENTE", "HUMEDAD AMBIENTE", "Humedad Ambiente", "D20026", 8, "CONSUMO"),
                // Configuración para histéresis superior
                (AppSettings.DownloadPath + @"\HISTERESIS_SUPERIOR", "HISTERESIS_SUPERIOR", "Histeresis Superior", "D20032", 9, "HISTÉRESIS"), 
                // Configuración para histéresis inferior
                (AppSettings.DownloadPath + @"\HISTERESIS_INFERIOR", "HISTERESIS_INFERIOR", "Histeresis Inferior", "D20034", 10, "HISTÉRESIS")
            };

            // Procesa cada archivo configurado
            foreach (var archivo in archivosAProcesar)
            {
                Console.WriteLine($"\nProcesando {archivo.nombre}...");
                await ProcessFileWithProgress(archivo.ruta, archivo.nombre, archivo.registro, archivo.plc, archivo.sensorId, archivo.unidad);
            }
        }

        /// <summary>
        /// Procesa un archivo específico mostrando una barra de progreso
        /// </summary>
        private async Task ProcessFileWithProgress(string folderPath, string archivoNombre, string nombreRegistro, string direccionPlc, int sensorId, string unidad)
        {
            try
            {
                // Obtiene todos los archivos CSV ordenados por fecha de modificación
                var archivos = Directory.GetFiles(folderPath, "*.csv")
                                      .OrderByDescending(f => new FileInfo(f).LastWriteTime);

                if (!archivos.Any())
                {
                    Console.WriteLine($"No se encontraron archivos CSV en {folderPath}");
                    return;
                }

                // Procesa el archivo más reciente
                string ultimoArchivo = archivos.First();
                string[] lineas = await File.ReadAllLinesAsync(ultimoArchivo);

                // Crea una barra de progreso para mostrar el avance del procesamiento
                using (var progressBar = new ProgressBar(lineas.Length - 1, $"Procesando {archivoNombre}", _progressBarOptions))
                {
                    using (MySqlConnection conn = new MySqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        
                        // Obtiene la fecha del último registro para evitar duplicados
                        DateTime? ultimaFecha = await GetLastRecordDate(conn, nombreRegistro);

                        int registrosProcesados = 0;
                        int registrosExistentes = 0;
                        
                        // Procesa cada línea del archivo (saltando la cabecera)
                        foreach (var linea in lineas.Skip(1))
                        {
                            var resultado = await ProcessLine(conn, linea, nombreRegistro, direccionPlc, sensorId, unidad, ultimaFecha);
                            if (resultado.registroNuevo)
                            {
                                registrosProcesados++;
                            }
                            else if (resultado.registroExistente)
                            {
                                registrosExistentes++;
                            }
                            progressBar.Tick($"Nuevos: {registrosProcesados} | Existentes: {registrosExistentes}");
                        }

                        // Muestra el resumen del procesamiento
                        Console.WriteLine($"\nResumen de {archivoNombre}:");
                        Console.WriteLine($"- Registros nuevos insertados: {registrosProcesados}");
                        Console.WriteLine($"- Registros existentes omitidos: {registrosExistentes}");
                        Console.WriteLine($"- Total procesado: {registrosProcesados + registrosExistentes}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar {archivoNombre}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene la fecha del último registro almacenado para un tipo de registro específico
        /// </summary>
        private async Task<DateTime?> GetLastRecordDate(MySqlConnection conn, string nombreRegistro)
        {
            string lastDateQuery = @"SELECT MAX(fecha_hora) FROM registros WHERE nombre_registro = @nombre_registro";
            using (var cmd = new MySqlCommand(lastDateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@nombre_registro", nombreRegistro);
                var result = await cmd.ExecuteScalarAsync();
                return result as DateTime?;
            }
        }

        /// <summary>
        /// Procesa una línea de datos y la inserta en la base de datos si es un registro nuevo
        /// </summary>
        private async Task<(bool registroNuevo, bool registroExistente)> ProcessLine(MySqlConnection conn, string linea, string nombreRegistro, string direccionPlc, int sensorId, string unidad, DateTime? ultimaFecha)
        {
            // Parsea los datos de la línea
            string[] datos = linea.Split(',');
            if (!DateTime.TryParse(datos[0], out DateTime fechaHora) || 
                !decimal.TryParse(datos[1], out decimal valor))
            {
                return (false, false); // Datos inválidos
            }

            // Verifica si el registro ya existe
            if (ultimaFecha.HasValue && fechaHora <= ultimaFecha.Value)
            {
                return (false, true);
            }

            // Query para insertar el nuevo registro
            string insertQuery = @"INSERT INTO registros 
                (fuente, nombre_registro, direccion_plc, sensor_id, valor, unidad, fecha_hora, fecha_hora_recuperacion, tipo_acceso) 
                VALUES 
                (@fuente, @nombre_registro, @direccion_plc, @sensor_id, @valor, @unidad, @fecha_hora, NOW(), 'READ')";

            // Ejecuta la inserción con los parámetros correspondientes
            using (var cmd = new MySqlCommand(insertQuery, conn))
            {
                cmd.Parameters.AddWithValue("@fuente", "PLC");
                cmd.Parameters.AddWithValue("@nombre_registro", nombreRegistro);
                cmd.Parameters.AddWithValue("@direccion_plc", direccionPlc);
                cmd.Parameters.AddWithValue("@sensor_id", sensorId);
                cmd.Parameters.AddWithValue("@valor", valor);
                cmd.Parameters.AddWithValue("@unidad", unidad);
                cmd.Parameters.AddWithValue("@fecha_hora", fechaHora);

                await cmd.ExecuteNonQueryAsync();
                return (true, false); // Registro insertado exitosamente
            }
        }
        }
    }
