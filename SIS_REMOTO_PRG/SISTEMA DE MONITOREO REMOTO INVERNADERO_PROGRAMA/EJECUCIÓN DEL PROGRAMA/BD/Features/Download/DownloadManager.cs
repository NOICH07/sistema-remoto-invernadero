// Importaciones necesarias para el funcionamiento
using OpenQA.Selenium;                // Core de Selenium WebDriver
using OpenQA.Selenium.Chrome;         // Específico para Chrome
using OpenQA.Selenium.Support.UI;     // Para esperas explícitas
using OpenQA.Selenium.Interactions;    // Para acciones del mouse
using System;
using System.Threading.Tasks;
using BD.Common.Configuration;
using BD.Common.Constants;
using System.Collections.Generic;
using BD.Features.Authentication;
using BD.Features.Browser; 

namespace BD.Features.Download
{
    /// <summary>
    /// Clase que maneja todas las operaciones de descarga de archivos
    /// </summary>
    public class DownloadManager
    {
        // Variables privadas para el control de descargas
        private readonly IWebDriver _driver;                // Driver principal de Selenium
        private readonly WebDriverWait _wait;              // Utilidad para esperas explícitas
        private readonly LoginManager _loginManager;        // Gestor de autenticación
        private readonly ChromeManager _chromeManager;      // Gestor de Chrome

        /// <summary>
        /// Constructor que inicializa el gestor de descargas
        /// </summary>
        public DownloadManager(IWebDriver driver, ChromeManager chromeManager)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            _loginManager = new LoginManager(driver);
            _chromeManager = chromeManager;
            ConfigurarPreferenciasDescarga();
        }

        /// <summary>
        /// Configura las preferencias de descarga de Chrome
        /// </summary>
        private void ConfigurarPreferenciasDescarga()
        {
            try
            {
                // Diccionario con preferencias de descarga
                var chromePrefs = new Dictionary<string, object>
                {
                    {"download.default_directory", AppSettings.DownloadPath},    // Ruta de descarga
                    {"download.prompt_for_download", false},                     // No mostrar diálogo
                    {"download.directory_upgrade", true},                        // Permitir actualizar directorio
                    {"safebrowsing.enabled", true},                             // Navegación segura
                    {"profile.default_content_setting_values.automatic_downloads", 1} // Permitir descargas automáticas
                };

                // Aplicar preferencias
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddUserProfilePreference("prefs", chromePrefs);

                Console.WriteLine($"✓ Preferencias de descarga configuradas para: {AppSettings.DownloadPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configurando preferencias de descarga: {ex.Message}");
            }
        }

        public async Task DownloadAllFiles()
        {
            try 
            {
                Console.WriteLine("\n=== INICIANDO DESCARGA DE ARCHIVOS ===");

                // Lista de todas las descargas
                var downloads = new List<Func<Task>>
                {
                    DescargarEncendido,
                    DescargarConsumoRNA,
                    DescargarConsumoONOFF,
                    DescargarHumedad12,
                    DescargarHumedad34,
                    DescargarTemperatura,
                    DescargarHumedadAmbiente,
                    DescargarHisteresisSup,
                    DescargarHisteresisInf
                };

                // Ejecutar cada descarga con reintento si falla
                foreach (var download in downloads)
                {
                    var retryCount = 0;
                    const int MAX_RETRIES = 3;

                    while (retryCount < MAX_RETRIES)
                    {
                        try
                        {
                            await download();
                            break;
                        }
                        catch (Exception)
                        {
                            retryCount++;
                            if (retryCount < MAX_RETRIES)
                            {
                                Console.WriteLine($"Reintentando descarga ({retryCount}/{MAX_RETRIES})...");
                                // Intentar reautenticar antes de reintentar
                                await _loginManager.HandleAuthentication();
                                await Task.Delay(2000);
                            }
                            else throw;
                        }
                    }
                }

                Console.WriteLine("✓ Todas las descargas completadas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en proceso de descarga: {ex.Message}");
                throw;
            }
        }

