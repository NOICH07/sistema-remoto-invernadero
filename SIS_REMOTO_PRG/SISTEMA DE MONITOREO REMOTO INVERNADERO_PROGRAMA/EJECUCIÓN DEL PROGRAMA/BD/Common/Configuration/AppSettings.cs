using System;
using System.IO;

namespace BD.Common.Configuration
{
    public static class AppSettings
    {
        // Rutas de archivos y carpetas
        public const string DownloadPath = @"C:\Users\ivano\OneDrive\Documents\RESIDENCIA\PROYECTO\DESCARGA DE DATOS";
        public static string ProcessedPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "RESIDENCIA",
            "PROYECTO",
            "DATOS_PROCESADOS"
        );

        // Configuración de base de datos
        public static class Database
        {
            public const string ConnectionString = "Server=localhost;Database=bd_invernadero;User ID=root;Password=tecnologico01;";
        }

        // Credenciales de acceso
        public static class Credentials
        {
            public const string Username = "roelul@outlook.com";
            public const string Password = "rulruL99";
        }

        // Configuración de carpetas para archivos
        public static class Folders
        {
            public const string Temperatura = "TEMPERATURA";
            public const string Humedad12 = "PROMEDIO HUMEDAD1-2";
            public const string Humedad34 = "PROMEDIO DE HUMEDAD3-4";
            public const string Encendido = "ENCENDIDO";
            public const string ConsumoRNA = "CONSUMO RNA";
            public const string ConsumoONOFF = "CONSUMO_ONOFF";
            public const string HumedadAmbiente = "HUMEDAD AMBIENTE";
            public const string HisteresisSuperior = "HISTERESIS_SUPERIOR";
            public const string HisteresisInferior = "HISTERESIS_INFERIOR";
        }

        // Configuración de ventanas
        public static class WindowConfig
        {
            public const int SW_MINIMIZE = 6;
            public const int SW_HIDE = 0;
        }

        // Configuración del navegador Chrome
        public static class ChromeConfig
        {
            public static class PreferencesDownload
            {
                public const string DefaultDirectory = DownloadPath;
                public const bool PromptForDownload = false;
                public const string DisablePopupBlocking = "true";
            }

            public static class Arguments
            {
                public const string NoFirstRun = "--no-first-run";
                public const string NoDefaultBrowserCheck = "--no-default-browser-check";
                public const string LogLevel = "--log-level=3";
                public const string StartMaximized = "--start-maximized";
                public const string RemoteDebuggingPort = "--remote-debugging-port=9222";
            }
        }

        // Tiempos de espera
        public static class Delays
        {
            public const int ShortDelay = 2000;
            public const int MediumDelay = 3000;
            public const int LongDelay = 5000;
        }
    }
}