using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BD.Common.Configuration;

namespace BD.Features.Download
{
    /// <summary>
    /// Clase encargada de gestionar archivos descargados, moverlos a carpetas específicas y realizar limpieza.
    /// </summary>
    public class FileManager
    {
        private readonly string _downloadPath;

        /// <summary>
        /// Constructor que inicializa el gestor de archivos con la ruta de descarga especificada
        /// </summary>
        public FileManager(string downloadPath)
        {
            _downloadPath = downloadPath;
        }

        /// <summary>
        /// Método principal que procesa todos los archivos CSV descargados y los mueve a sus respectivas carpetas
        /// </summary>
        public async Task MoveAllDownloadedFiles()
        {
            try
            {
                // Pausa para asegurar que las descargas se hayan completado
                await Task.Delay(5000);

                // Obtiene todos los archivos CSV ordenados por fecha de modificación
                var directoryInfo = new DirectoryInfo(_downloadPath);
                var archivosCSV = directoryInfo.GetFiles("*.csv")
                                             .OrderByDescending(f => f.LastWriteTime);

                // Diccionario que mapea tipos de archivos con sus carpetas destino y palabras clave
                var mapeoArchivos = new Dictionary<string, (string carpeta, string[] palabrasClave)>
                {
                    { "ENCENDIDO", ("ENCENDIDO", new[] { "ENCENDIDO" }) },
                    { "CONSUMO RNA", ("CONSUMO RNA", new[] { "CONSUMO_RNA", "CONSUMO RNA" }) },
                    { "CONSUMO_ONOFF", ("CONSUMO_ONOFF", new[] { "CONSUMO_ONOFF", "CONSUMO ONOFF" }) },
                    { "PROMEDIO HUMEDAD1-2", ("PROMEDIO HUMEDAD1-2", new[] { "PROMEDIO HUMEDAD 1", "HUMEDAD1-2" }) },
                    { "PROMEDIO DE HUMEDAD3-4", ("PROMEDIO DE HUMEDAD3-4", new[] { "PROMEDIO HUMEDAD 2", "HUMEDAD3-4" }) },
                    { "TEMPERATURA", ("TEMPERATURA", new[] { "TEMPERATURA" }) },
                    { "HUMEDAD AMBIENTE", ("HUMEDAD AMBIENTE", new[] { "HUMEDAD_AMBIENTE", "HUMEDAD AMBIENTE" }) },
                    { "HISTERESIS_SUPERIOR", ("HISTERESIS_SUPERIOR", new[] { "HISTERESIS SUPERIOR" }) },
                    { "HISTERESIS_INFERIOR", ("HISTERESIS_INFERIOR", new[] { "HISTERESIS INFERIOR" }) }
                };

                // Procesa cada archivo encontrado
                foreach (var archivo in archivosCSV)
                {
                    await MoveFile(archivo, mapeoArchivos);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al mover archivos: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Mueve un archivo específico a su carpeta correspondiente según las palabras clave
        /// </summary>
        private async Task MoveFile(FileInfo archivo, Dictionary<string, (string carpeta, string[] palabrasClave)> mapeoArchivos)
        {
            string nombreArchivo = archivo.Name.ToUpper();
            string carpetaDestino = "";

            // Busca la carpeta destino basándose en las palabras clave del nombre del archivo
            foreach (var mapeo in mapeoArchivos)
            {
                if (mapeo.Value.palabrasClave.Any(clave => nombreArchivo.Contains(clave)))
                {
                    carpetaDestino = Path.Combine(_downloadPath, mapeo.Value.carpeta);
                    Console.WriteLine($"Detectado archivo de {mapeo.Key}");
                    break;
                }
            }

            // Mueve el archivo si se encontró una carpeta destino válida
            if (!string.IsNullOrEmpty(carpetaDestino))
            {
                await MoveFileToDestination(archivo, carpetaDestino);
            }
            else
            {
                Console.WriteLine($"No se pudo determinar la carpeta para: {nombreArchivo}");
            }
        }

        /// <summary>
        /// Realiza el movimiento físico del archivo a su destino, manejando casos de archivos existentes
        /// </summary>
        private async Task MoveFileToDestination(FileInfo archivo, string carpetaDestino)
        {
            try
            {
                // Asegura que la carpeta destino exista
                Directory.CreateDirectory(carpetaDestino);
                string rutaDestino = Path.Combine(carpetaDestino, archivo.Name);

                // Elimina el archivo destino si ya existe
                if (File.Exists(rutaDestino))
                {
                    File.Delete(rutaDestino);
                    Console.WriteLine($"Archivo existente eliminado: {rutaDestino}");
                }

                // Verifica que el archivo no esté en uso
                await WaitForFileAvailability(archivo.FullName);

                // Realiza el movimiento del archivo
                File.Move(archivo.FullName, rutaDestino);
                Console.WriteLine($"Archivo movido a: {rutaDestino}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moviendo archivo {archivo.Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Espera hasta que un archivo esté disponible para su uso
        /// </summary>
        private async Task WaitForFileAvailability(string filePath, int maxAttempts = 3)
        {
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                try
                {
                    // Intenta abrir el archivo de forma exclusiva para verificar disponibilidad
                    using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        break;
                    }
                }
                catch (IOException)
                {
                    attempts++;
                    if (attempts == maxAttempts)
                    {
                        throw new IOException($"El archivo {filePath} está en uso después de {maxAttempts} intentos.");
                    }
                    await Task.Delay(1000); // Espera 1 segundo antes del siguiente intento
                }
            }
        }

        /// <summary>
        /// Elimina archivos antiguos que superen cierta cantidad de días
        /// </summary>
        public void CleanupOldFiles(int daysOld = 7)
        {
            try
            {
                // Recorre todas las carpetas dentro de la ruta de descarga
                foreach (var folder in Directory.GetDirectories(_downloadPath))
                {
                    // Obtiene y elimina archivos más antiguos que el número de días especificado
                    var oldFiles = Directory.GetFiles(folder)
                                         .Select(f => new FileInfo(f))
                                         .Where(f => f.LastWriteTime < DateTime.Now.AddDays(-daysOld));

                    foreach (var file in oldFiles)
                    {
                        file.Delete();
                        Console.WriteLine($"Archivo antiguo eliminado: {file.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error limpiando archivos antiguos: {ex.Message}");
            }
        }
    }
}