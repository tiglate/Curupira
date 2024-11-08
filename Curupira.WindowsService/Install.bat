@echo off
REM This batch script installs the CurupiraService as a Windows service.

REM Check for administrator privileges
net session >nul 2>&1
if %errorlevel% == 0 (
  echo Administrator privileges detected. Proceeding with installation...
) else (
  echo Error: This script requires administrator privileges. Please run as administrator.
  pause
  exit /b 1 
)

REM Set service name and display name
set SERVICE_NAME=Curupira
set DISPLAY_NAME=Curupira

REM Set service description
set SERVICE_DESCRIPTION="A lightweight Command and Control server with command-line utilities for automating tasks and managing Windows servers."

REM Set service executable path (adjust if needed)
set SERVICE_PATH=%~dp0%SERVICE_NAME%.exe

REM Check if the service already exists
sc query %SERVICE_NAME% >nul 2>&1
if %errorlevel% == 0 (
  echo Service '%SERVICE_NAME%' already exists. Skipping installation.
) else (
  echo Installing service '%SERVICE_NAME%'...

  REM Create the service with automatic start
  sc create %SERVICE_NAME% binPath= "%SERVICE_PATH%" start= auto DisplayName= "%DISPLAY_NAME%" obj= LocalSystem

  REM Check if the service was created successfully
  if %errorlevel% == 0 (
    echo Service '%SERVICE_NAME%' installed successfully.

    REM Set service description
    sc description %SERVICE_NAME% "%SERVICE_DESCRIPTION%" 

    echo Starting service '%SERVICE_NAME%'...
    sc start %SERVICE_NAME%
    if %errorlevel% == 0 (
      echo Service '%SERVICE_NAME%' started successfully.
    ) else (
      echo Error: Failed to start service '%SERVICE_NAME%'.
    )
  ) else (
    echo Error: Failed to install service '%SERVICE_NAME%'.
  )
)

REM Keep the console window open
pause