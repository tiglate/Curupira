@echo off
REM This batch script uninstalls the CurupiraService Windows service.

REM Check for administrator privileges
net session >nul 2>&1
if %errorlevel% == 0 (
  echo Administrator privileges detected. Proceeding with uninstallation...
) else (
  echo Error: This script requires administrator privileges. Please run as administrator.
  pause
  exit /b 1
)

REM Set service name
set SERVICE_NAME=Curupira

REM Check if the service exists
sc query %SERVICE_NAME% >nul 2>&1
if %errorlevel% == 0 (
  echo Stopping service '%SERVICE_NAME%'...
  sc stop %SERVICE_NAME%

  REM Check if the service was stopped successfully
  if %errorlevel% == 0 (
    echo Service '%SERVICE_NAME%' stopped successfully.

    echo Uninstalling service '%SERVICE_NAME%'...
    sc delete %SERVICE_NAME%

    REM Check if the service was deleted successfully
    if %errorlevel% == 0 (
      echo Service '%SERVICE_NAME%' uninstalled successfully.
    ) else (
      echo Error: Failed to uninstall service '%SERVICE_NAME%'.
    )
  ) else (
    echo Error: Failed to stop service '%SERVICE_NAME%'.
  )
) else (
  echo Service '%SERVICE_NAME%' does not exist. Skipping uninstallation.
)

REM Keep the console window open
pause