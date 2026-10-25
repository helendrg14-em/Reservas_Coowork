@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo Instala .NET SDK 10 para ejecutar este proyecto.
  pause
  exit /b 1
)
where sqllocaldb >nul 2>nul
if errorlevel 1 (
  echo Instala SQL Server Express LocalDB desde Visual Studio.
  pause
  exit /b 1
)
sqllocaldb start MSSQLLocalDB
if errorlevel 1 (
  echo No se pudo iniciar LocalDB. Revisa el mensaje anterior.
  pause
  exit /b 1
)
echo.
echo WorkHub: http://localhost:5004
echo Administrador WorkHub: admin@workhub.local
echo Clave WorkHub: Admin123!
echo Espera a que aparezca Now listening on antes de abrir el navegador.
echo.
dotnet run --project Workhub.csproj --launch-profile http
if errorlevel 1 pause
endlocal
