# This is the main function of the script
function Execute {
    param (
        [string]$target = "package",  # Default target is 'package'
        [string]$projFile,
        [string]$releaseDir
    )
	
	if ([string]::IsNullOrEmpty($target)) {
		$target = "package"
	}

    switch ($target) {
        "package" {
            Package $projFile $releaseDir
        }
        "build" {
            Build $projFile
        }
        "compile" {
            Build $projFile
        }
        "test" {
            Test $projFile
        }
        "clean" {
            Clean $projFile
        }
        "clear" {
            Clean $projFile
        }
        default {
            Write-Host "Unknown target: $target. Valid targets are 'package', 'build', 'test', 'clean', and 'clear'."
            exit 1
        }
    }
}

# Override the Package function to include distribution steps
function Package {
    param (
        [string]$projFile,
        [string]$releaseDir
    )

    Build $projFile "Release"

    # Create the distribution folders
    $distPath = Join-Path (Get-ScriptDirectory) "dist"
    Create-DistFolders $distPath

    # Copy necessary files to the distribution folder
    Copy-FilesToDist $distPath $releaseDir
}

# Build target: Build the application (in Debug mode by default)
function Build {
    param (
        [string]$projFile,
        [string]$config = "Debug"
    )

	if ([string]::IsNullOrEmpty($config)) {
		$config = "Debug"
	}

    # Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

    # Execute the msbuild command for Build-Debug
    Execute-MSBuild $msbuildPath "Build-$config" $projFile
}

# Test target: Run unit tests using vstest.console.exe
function Test {
    param (
        [string]$projFile
    )

    # Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

    # Execute the msbuild command for Run-Tests
    Execute-MSBuild $msbuildPath "Run-Tests" $projFile
}

# Clean target: Delete the dist folder and debug/release binaries
function Clean {
    param (
        [string]$projFile
    )

    # Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

    # Execute the msbuild command for Clean
    Execute-MSBuild $msbuildPath "Clean" $projFile

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
        [string]$target,
        [string]$projFile
    )

    if (-not (Test-Path $projFile)) {
        Write-Host "$projFile not found."
        exit 1
    }

    & "$msbuildPath" $projFile /t:$target
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
        [string]$distPath,
        [string]$releaseDir
    )

    $releasePath = Join-Path (Get-ScriptDirectory) $releaseDir
    $solutionPath = Get-ScriptDirectory

    # Copy .dll files to dist\lib
    Copy-Item -Path "$releasePath\*.dll" -Destination (Join-Path $distPath "lib") -Force

    # Copy .exe files to dist\bin
    Copy-Item -Path "$releasePath\*.exe" -Destination (Join-Path $distPath "bin") -Force

    # Copy .bat files to dist\bin
    Copy-Item -Path "$releasePath\*.bat" -Destination (Join-Path $distPath "bin") -Force

    # Copy *.config to dist\conf
    Copy-Item -Path "$releasePath\*.config" -Destination (Join-Path $distPath "conf") -Force

    # Copy Config\*.* to dist\conf
    Copy-Item -Path (Join-Path $releasePath "Config\*.*") -Destination (Join-Path $distPath "conf") -Force

    # Copy LICENSE.txt to dist
    Copy-Item -Path (Join-Path $solutionPath "LICENSE.txt") -Destination $distPath -Force

    Write-Host "Files copied to distribution folder."
}

# Get the directory where the script is located
function Get-ScriptDirectory {
    return $PSScriptRoot
}
