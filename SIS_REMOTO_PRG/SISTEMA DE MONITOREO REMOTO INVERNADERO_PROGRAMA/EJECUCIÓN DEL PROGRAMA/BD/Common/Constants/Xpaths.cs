namespace BD.Common.Constants
{
    public static class XPaths
    {
        public static class Navigation
{
    public const string MainMenu = "//button[@aria-label='Menu']";
    public const string Device = "//button[contains(text(),'Devices')]";
    public const string MasButtonDevice = "//button[@aria-label='Más']";
    public const string Registros = "//md-list-item[contains(.,'Registros')]";
    public const string CloseInitialMessage = "//button[@aria-label='Cerrar']";
    public const string LoginInterno = "//button[@aria-label='Iniciar sesión']";
    public const string PagSig="/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-content-wrapper/md-tab-content[2]/div/md-content/md-list/md-list-item[1]/div/div/button[2]";
}

        public static class Buttons
        {
            public static class TresPuntos
            {
                public const string Encendido = "/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-content-wrapper/md-tab-content[2]/div/md-content/md-list/md-list-item[2]/md-menu/button";
                public const string ConsumoRNA = "/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-content-wrapper/md-tab-content[2]/div/md-content/md-list/md-list-item[5]/md-menu/button";
                public const string ConsumoONOFF = "/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-content-wrapper/md-tab-content[2]/div/md-content/md-list/md-list-item[6]/md-menu/button";
                public const string Humedad12 = "/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-content-wrapper/md-tab-content[2]/div/md-content/md-list/md-list-item[7]/md-menu/button";
                public const string Humedad34 = "/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-content-wrapper/md-tab-content[2]/div/md-content/md-list/md-list-item[8]/md-menu/button";
                public const string Temperatura = "/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-content-wrapper/md-tab-content[2]/div/md-content/md-list/md-list-item[9]/md-menu/button";
                public const string HumedadAmbiente = "/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-content-wrapper/md-tab-content[2]/div/md-content/md-list/md-list-item[10]/md-menu/button";
                public const string HisteresisSup = "/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-content-wrapper/md-tab-content[2]/div/md-content/md-list/md-list-item[2]/md-menu/button";
                public const string HisteresisInf = "/html/body/md-content/div/div/md-content/div/div/div/md-sidenav/md-content/md-tabs/md-tabs-content-wrapper/md-tab-content[2]/div/md-content/md-list/md-list-item[3]/md-menu/button";
            }


            public static class Historial
            {
                public const string Encendido = "/html/body/div[2]/md-menu-content/md-menu-item[1]/button";
                public const string ConsumoRNA = "/html/body/div[3]/md-menu-content/md-menu-item[1]/button";
                public const string ConsumoONOFF = "/html/body/div[4]/md-menu-content/md-menu-item[1]/button";
                public const string Humedad12 = "/html/body/div[5]/md-menu-content/md-menu-item[1]/button";
                public const string Humedad34 = "/html/body/div[6]/md-menu-content/md-menu-item[1]/button";
                public const string Temperatura = "/html/body/div[7]/md-menu-content/md-menu-item[1]/button";
                public const string HumedadAmbiente = "/html/body/div[8]/md-menu-content/md-menu-item[1]/button";
                public const string HisteresisSup = "/html/body/div[2]/md-menu-content/md-menu-item[1]/button";
                public const string HisteresisInf = "/html/body/div[3]/md-menu-content/md-menu-item[1]/button";
            }

            public static class Exportar
            {
                public const string Encendido = "/html/body/div[3]/md-dialog/md-toolbar/div/button[2]";
                public const string ConsumoRNA = "/html/body/div[4]/md-dialog/md-toolbar/div/button[2]";
                public const string ConsumoONOFF = "/html/body/div[5]/md-dialog/md-toolbar/div/button[2]";
                public const string Humedad12 = "/html/body/div[6]/md-dialog/md-toolbar/div/button[2]";
                public const string Humedad34 = "/html/body/div[7]/md-dialog/md-toolbar/div/button[2]";
                public const string Temperatura = "/html/body/div[8]/md-dialog/md-toolbar/div/button[2]";
                public const string HumedadAmbiente = "/html/body/div[9]/md-dialog/md-toolbar/div/button[2]";
                public const string HisteresisSup = "/html/body/div[3]/md-dialog/md-toolbar/div/button[2]";
                public const string HisteresisInf = "/html/body/div[4]/md-dialog/md-toolbar/div/button[2]";
            }

            public static class Cerrar
            {
                public const string Encendido = "/html/body/div[3]/md-dialog/md-toolbar/div/button[3]";
                public const string ConsumoRNA = "/html/body/div[4]/md-dialog/md-toolbar/div/button[3]";
                public const string ConsumoONOFF = "/html/body/div[5]/md-dialog/md-toolbar/div/button[3]";
                public const string Humedad12 = "/html/body/div[6]/md-dialog/md-toolbar/div/button[3]";
                public const string Humedad34 = "/html/body/div[7]/md-dialog/md-toolbar/div/button[3]";
                public const string Temperatura = "/html/body/div[8]/md-dialog/md-toolbar/div/button[3]";
                public const string HumedadAmbiente = "/html/body/div[9]/md-dialog/md-toolbar/div/button[3]";
                public const string HisteresisSup = "/html/body/div[3]/md-dialog/md-toolbar/div/button[3]";
                public const string HisteresisInf = "/html/body/div[4]/md-dialog/md-toolbar/div/button[3]";
            }
        }

        public static class Downloads
        {
            public const string HisteresisSuperiorExport = "//button[contains(@aria-label, 'Exportar')]//following::button[contains(text(), 'Histéresis Superior')]";
            public const string HisteresisInferiorExport = "//button[contains(@aria-label, 'Exportar')]//following::button[contains(text(), 'Histéresis Inferior')]";
            public const string ExportarDatos = "//button[contains(text(), 'Exportar datos')]";
            public const string ConfirmarExport = "//button[contains(text(), 'Confirmar')]";
        }
    }

    public static class XPathConstants
    {
        // Login paths
        public const string EXTERNAL_LOGIN_BUTTON = "/html/body/md-content/div/form/md-card/md-card-content/div/button";
        public const string REMEMBER_ME_CHECKBOX = "/html/body/md-content/div/form/md-card/md-card-content/div/div/md-checkbox";
        public const string INTERNAL_LOGIN_BUTTON = "/html/body/div[2]/md-dialog/form/md-dialog-actions/button";
    }
}