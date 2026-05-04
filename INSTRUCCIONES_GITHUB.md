# 📋 INSTRUCCIONES PARA SUBIR A GITHUB

## PARTE 1: Preparar Repositorio Local (Desde CMD)

Abre **Command Prompt (CMD)** o **PowerShell** en Windows y ejecuta:

```batch
cd c:\Sistema_Invernadero
```

### Paso 1: Configurar Git (primera vez)
```batch
git config --global user.email "ivanoich0123@gmail.com"
git config --global user.name "ivanoich0123"
```

### Paso 2: Inicializar repositorio Git
```batch
git init
```

### Paso 3: Agregar todos los archivos
```batch
git add .
```

### Paso 4: Crear commit inicial
```batch
git commit -m "Initial commit: Sistema Remoto para Sistema Invernadero

- Código fuente del proyecto en C#
- Módulos de autenticación y procesamiento de datos
- Documentación completa
- Configuración de seguridad (.gitignore)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

### Paso 5: Verificar commit
```batch
git log --oneline
```

---

## PARTE 2: Crear Repositorio en GitHub

1. **Accede a GitHub:**
   - Abre: https://github.com/new
   - Inicia sesión con: ivanoich0123@gmail.com

2. **Crea el repositorio:**
   - **Repository name:** `sistema-remoto-invernadero`
   - **Description:** Sistema remoto para monitoreo de invernaderos en C#
   - **Public** o **Private** (tu elección)
   - **IMPORTANTE:** NO marques "Initialize this repository with a README" (ya lo tenemos)
   - Haz clic en **Create repository**

---

## PARTE 3: Conectar y Subir a GitHub (Desde CMD)

Copia y pega EXACTAMENTE lo que te aparece después de crear el repo. Será algo como esto:

```batch
git remote add origin https://github.com/ivanoich0123/sistema-remoto-invernadero.git
git branch -M main
git push -u origin main
```

**NOTA:** Si te pide autenticación:
- Puedes usar:
  - Usuario y contraseña de GitHub
  - O un **Personal Access Token** (recomendado para seguridad)

---

## 🔐 Seguridad: Generar Personal Access Token (Recomendado)

Si GitHub te pide contraseña, usa un **token** en lugar de contraseña:

1. Ve a: https://github.com/settings/tokens
2. Haz clic en **Generate new token (classic)**
3. Dale nombre: `git-sistema-invernadero`
4. Marca estos permisos:
   - ✅ `repo` (acceso completo a repositorios)
   - ✅ `write:packages`
5. Haz clic en **Generate token**
6. **COPIA el token** (aparece una sola vez)
7. Úsalo como contraseña cuando Git lo pida

---

## ✅ Verificar Que Todo Está Bien

Después de hacer push, verifica:

```batch
git remote -v
git branch -a
```

Y abre: https://github.com/ivanoich0123/sistema-remoto-invernadero

Deberías ver tu código allí! 🎉

---

## ⚠️ RESUMEN DE SEGURIDAD

**✅ PROTEGIDO:**
- Archivo `.gitignore` excluye `CONEXION A LA BASE DE DATOS_user.txt`
- No incluye credenciales reales
- `appsettings.json` no será subido

**✅ NUEVA CONTRASEÑA:**
```
MySQL_Tech2024@Inv#Secure!
```

**⚠️ PRÓXIMOS PASOS EN TU BD:**
1. Cambia la contraseña de MySQL (ver instrucciones anteriores)
2. Actualiza tu archivo local `appsettings.json` con la nueva contraseña
3. Nunca subas ese archivo a GitHub

---

¿Preguntas? ¡Preguntame!
