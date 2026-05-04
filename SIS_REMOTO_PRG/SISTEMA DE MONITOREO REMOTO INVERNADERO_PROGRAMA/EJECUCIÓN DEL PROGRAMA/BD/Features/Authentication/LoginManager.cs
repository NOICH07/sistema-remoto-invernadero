// Importaciones necesarias para Selenium y otras funcionalidades
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System;
using System.IO;
using System.Threading.Tasks;
using BD.Common.Configuration;
using BD.Common.Constants;
using BD.Features.Browser;
using System.Collections.Generic;

namespace BD.Features.Authentication
{
    /// <summary>
    /// Clase que maneja toda la lógica de autenticación en la aplicación
    /// </summary>
    public class LoginManager
    {
        // Variables privadas para el manejo del estado de autenticación
        private readonly IWebDriver _driver;                    // Driver de Selenium
        private readonly WebDriverWait _wait;                   // Utilidad para esperas explícitas
        private bool _isExternalLogin = false;                  // Indica si el login fue externo
        private bool _isAuthenticated = false;                 // Estado de autenticación actual
        private static readonly object _lock = new object();   // Lock para operaciones thread-safe
        private static bool _isLoggedInInternally = false;    // Estado de login interno
        private static bool _isLoggedInExternally = false;    // Estado de login externo
        private bool _hasAttemptedInternalLogin = false;      // Control de intentos de login interno
        private bool _hasAttemptedExternalLogin = false;      // Control de intentos de login externo

        /// <summary>
        /// Constructor que inicializa el manager con el driver de Selenium
        /// </summary>
        public LoginManager(IWebDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            _isExternalLogin = false;
        }

