@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ════════════════════════════════════════════════════════════════
echo 🔧 CONFIGURANDO REPOSITORIO GIT
echo ════════════════════════════════════════════════════════════════
echo.

cd /d "c:\Sistema_Invernadero"

REM Verificar si Git está instalado
git --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Git no está instalado. Por favor, descárgalo de: https://git-scm.com/download/win
    pause
    exit /b 1
)

echo ✅ Git encontrado
echo.

REM Verificar si ya hay repositorio
if exist .git (
    echo ✅ Repositorio Git ya existe
) else (
    echo 📁 Inicializando repositorio Git...
    git init
    echo ✅ Repositorio creado
)

echo.
echo 📝 Configurando Git...
git config user.email "ivanoich0123@gmail.com"
git config user.name "ivanoich0123"
echo ✅ Configuración completada

echo.
echo 📦 Agregando archivos al staging...
git add .
echo ✅ Archivos listos para commit

echo.
echo 💾 Creando commit inicial...
git commit -m "Initial commit: Sistema Remoto para Sistema Invernadero

- Código fuente del proyecto en C#
- Módulos de autenticación y procesamiento de datos
- Documentación completa
- Configuración de seguridad (.gitignore)

Co-authored-by: Copilot ^<223556219+Copilot@users.noreply.github.com^>"

echo ✅ Commit creado

echo.
echo ════════════════════════════════════════════════════════════════
echo ✅ REPOSITORIO LOCAL LISTO
echo ════════════════════════════════════════════════════════════════
echo.
echo Próximos pasos:
echo.
echo 1. Crea un repositorio en GitHub:
echo    - Accede a https://github.com/new
echo    - Nombre: sistema-remoto-invernadero
echo    - Descripción: Sistema remoto para monitoreo de invernaderos en C#
echo    - NO inicialices con README (ya lo tenemos)
echo.
echo 2. Después de crear el repositorio en GitHub, ejecuta:
echo    git remote add origin https://github.com/tu-usuario/sistema-remoto-invernadero.git
echo    git branch -M main
echo    git push -u origin main
echo.
echo ════════════════════════════════════════════════════════════════
echo.

git log --oneline -1

echo.
echo Presiona cualquier tecla para salir...
pause