        private async Task DescargarArchivo(string tresPuntosXPath, string historialXPath, string exportarXPath, string cerrarXPath, string nombreArchivo)
        {
            try
            {
                await Task.Delay(2000);
                
                // 1. Clic en tres puntos
                var tresPuntosButton = _wait.Until(d => {
                    var element = d.FindElement(By.XPath(tresPuntosXPath));
                    return element.Displayed ? element : null;
                });
                
                var actions = new Actions(_driver);
                actions.MoveToElement(tresPuntosButton).Click().Perform();
                await Task.Delay(2000);

                // 2. Clic en Historial
                var historialButton = _wait.Until(d => {
                    var element = d.FindElement(By.XPath(historialXPath));
                    return element.Displayed ? element : null;
                });
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", historialButton);
                await Task.Delay(3000);

                // 3. Clic en Exportar
                var exportarButton = _wait.Until(d => {
                    var element = d.FindElement(By.XPath(exportarXPath));
                    return element.Displayed ? element : null;
                });

                // Usar JavaScript para forzar la descarga directa
                ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    arguments[0].setAttribute('download', 'true');
                    arguments[0].click();", exportarButton);

                // Esperar a que se complete la descarga
                await Task.Delay(2000);

                // 4. Cerrar ventana
                var cerrarButton = _wait.Until(d => {
                    var element = d.FindElement(By.XPath(cerrarXPath));
                    return element.Displayed ? element : null;
                });
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", cerrarButton);
                Console.WriteLine($"Ventana de registro de {nombreArchivo} cerrada");
                await Task.Delay(1000);

                Console.WriteLine($"✓ Archivo {nombreArchivo} descargado automáticamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al descargar {nombreArchivo}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DownloadFile()
        {
            try
            {
                // Intentar descarga
                if (!await TryDownload())
                {
                    // Si falla, verificar autenticación
                    Console.WriteLine("Fallo en descarga - Verificando autenticación...");
                    if (await _loginManager.HandleReauthentication())
                    {
                        // Reintentar descarga después de reautenticación
                        Console.WriteLine("Reautenticación exitosa - Reintentando descarga...");
                        return await TryDownload();
                    }
                    else
                    {
                        Console.WriteLine("❌ No se pudo reautenticar - Descarga fallida");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en proceso de descarga: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> TryDownload()
        {
            try
            {
                // Verificar autenticación antes de intentar descargar
                if (!_loginManager.IsAlreadyAuthenticated())  // Quitar el await ya que no es async
                {
                    Console.WriteLine("Se perdió la autenticación - Reautenticando...");
                    if (!await _loginManager.HandleAuthentication())
                    {
                        return false;
                    }
                }

                // Intentar descargar todos los archivos
                await DownloadAllFiles();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en intento de descarga: {ex.Message}");
                return false;
            }
        }

        // Métodos específicos para cada tipo de descarga
        private async Task DescargarEncendido()
        {
            await DescargarArchivo(
                XPaths.Buttons.TresPuntos.Encendido,
                XPaths.Buttons.Historial.Encendido,
                XPaths.Buttons.Exportar.Encendido,
                XPaths.Buttons.Cerrar.Encendido,
                "ENCENDIDO"
            );
        }

        private async Task DescargarConsumoRNA()
        {
            await DescargarArchivo(
                XPaths.Buttons.TresPuntos.ConsumoRNA,
                XPaths.Buttons.Historial.ConsumoRNA,
                XPaths.Buttons.Exportar.ConsumoRNA,
                XPaths.Buttons.Cerrar.ConsumoRNA,
                "CONSUMO RNA"
            );
        }

        private async Task DescargarConsumoONOFF()
        {
            await DescargarArchivo(
                XPaths.Buttons.TresPuntos.ConsumoONOFF,
                XPaths.Buttons.Historial.ConsumoONOFF,
                XPaths.Buttons.Exportar.ConsumoONOFF,
                XPaths.Buttons.Cerrar.ConsumoONOFF,
                "CONSUMO ONOFF"
            );
        }

        private async Task DescargarHumedad12()
        {
            await DescargarArchivo(
                XPaths.Buttons.TresPuntos.Humedad12,
                XPaths.Buttons.Historial.Humedad12,
                XPaths.Buttons.Exportar.Humedad12,
                XPaths.Buttons.Cerrar.Humedad12,
                "HUMEDAD 1-2"
            );
        }

        private async Task DescargarHumedad34()
        {
            await DescargarArchivo(
                XPaths.Buttons.TresPuntos.Humedad34,
                XPaths.Buttons.Historial.Humedad34,
                XPaths.Buttons.Exportar.Humedad34,
                XPaths.Buttons.Cerrar.Humedad34,
                "HUMEDAD 3-4"
            );
        }

        private async Task DescargarTemperatura()
        {
            await DescargarArchivo(
                XPaths.Buttons.TresPuntos.Temperatura,
                XPaths.Buttons.Historial.Temperatura,
                XPaths.Buttons.Exportar.Temperatura,
                XPaths.Buttons.Cerrar.Temperatura,
                "TEMPERATURA"
            );
        }

        private async Task DescargarHumedadAmbiente()
        {
            await DescargarArchivo(
                XPaths.Buttons.TresPuntos.HumedadAmbiente,
                XPaths.Buttons.Historial.HumedadAmbiente,
                XPaths.Buttons.Exportar.HumedadAmbiente,
                XPaths.Buttons.Cerrar.HumedadAmbiente,
                "HUMEDAD AMBIENTE"
            );
        }
        // Cambiar de private a public
        public async Task DescargarHisteresisSup()
        {
            try
            {
                // Navegar a la siguiente página antes de iniciar la descarga
                Console.WriteLine("Navegando a página de histéresis...");
                var nextButton = _wait.Until(d => d.FindElement(By.XPath(XPaths.Navigation.PagSig)));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", nextButton);
                await Task.Delay(2000);

                // Continuar con la descarga normal
                await DescargarArchivo(
                    XPaths.Buttons.TresPuntos.HisteresisSup,
                    XPaths.Buttons.Historial.HisteresisSup,
                    XPaths.Buttons.Exportar.HisteresisSup,
                    XPaths.Buttons.Cerrar.HisteresisSup,
                    "HISTERESIS SUPERIOR"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navegando a histéresis: {ex.Message}");
                throw;
            }
        }
        // Cambiar de private a public
        public async Task DescargarHisteresisInf()
        {
            await DescargarArchivo(
                XPaths.Buttons.TresPuntos.HisteresisInf,
                XPaths.Buttons.Historial.HisteresisInf,
                XPaths.Buttons.Exportar.HisteresisInf,
                XPaths.Buttons.Cerrar.HisteresisInf,
                "HISTERESIS INFERIOR"
            );
        }
        }
        }

