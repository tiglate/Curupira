@echo off
setlocal

:: Find the path to vstest.console.exe using vswhere
for /f "delims=" %%i in ('"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -find **\TestPlatform\vstest.console.exe') do set VSTEST_CONSOLE=%%i

:: Check if vstest.console.exe was found
if "%VSTEST_CONSOLE%"=="" (
    echo vstest.console.exe not found. Please ensure Visual Studio Test Tools are installed.
    exit /b 1
)

:: Run vstest.console.exe with the provided arguments
"%VSTEST_CONSOLE%" %*

endlocal