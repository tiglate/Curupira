# Define the main function
function Main {
    param (
        [string]$target = "package",  # Default target is 'package'
        [string]$projFile
    )
	
	if ([string]::IsNullOrEmpty($target)) {
		$target = "package"
	}

    switch ($target) {
        "package" {
            Package $projFile
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

# Package target: Build and package the application
function Package {
    param (
        [string]$projFile
    )

    # Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

    # Execute the msbuild command for Build-Release
    Execute-MSBuild $msbuildPath "Build-Release" $projFile
}

# Build target: Build the application in Debug mode
function Build {
    param (
        [string]$projFile
    )

    # Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

    # Execute the msbuild command for Build-Debug
    Execute-MSBuild $msbuildPath "Build-Debug" $projFile
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

# Get the directory where the script is located
function Get-ScriptDirectory {
    return (Get-Location).Path
}
