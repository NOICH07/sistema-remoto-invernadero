using OpenQA.Selenium;  // Para la automatización web principal
using OpenQA.Selenium.Chrome;  // Específico para Chrome
using OpenQA.Selenium.Support.UI;  // Para esperas explícitas
using OpenQA.Selenium.Interactions;  // Para interacciones avanzadas
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;  // Para manejo de procesos
using System.Threading;    // Para pausas en la ejecución
using BD.Common.Configuration;  // Configuraciones personalizadas

namespace BD.Features.Browser
{
    /// <summary>
    /// Clase que maneja la configuración y ciclo de vida del navegador Chrome
    /// </summary>
    public class ChromeManager
    {
        private readonly ChromeDriverService _driverService;
        private readonly ChromeOptions _options;
        private IWebDriver? _driver;  // Agregar el campo _driver

        public ChromeManager()
        {
            _driverService = ChromeDriverService.CreateDefaultService();
            _driverService.HideCommandPromptWindow = true;

            _options = new ChromeOptions();
            _options.AddArguments(
                "--disable-gpu",
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--remote-debugging-port=9222",
                "--start-maximized",           // Forzar maximizado desde el inicio
                "--disable-extensions",        // Deshabilitar extensiones
                "--disable-popup-blocking",    // Deshabilitar bloqueo de popups
                "--disable-notifications",     // Deshabilitar notificaciones
                "--disable-infobars",          // Deshabilitar infobar
                "--window-size=1920,1080"      // Forzar resolución específica
            );

            // Configurar timeouts y preferencias
            _options.AddUserProfilePreference("download.default_directory", AppSettings.DownloadPath);
            _options.AddUserProfilePreference("download.prompt_for_download", false);
            _options.AddUserProfilePreference("disable-popup-blocking", true);
            _options.PageLoadStrategy = PageLoadStrategy.Normal; // Estrategia de carga más confiable
        }

        /// <summary>
        /// Inicializa y configura una nueva instancia de ChromeDriver
        /// </summary>
        public IWebDriver InitializeDriver()
        {
            try
            {
                Console.WriteLine("Iniciando configuración de ChromeDriver...");
                
                _driver = new ChromeDriver(_driverService, _options, TimeSpan.FromMinutes(3));
                
                // Configurar timeouts explícitos
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                _driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
                _driver.Manage().Window.Maximize();

                // Esperar a que el navegador esté realmente listo
                Thread.Sleep(3000);

                // Verificar el estado de maximizado
                var windowSize = _driver.Manage().Window.Size;
                Console.WriteLine($"Tamaño de ventana: {windowSize.Width}x{windowSize.Height}");

                if (windowSize.Width < 1024 || windowSize.Height < 768)
                {
                    Console.WriteLine("Ajustando tamaño de ventana...");
                    _driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
                    _driver.Manage().Window.Maximize();
                    Thread.Sleep(2000);
                }
                
                Console.WriteLine("✓ ChromeDriver inicializado correctamente en modo maximizado");
                return _driver;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al inicializar ChromeDriver: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cierra y limpia los recursos del ChromeDriver
        /// </summary>
        public void CloseDriver()
        {
            try
            {
                _driver?.Quit();
                _driver?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cerrar ChromeDriver: {ex.Message}");
            }
        }
        /// <summary>
        /// Encuentra un puerto disponible para el ChromeDriver
        /// </summary>
        private int FreeChromeDriverPort()
        {
            // Búsqueda de puerto libre entre 9515-9999
            var random = new Random();
            int port;
            do
            {
                port = random.Next(9515, 10000);
            } while (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Any(x => x.Port == port));

            return port;
        }
    }
}