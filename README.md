# 🌱 Sistema Remoto para Sistema Invernadero

Sistema de monitoreo remoto para invernaderos desarrollado en C# con .NET. Permite el seguimiento y control de condiciones ambientales en tiempo real.

## 📋 Descripción

Este proyecto es una solución completa para:
- Monitoreo remoto de parámetros del invernadero
- Almacenamiento de datos en base de datos MySQL
- Interfaz de usuario para visualización y control
- Procesamiento automático de datos

## 🔧 Requisitos Previos

- **.NET Framework** (versión especificada en `.csproj`)
- **MySQL Server** 8.0 o superior
- **Visual Studio 2022** o superior (recomendado)
- **Git** para control de versiones

## 📦 Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/sistema-remoto-invernadero.git
cd sistema-remoto-invernadero
```

### 2. Configurar la Base de Datos

1. Abre MySQL y crea la base de datos:
```sql
CREATE DATABASE invernadero_db;
```

2. Importa el esquema (si existe archivo SQL):
```bash
mysql -u root -p invernadero_db < database/schema.sql
```

### 3. Configurar credenciales

1. Copia el archivo de ejemplo:
```bash
copy appsettings.example.json appsettings.json
```

2. Edita `appsettings.json` con tus credenciales de base de datos:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=invernadero_db;User=root;Password=TU_CONTRASEÑA;"
  }
}
```

### 4. Compilar y ejecutar

```bash
# Abrir solución
start BD.sln

# O compilar desde línea de comandos
dotnet build

# Ejecutar
dotnet run
```

## 🚀 Uso

Ejecuta el archivo batch incluido:
```bash
ejecutar_programa.bat
```

O desde Visual Studio, presiona `F5` para depuración.

## 📁 Estructura del Proyecto

```
Sistema Remoto para Sistema Invernadero/
├── BD/                           # Proyecto principal C#
│   ├── Features/                 # Características modulares
│   │   ├── Authentication/       # Módulo de autenticación
│   │   ├── Processing/           # Procesamiento de datos
│   │   └── ...
│   ├── obj/                      # Archivos compilados (ignorados)
│   ├── bin/                      # Binarios (ignorados)
│   └── BD.csproj                 # Configuración del proyecto
├── BASE DE DATOS/                # Scripts SQL
├── DESCARGA DE DATOS/            # Utilidades de descarga
├── appsettings.json              # Configuración (NO SE COMMIT)
└── README.md                     # Este archivo
```

## 🔐 Seguridad

**⚠️ IMPORTANTE:**
- Usa `appsettings.example.json` como template
- Agrega `appsettings.json` al `.gitignore` (ya incluido)
- Cambia regularmente las credenciales de acceso
- No compartas el archivo `.gitignore` con credenciales

## 🗄️ Base de Datos

- **Motor:** MySQL
- **Versión:** 8.0+
- **Tipo:** Relacional
- **Conexión:** Localhost:3306

## 👥 Autores

- Desarrollador principal: [NOICH07]
- Proyecto de: [Instituto Tecnologico de Culiacan]
---

**Última actualización:** 2026-05-04
