@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul

:: Перевіряємо чи передано параметр
if "%1"=="" (
    echo ❌ Помилка: Не вказано ім'я міграції!
    echo Використання: add-migration.bat [ІмяМіграції]
    echo Приклад: add-migration.bat InitDatabase
    pause
    exit /b 1
)

set migration_name=%1

echo 🔧 Створення міграції "%migration_name%"...
echo.

:: Спроба 1: З папки Data проекту (найнадійніший спосіб)
echo 🚀 Спроба 1: Створення з папки EasyPeasy.Data...
pushd EasyPeasy.Data

dotnet ef migrations add "%migration_name%" --context "EasyPeasyDbContext" --verbose

if %errorlevel% equ 0 (
    popd
    echo.
    echo ✅ Міграція "%migration_name%" створена успішно!
    goto :success
)

popd
echo ⚠️ Спроба 1 не вдалася, пробуємо спосіб 2...
echo.

:: Спроба 2: З кореневої папки без startup-project
echo 🚀 Спроба 2: Створення з кореневої папки...
dotnet ef migrations add "%migration_name%" ^
    --project "EasyPeasy.Data" ^
    --output-dir "Migrations" ^
    --context "EasyPeasyDbContext" ^
    --verbose

if %errorlevel% equ 0 (
    echo.
    echo ✅ Міграція "%migration_name%" створена успішно!
    goto :success
)

echo ⚠️ Спроба 2 не вдалася, пробуємо спосіб 3...
echo.

:: Спроба 3: З --no-build прапором
echo 🚀 Спроба 3: Створення з --no-build...
dotnet ef migrations add "%migration_name%" ^
    --project "EasyPeasy.Data" ^
    --output-dir "Migrations" ^
    --context "EasyPeasyDbContext" ^
    --no-build ^
    --verbose

if %errorlevel% equ 0 (
    echo.
    echo ✅ Міграція "%migration_name%" створена успішно!
    goto :success
)

:: Всі спроби не вдалися
echo.
echo ❌ Всі спроби створення міграції не вдалися!
echo.
echo 💡 Спробуйте вручну:
echo    1. cd EasyPeasy.Data
echo    2. dotnet ef migrations add %migration_name%
echo    3. cd ..
echo.
pause
exit /b 1

:success
echo.
echo 📁 Перевіряємо створені файли:
dir "EasyPeasy.Data\Migrations\*%migration_name%*" /b 2>nul
echo.
echo 🎉 Готово!
pause