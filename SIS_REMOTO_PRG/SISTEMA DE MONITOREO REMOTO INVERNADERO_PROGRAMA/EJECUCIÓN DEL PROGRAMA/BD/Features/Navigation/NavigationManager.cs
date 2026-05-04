// Importaciones necesarias para el funcionamiento
using OpenQA.Selenium;                // Core de Selenium WebDriver
using OpenQA.Selenium.Support.UI;     // Para esperas explícitas
using OpenQA.Selenium.Interactions;    // Para acciones avanzadas del mouse
using OpenQA.Selenium.Chrome;         // Específico para Chrome
using System;
using System.Threading.Tasks;
using BD.Common.Constants;
using BD.Features.Browser;
using BD.Features.Download;           // Agregar esta línea

namespace BD.Features.Navigation
{
    /// <summary>
    /// Clase que maneja toda la navegación automatizada en la aplicación
    /// </summary>
    public class NavigationManager
    {
        // Variables privadas para el control de la navegación
        private readonly IWebDriver _driver;                    // Driver principal de Selenium
        private readonly WebDriverWait _wait;                   // Utilidad para esperas explícitas
        private readonly Actions _actions;                      // Para acciones complejas del mouse
        private readonly ChromeManager _chromeManager;          // Gestor del navegador Chrome
        private string _originalHandle = string.Empty;          // Identificador de la ventana original
        private readonly DownloadManager _downloadManager; // Agregar esta línea
        
        /// <summary>
        /// Constructor que inicializa el administrador de navegación
        /// </summary>
        /// <param name="driver">Driver de Selenium inicializado</param>
        /// <param name="chromeManager">Gestor de Chrome configurado</param>
        public NavigationManager(IWebDriver driver, ChromeManager chromeManager)
        {
            try
            {
                _driver = driver ?? throw new ArgumentNullException(nameof(driver));
                _chromeManager = chromeManager ?? throw new ArgumentNullException(nameof(chromeManager));
                _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                _actions = new Actions(driver);
                _originalHandle = driver.CurrentWindowHandle;
                _downloadManager = new DownloadManager(driver, chromeManager); // Inicializar DownloadManager
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al inicializar NavigationManager: {ex.Message}");
                throw;
            }
        }

        public async Task NavigatePostLogin(bool isExternalLogin)
        {
            try
            {
                Console.WriteLine("\n=== INICIANDO PROCESO DE POST LOGIN ===");
                await WaitForPageLoad();
                await Task.Delay(3000);

                // Si es login externo, navegamos directamente
                if (isExternalLogin)
                {
                    await NavigateToRegistros();
                    Console.WriteLine("\n=== INICIANDO PROCESO DE DESCARGA ===");
                    return;
                }

                // Para login interno, solo verificamos si estamos en registros
                if (await IsInRegistros())
                {
                    Console.WriteLine("Ya se encuentra en la sección de registros (login interno)");
                    Console.WriteLine("\n=== INICIANDO PROCESO DE DESCARGA ===");
                    return;
                }

                // Si no estamos en registros, intentamos la navegación normal
                Console.WriteLine("No se encuentra en registros, iniciando navegación...");
                await NavigateToRegistros();
                Console.WriteLine("\n=== INICIANDO PROCESO DE DESCARGA ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en navegación post-login: {ex.Message}");
                throw;
            }
        }

        private async Task NavigateToRegistros()
        {
            try
            {
                // 1. Devices
                Console.WriteLine("Buscando opción Devices...");
                var devicesButton = _wait.Until(d => d.FindElement(
                    By.XPath("/html/body/md-content/div/md-sidenav/md-content/ul/li[2]/a")));
                await ClickElement(devicesButton);
                Console.WriteLine("✓ Opción Devices clickeada");
                await Task.Delay(1000); // Reducido de 2000

                // 4. More
                Console.WriteLine("Buscando botón More...");
                var moreButton = _wait.Until(d => d.FindElement(
                    By.XPath("/html/body/md-content/div/div/md-content/div/md-card/md-data-table-container/table/tbody/tr/td[10]/a")));
                await ClickElement(moreButton);
                Console.WriteLine("✓ Botón More clickeado");
                await Task.Delay(1000); // Reducido de 2000

                // 5. Registros
                Console.WriteLine("Buscando pestaña Registros...");
                var registrosTab = _wait.Until(d => d.FindElement(
                    By.XPath("/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-wrapper/md-tabs-canvas/md-pagination-wrapper/md-tab-item[2]")));
                await ClickElement(registrosTab);
                Console.WriteLine("✓ Pestaña Registros clickeada");
                await Task.Delay(2000); // Reducido de 5000
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la navegación a Registros: {ex.Message}");
                throw;
            }
        }

