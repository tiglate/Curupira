# Define the main function
function Main {
    param (
        [string]$target = "package"  # Default target is 'package'
    )
	
	if ([string]::IsNullOrEmpty($target)) {
		$target = "package"
	}

    switch ($target) {
        "package" {
            Package
        }
        "build" {
            Build
        }
        "compile" {
            Build
        }
        "test" {
            Test
        }
        "clean" {
            Clean
        }
        "clear" {
            Clean
        }
        default {
            Write-Host "Unknown target: $target. Valid targets are 'package', 'build', 'test', 'clean', and 'clear'."
            exit 1
        }
    }
}

# Package target: Build and package the application
function Package {
    # Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

    # Execute the msbuild command for Build-Release
    Execute-MSBuild $msbuildPath "Build-Release"

    # Create the distribution folders
    $distPath = Join-Path (Get-ScriptDirectory) "dist"
    Create-DistFolders $distPath

    # Copy necessary files to the distribution folder
    Copy-FilesToDist $distPath
}

# Build target: Build the application in Debug mode
function Build {
    # Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

    # Execute the msbuild command for Build-Debug
    Execute-MSBuild $msbuildPath "Build-Debug"
}

# Test target: Run unit tests using vstest.console.exe
function Test {
    # Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

    # Execute the msbuild command for Run-Tests
    Execute-MSBuild $msbuildPath "Run-Tests"
}

# Clean target: Delete the dist folder and debug/release binaries
function Clean {
    # Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

    # Execute the msbuild command for Clean
    Execute-MSBuild $msbuildPath "Clean"

    $distPath = Join-Path (Get-ScriptDirectory) "dist"

    # Remove dist folder
    if (Test-Path $distPath) {
        Remove-Item -Recurse -Force $distPath
        Write-Host "Deleted dist folder."
    }
}

# Find the location of msbuild.exe using vswhere
function Find-MSBuild {
    $vswherePath = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswherePath)) {
        Write-Host "vswhere.exe not found."
        return $null
    }

    $msbuildPath = & "$vswherePath" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found."
        return $null
    }

    Write-Host "MSBuild found at: $msbuildPath"
    return $msbuildPath
}

# Execute msbuild with the provided target
function Execute-MSBuild {
    param (
        [string]$msbuildPath,
        [string]$target
    )

    $buildProjPath = Join-Path (Get-ScriptDirectory) "build-console.proj"
    if (-not (Test-Path $buildProjPath)) {
        Write-Host "build-console.proj not found."
        exit 1
    }

    & "$msbuildPath" $buildProjPath /t:$target
    if ($LASTEXITCODE -ne 0) {
        Write-Host "MSBuild execution failed."
        exit $LASTEXITCODE
    }
}

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

# Get the directory where the script is located
function Get-ScriptDirectory {
    return (Get-Location).Path
}

# Call the main function, passing the first command line argument
Main $args[0]
