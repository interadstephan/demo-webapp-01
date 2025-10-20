@echo off
echo Starting Offline Sync Application...
echo.

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo Docker is not running. Please start Docker and try again.
    exit /b 1
)

REM Start SQL Server
echo Starting SQL Server...
docker-compose up -d

REM Wait for SQL Server to be ready
echo Waiting for SQL Server to be ready...
timeout /t 10 /nobreak >nul

REM Start backend
REM echo Starting ASP.NET Core API...
REM start /B cmd /c "cd OfflineSync.Api && dotnet run"

REM Wait for backend to start
timeout /t 5 /nobreak >nul

REM Start frontend
echo Starting Angular frontend...
start /B cmd /c "cd OfflineSync.Client && npm start"

echo.
echo Application started!
echo    Backend API: http://localhost:5000
echo    Frontend UI: http://localhost:4200
echo.
echo Press any key to stop all services...
pause >nul

REM Stop services
echo Stopping services...
docker-compose down
taskkill /F /IM dotnet.exe /T >nul 2>&1
taskkill /F /IM node.exe /T >nul 2>&1