        /// <summary>
        /// Método principal que maneja el proceso de autenticación
        /// Intenta primero login interno, luego externo si es necesario
        /// </summary>
        public async Task<bool> HandleAuthentication()
        {
            try
            {
                Console.WriteLine("Verificando estado inicial de autenticación...");

                if (IsAlreadyAuthenticated())
                {
                    Console.WriteLine("✓ Ya se encuentra autenticado - Continuando");
                    _isAuthenticated = true;
                    _isExternalLogin = false;
                    return true;
                }

                if (await AttemptInternalLogin())
                {
                    _isAuthenticated = true;
                    return true;
                }

                // Si falla el interno, intenta el externo
                if (await AttemptExternalLogin())
                {
                    _isAuthenticated = true;
                    _isExternalLogin = true; // Marcamos que el login fue externo
                    return true;
                }

                Console.WriteLine("❌ No se pudo completar la autenticación");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en autenticación: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Método que maneja el proceso de reautenticación cuando se pierde la sesión
        /// </summary>
        public async Task<bool> HandleReauthentication()
        {
            try
            {
                Console.WriteLine("Verificando si se requiere reautenticación...");
                
                // 1. Verificar si perdimos la autenticación
                if (!IsAlreadyAuthenticated())
                {
                    Console.WriteLine("Se detectó pérdida de sesión - Intentando reautenticación");
                    
                    // 2. Intentar login interno primero
                    if (await AttemptInternalLogin())
                    {
                        Console.WriteLine("✓ Reautenticación interna exitosa");
                        return true;
                    }
                    
                    // 3. Si falla, intentar login externo
                    if (await AttemptExternalLogin())
                    {
                        Console.WriteLine("✓ Reautenticación externa exitosa");
                        return true;
                    }

                    Console.WriteLine("❌ No se pudo completar la reautenticación");
                    return false;
                }

                Console.WriteLine("✓ No se requiere reautenticación");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante reautenticación: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Intenta cerrar el mensaje inicial que aparece en el login externo
        /// </summary>
        private async Task<bool> TryCloseExternalLoginMessage()
        {
            try
            {
                var messageButton = _wait.Until(d => d.FindElement(
                    By.XPath(XPaths.Navigation.CloseInitialMessage)));
                
                if (messageButton != null && messageButton.Displayed)
                {
                    Console.WriteLine("Mensaje detectado - Intentando cerrar...");
                    await ClickElement(messageButton);
                    await Task.Delay(1000);
                    Console.WriteLine("✓ Mensaje cerrado exitosamente");
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Realiza el proceso de login externo:
        /// 1. Cierra mensaje inicial
        /// 2. Ingresa credenciales
        /// 3. Marca "Recuérdame"
        /// 4. Hace clic en login
        /// </summary>
        private async Task<bool> AttemptExternalLogin()
        {
            try
            {
                Console.WriteLine("\n=== INICIANDO LOGIN EXTERNO ===");

                // 1. Cerrar mensaje inicial - CORRECCIÓN
                try
                {
                    await Task.Delay(3000); // Aumentamos el tiempo de espera inicial
                    
                    // Nuevo selector más preciso para el botón de cierre
                    var closeButton = _wait.Until(d => d.FindElement(
                        By.XPath("/html/body/div[2]/md-dialog/md-toolbar/div/button/md-icon")));

                    if (closeButton != null && closeButton.Displayed)
                    {
                        Console.WriteLine("Mensaje inicial detectado - Intentando cerrar...");
                        // Intentar diferentes métodos de clic
                        try
                        {
                            // Intento 1: JavaScript click
                            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", closeButton);
                        }
                        catch
                        {
                            try
                            {
                                // Intento 2: Actions click
                                var actions = new Actions(_driver);
                                actions.MoveToElement(closeButton).Click().Perform();
                            }
                            catch
                            {
                                // Intento 3: Click directo
                                closeButton.Click();
                            }
                        }
                        
                        Console.WriteLine("✓ Mensaje inicial cerrado");
                        await Task.Delay(2000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cerrar mensaje inicial: {ex.Message}");
                    // Continuamos con el proceso aunque falle el cierre del mensaje
                }

                // 2. Ingresar credenciales
                Console.WriteLine("Ingresando credenciales...");
                
                // Email
                var emailContainer = _wait.Until(d => d.FindElement(
                    By.XPath("/html/body/md-content/div/form/md-card/md-card-content/div/md-input-container[1]")));
                var usernameInput = emailContainer.FindElement(By.TagName("input"));
                
                usernameInput.Click();
                await Task.Delay(500);
                usernameInput.Clear();
                usernameInput.SendKeys(AppSettings.Credentials.Username);
                Console.WriteLine("✓ Usuario ingresado");
                await Task.Delay(1000);

                // Contraseña
                var passwordContainer = _wait.Until(d => d.FindElement(
                    By.XPath("/html/body/md-content/div/form/md-card/md-card-content/div/md-input-container[2]")));
                var passwordInput = passwordContainer.FindElement(By.TagName("input"));
                
                passwordInput.Click();
                await Task.Delay(500);
                passwordInput.Clear();
                passwordInput.SendKeys(AppSettings.Credentials.Password);
                Console.WriteLine("✓ Contraseña ingresada");
                await Task.Delay(1000);

                // 3. Marcar Recuérdame - CORRECCIÓN
                var rememberMe = _wait.Until(d => d.FindElement(
                    By.CssSelector("#login > form > md-card > md-card-content > div > div > md-checkbox"))); // Nuevo selector

                if (!rememberMe.Selected)
                {
                    await ClickElement(rememberMe);
                    Console.WriteLine("✓ Opción 'Recuérdame' marcada");
                    await Task.Delay(1000);
                }

                // 4. Clic en botón de login - CORRECCIÓN
                var loginButton = _wait.Until(d => d.FindElement(
                    By.CssSelector("button[type='submit']"))); // Nuevo selector
                await ClickElement(loginButton);
                Console.WriteLine("✓ Botón de inicio de sesión presionado");
                
                await Task.Delay(2000);
                Console.WriteLine("✓ Login externo completado exitosamente");
                _isExternalLogin = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en login externo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Realiza el proceso de login interno buscando y haciendo clic en el botón correspondiente
        /// </summary>
        private async Task<bool> AttemptInternalLogin()
        {
            try
            {
                Console.WriteLine("Buscando botón de inicio de sesión interno...");
                var loginButton = _wait.Until(d => d.FindElement(
                    By.XPath(XPaths.Navigation.LoginInterno)));
                
                if (loginButton != null && loginButton.Displayed)
                {
                    Console.WriteLine("Botón de login interno encontrado - Ejecutando...");
                    await ClickElement(loginButton);
                    await Task.Delay(2000);

                    // Verificar que el menú principal sea visible después del login
                    if (IsAlreadyAuthenticated())
                    {
                        Console.WriteLine("✓ Login interno completado");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("❌ Login interno falló - No se detectó autenticación");
                        return false;
                    }
                }
                
                Console.WriteLine("❌ No se encontró el botón de login interno");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en login interno: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Método utilitario para hacer clic en elementos con manejo de errores
        /// </summary>
        private async Task ClickElement(IWebElement element)
        {
            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
            }
            catch
            {
                element.Click();
            }
            await Task.Delay(1000);
        }

        // Métodos de verificación de estado

        /// <summary>
        /// Verifica si el usuario está autenticado actualmente
        /// </summary>
        public bool IsAuthenticated() 
        {
            if (!_isAuthenticated) return false;
            
            try 
            {
                var currentUrl = _driver?.Url ?? string.Empty;
                return currentUrl.Contains("diacloudsolutions.com");
            }
            catch 
            {
                return _isAuthenticated;
            }
        }

        /// <summary>
        /// Verifica si ya existe una autenticación previa
        /// </summary>
        public bool IsAlreadyAuthenticated()
        {
            try
            {
                // Primero verificar si hay botón de login interno
                try
                {
                    var loginButton = _driver.FindElement(By.XPath(XPaths.Navigation.LoginInterno));
                    if (loginButton != null && loginButton.Displayed)
                    {
                        // Si el botón está visible, significa que necesitamos hacer login
                        return false;
                    }
                }
                catch
                {
                    // Si no encuentra el botón, continuamos con la verificación del menú
                }

                // Verificar si el menú principal está visible
                var menuElement = _wait.Until(d => d.FindElement(
                    By.XPath(XPaths.Navigation.MainMenu)));
                
                return menuElement != null && menuElement.Displayed;
            }
            catch
            {
                return false;
            }
        }

        // Métodos de control de estado de login

        /// <summary>
        /// Verifica si se realizó un login interno
        /// </summary>
        public bool IsLoggedInInternally()
        {
            return _isLoggedInInternally;
        }

        /// <summary>
        /// Verifica si se realizó un login externo
        /// </summary>
        public bool IsLoggedInExternally()
        {
            return _isLoggedInExternally;
        }

        /// <summary>
        /// Establece el estado de los tipos de login
        /// </summary>
        public void SetLoginStatus(bool isInternal, bool isExternal)
        {
            _isLoggedInInternally = isInternal;
            _isLoggedInExternally = isExternal;
        }

        public async Task<bool> HandleLogin(IWebElement loginButton, bool isInternal)
        {
            if (_driver == null) return false;

            try
            {
                if (await TryClosePopup())
                {
                    if (!_hasAttemptedExternalLogin)
                    {
                        Console.WriteLine("Intentando login externo...");
                        return await PerformLogin(loginButton, false);
                    }
                }
                else
                {
                    if (!_hasAttemptedInternalLogin)
                    {
                        Console.WriteLine("No se pudo cerrar mensaje, intentando login interno...");
                        return await PerformLogin(loginButton, true);
                    }
                }

                try
                {
                    if (_driver == null) throw new InvalidOperationException("Driver no inicializado");

                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    var iniciarSesionButton = wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Iniciar sesión')]")));
                    
                    Console.WriteLine("Botón de Iniciar sesión encontrado");
                    await Task.Delay(1000);
                    
                    return await PerformLogin(iniciarSesionButton, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"No se encontró el botón de Iniciar sesión: {ex.Message}");
                }
                
                Console.WriteLine("❌ Todos los intentos de login fallaron");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en manejo de login: {ex.Message}");
                return false;
            }
        }
        private async Task ExecuteClick(IWebElement element)
        {
            if (_driver == null) throw new InvalidOperationException("Driver no inicializado");

            await Task.Run(async () => 
            {
                try
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
                    await Task.Delay(500);
                }
                catch
                {
                    try
                    {
                        var actions = new Actions(_driver);
                        actions.MoveToElement(element).Click().Perform();
                        await Task.Delay(500);
                    }
                    catch
                    {
                        element.Click();
                        await Task.Delay(500);
                    }
                }
            });
        }

        private async Task<bool> TryClosePopup()
        {
            try
            {
                if (_driver == null) throw new InvalidOperationException("Driver no inicializado");

                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                var closeButton = wait.Until(d => d.FindElement(By.XPath("/html/body/div[2]/md-dialog/form/md-dialog-actions/button")));
                
                if (closeButton != null && closeButton.Displayed)
                {
                    await ExecuteClick(closeButton);
                    await Task.Delay(1000);
                    Console.WriteLine("✓ Mensaje de login externo cerrado exitosamente");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se encontró el mensaje de login externo: {ex.Message}");
                return false;
            }
    }

        private async Task<bool> PerformLogin(IWebElement button, bool isInternal)
        {
            try
            {
                Console.WriteLine($"Intentando login {(isInternal ? "interno" : "externo")}...");
                await Task.Delay(1000);

                await ExecuteClick(button);

                if (isInternal)
                {
                    try
                    {
                        if (_driver == null) throw new InvalidOperationException("Driver no inicializado");

                        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                        var confirmButton = wait.Until(d => d.FindElement(By.XPath("/html/body/div[2]/md-dialog/form/md-dialog-actions/button")));
                        
                        Console.WriteLine("Se detectó botón de login interno - requiere autenticación");
                        await Task.Delay(1000);
                        
                        await ExecuteClick(confirmButton);
                        Console.WriteLine("✓ Clic en botón de confirmación realizado");

                        _hasAttemptedInternalLogin = true;
                        _isLoggedInInternally = true;
                    }
                    catch (Exception dialogEx)
                    {
                        Console.WriteLine($"Error en autenticación interna: {dialogEx.Message}");
                        return false;
                    }
                }
                else
                {
                    _hasAttemptedExternalLogin = true;
                    _isLoggedInExternally = true;
                }

                await Task.Delay(2000);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en login {(isInternal ? "interno" : "externo")}: {ex.Message}");
                return false;
            }
        }
        
        public void ResetLoginAttempts()
        {
            _hasAttemptedInternalLogin = false;
            _hasAttemptedExternalLogin = false;
            _isLoggedInInternally = false;
            _isLoggedInExternally = false;
        }

        public bool IsExternalLogin() => _isExternalLogin;

        private async Task<bool> TryExternalLogin()
        {
            try
            {
                Console.WriteLine("Iniciando login externo...");

                try
                {
                    await Task.Delay(2000);
                    var closeButton = _wait.Until(d => d.FindElement(
                        By.XPath("/html/body/div[2]/md-dialog/md-toolbar/div/button")));

                    if (closeButton != null && closeButton.Displayed)
                    {
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", closeButton);
                        await Task.Delay(1000);
                        Console.WriteLine("✓ Mensaje inicial cerrado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cerrar mensaje inicial: {ex.Message}");
                }

                var usernameInput = _wait.Until(d => d.FindElement(By.Name("username")));
                var passwordInput = _wait.Until(d => d.FindElement(By.Name("password")));

                usernameInput.Clear();
                usernameInput.SendKeys(AppSettings.Credentials.Username);
                await Task.Delay(500);

                passwordInput.Clear();
                passwordInput.SendKeys(AppSettings.Credentials.Password);
                await Task.Delay(500);

                var rememberMe = _wait.Until(d => d.FindElement(
                    By.CssSelector("md-checkbox[aria-label='Recuérdame']")));
                if (!rememberMe.Selected)
                {
                    rememberMe.Click();
                }
                await Task.Delay(500);

                var loginButton = _wait.Until(d => d.FindElement(
                    By.CssSelector("button[type='submit']")));
                loginButton.Click();

                await Task.Delay(2000);
                Console.WriteLine("✓ Login externo completado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en login externo: {ex.Message}");
                return false;
            }
        }
    }
}