        private async Task ClickElement(IWebElement element)
        {
            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                await Task.Delay(300); // Reducido de 500
                element.Click();
                await Task.Delay(500); // Reducido de 1000
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al hacer clic: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> IsInRegistros()
        {
            try
            {
                await Task.Delay(1000); // Reducido de 2000

                var registrosTab = _wait.Until(d => d.FindElement(By.XPath(
                    "/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-wrapper/md-tabs-canvas/md-pagination-wrapper/md-tab-item[2]")));

                if (registrosTab == null)
                {
                    Console.WriteLine("No se encontró la pestaña Registros");
                    return false;
                }

                // Verificamos si está visible
                bool isVisible = registrosTab.Displayed;

                // Verificamos si está seleccionado de manera segura
                bool isSelected = false;
                var className = registrosTab.GetAttribute("class");
                if (!string.IsNullOrEmpty(className))
                {
                    isSelected = className.Contains("md-active");
                }

                Console.WriteLine($"Estado de pestaña Registros - Visible: {isVisible}, Seleccionada: {isSelected}");

                return isVisible && isSelected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verificando pestaña Registros: {ex.Message}");
                return false;
            }
        }
        public async Task NavigateToUrl(string url)
        {
            try
            {
                _driver.Navigate().GoToUrl(url);
                await WaitForPageLoad();
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al navegar a la URL: {ex.Message}");
                throw;
            }
        }
        public async Task WaitForPageLoad()
        {
            try
            {
                await Task.Delay(2000);
                //crear una referencia explicita al IjavascriptExecutor
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                _wait.Until(d =>{
                    try{
                    var state = js.ExecuteScript("return document.readyState")?.ToString();
                    return !string.IsNullOrEmpty(state) && state.Equals("complete", StringComparison.OrdinalIgnoreCase);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al verificar el estado de carga de la página: {ex.Message}");
                        return false;
                    }
                });
             Console.WriteLine("✓ Página cargada completamente.");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al esperar la carga de la página: {ex.Message}");
                throw;
            }

    }

        private async Task CloseMenu()
        {
            try
            {
                Console.WriteLine("Intentando cerrar menú...");
                await Task.Delay(2000);

                // 1. Primer intento: Buscar y hacer clic en el backdrop
                try
                {
                    var backdrop = _wait.Until(d => d.FindElement(
                        By.CssSelector("md-backdrop._md-sidenav-backdrop")));
                    
                    if (backdrop != null && backdrop.Displayed)
                    {
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", backdrop);
                        await Task.Delay(2000);
                    }
                }
                catch
                {
                    // 2. Segundo intento: Usar Actions para hacer clic fuera
                    try
                    {
                        _actions.MoveByOffset(100, 100).Click().Perform();
                        await Task.Delay(2000);
                    }
                    catch
                    {
                        // 3. Tercer intento: JavaScript directo
                        ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                            var backdrop = document.querySelector('md-backdrop._md-sidenav-backdrop');
                            if(backdrop) {
                                backdrop.click();
                            } else {
                                var content = document.querySelector('md-content > div > div > md-content');
                                if(content) {
                                    content.click();
                                }
                            }
                        ");
                        await Task.Delay(2000);
                    }
                }

                // Verificar si el menú se cerró
                try
                {
                    var backdropAfter = _driver.FindElement(By.CssSelector("md-backdrop._md-sidenav-backdrop"));
                    if (backdropAfter.Displayed)
                    {
                        throw new Exception("El menú sigue abierto después del intento de cierre");
                    }
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("✓ Menú cerrado correctamente");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cerrar menú: {ex.Message}");
                throw;
            }
        }
        public async Task NavigateToHysteresis()
        {
            try 
            {
                Console.WriteLine("Navegando a datos de histéresis...");
                
                // Usar el DownloadManager para las descargas
                await _downloadManager.DescargarHisteresisSup();
                await Task.Delay(2000);
                await _downloadManager.DescargarHisteresisInf();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en descarga de histéresis: {ex.Message}");
                throw;
            }
        }
    }
}




