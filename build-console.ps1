. .\build-common.ps1

# Create the distribution folders
function Create-DistFolders {
    param (
        [string]$distPath
    )

    $folders = @("bin", "conf", "lib", "logs")
    foreach ($folder in $folders) {
        $folderPath = Join-Path $distPath $folder
        if (-not (Test-Path $folderPath)) {
            New-Item -Path $folderPath -ItemType Directory | Out-Null
        }
    }

    Write-Host "Distribution folders created at: $distPath"
}

# Copy necessary files to the dist folder
function Copy-FilesToDist {
    param (
        [string]$distPath
    )

    $releasePath = Join-Path (Get-ScriptDirectory) "Curupira.AppClient\bin\Release"
    $solutionPath = Get-ScriptDirectory

    # Copy .dll files to dist\lib
    Copy-Item -Path "$releasePath\*.dll" -Destination (Join-Path $distPath "lib") -Force

    # Copy .exe files to dist\bin
    Copy-Item -Path "$releasePath\*.exe" -Destination (Join-Path $distPath "bin") -Force

    # Copy Curupira.exe.config to dist\conf
    Copy-Item -Path "$releasePath\Curupira.exe.config" -Destination (Join-Path $distPath "conf") -Force

    # Copy NLog.config to dist\conf
    Copy-Item -Path "$releasePath\NLog.config" -Destination (Join-Path $distPath "conf") -Force

    # Copy Config\*.* to dist\conf
    Copy-Item -Path (Join-Path $releasePath "Config\*.*") -Destination (Join-Path $distPath "conf") -Force

    # Copy LICENSE.txt to dist
    Copy-Item -Path (Join-Path $solutionPath "LICENSE.txt") -Destination $distPath -Force

    Write-Host "Files copied to distribution folder."
}

# Override the Package function to include distribution steps
function Package {
    # Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

    # Execute the msbuild command for Build-Release
    Execute-MSBuild $msbuildPath "Build-Release" "build-console.proj"

    # Create the distribution folders
    $distPath = Join-Path (Get-ScriptDirectory) "dist"
    Create-DistFolders $distPath

    # Copy necessary files to the distribution folder
    Copy-FilesToDist $distPath
}

# Call the main function, passing the first command line argument and the project file
Main $args[0] "build-console.proj"
