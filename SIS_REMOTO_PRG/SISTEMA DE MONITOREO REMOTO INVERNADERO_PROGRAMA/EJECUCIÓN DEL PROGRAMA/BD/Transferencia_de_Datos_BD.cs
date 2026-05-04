// Importaciones necesarias para el funcionamiento del programa
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenQA.Selenium;  // Para automatización web
using BD.Common.Configuration;  // Configuraciones de la aplicación
using BD.Common.Constants;  // Constantes compartidas
using BD.Features.Browser;  // Manejo del navegador Chrome
using BD.Features.Authentication;  // Manejo de autenticación
using BD.Features.Navigation;  // Navegación en la aplicación
using BD.Features.Download;  // Descarga de archivos
using BD.Features.Processing;  // Procesamiento de datos
using OpenQA.Selenium.Chrome;  // Driver específico de Chrome
using OpenQA.Selenium.Support.UI;  // Utilidades de espera de Selenium

namespace BD
{
    class Program
    {
        // Variables estáticas para el control del navegador y la ventana de la consola
        private static IWebDriver? driver;
        private static IntPtr consoleHandle;
        
        static async Task Main()
        {
            try
            {
                // Configuración inicial de la ventana de consola
                consoleHandle = GetConsoleWindow();
                if (consoleHandle == IntPtr.Zero)
                {
                    throw new Exception("No se pudo obtener el handle de la consola");
                }
                ShowWindow(consoleHandle, AppSettings.WindowConfig.SW_MINIMIZE);

                // Inicialización del navegador Chrome
                var chromeManager = new ChromeManager();
                driver = chromeManager.InitializeDriver();
                if (driver == null)
                {
                    throw new Exception("No se pudo inicializar el driver de Chrome");
                }
                driver.Manage().Window.Maximize();

              //Instanciacion de los managers necesarios para el flujo de trabajo
                var navigationManager = new NavigationManager(driver, chromeManager);
                var downloadManager = new DownloadManager(driver, chromeManager);
                var fileManager = new FileManager(AppSettings.DownloadPath);
                var dataProcessor = new DataProcessor(AppSettings.Database.ConnectionString);
                var loginManager = new LoginManager(driver);
                // Proceso principal de la aplicación
                Console.WriteLine("\n=== INICIANDO PROCESO DE NAVEGACIÓN Y AUTENTICACIÓN ===");
                await navigationManager.NavigateToUrl("https://www.diacloudsolutions.com/#/main/devices/76293/1");
                
                // Flujo de autenticación y procesamiento
                if (await loginManager.HandleAuthentication())
                {
                    Console.WriteLine("✓ Autenticación completada");
                    // Post-login: navegación, descarga y procesamiento
                    await navigationManager.NavigatePostLogin(false);
                    await downloadManager.DownloadAllFiles();
                    await fileManager.MoveAllDownloadedFiles();
                    
                    Console.WriteLine("Iniciando procesamiento de datos...");
                    await dataProcessor.ProcessAllFiles();
                    Console.WriteLine("Procesamiento completado.");
                }
                else
                {
                    throw new Exception("❌ No se pudo completar la autenticación");
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores global
                Console.WriteLine($"Error en la aplicación: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Limpieza y cierre de recursos
                if (driver != null) driver.Quit();
                await Task.Delay(5000);
                if (consoleHandle != IntPtr.Zero)
                {
                    ShowWindow(consoleHandle, AppSettings.WindowConfig.SW_HIDE);
                }
            }
        }

        // Importaciones de Windows API para manejo de ventanas
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}