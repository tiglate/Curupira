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
        "clean" {
            Clean
        }
        "test" {
            Test
        }
        default {
            Write-Host "Unknown target: $target. Valid targets are 'package', 'clean', and 'test'."
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

    # Execute the msbuild command
    Execute-MSBuild $msbuildPath

    # Create the distribution folders
    $distPath = Join-Path (Get-ScriptDirectory) "dist"
    Create-DistFolders $distPath

    # Copy necessary files to the distribution folder
    Copy-FilesToDist $distPath
}

# Clean target: Delete the dist folder and debug/release binaries
function Clean {
	# Find the msbuild.exe path
    $msbuildPath = Find-MSBuild
    if (-not $msbuildPath) {
        Write-Host "MSBuild not found. Exiting script."
        exit 1
    }

	# Clean the solution build output
    & "$msbuildPath" "Curupira.sln" /t:Clean /p:Configuration=Debug
	& "$msbuildPath" "Curupira.sln" /t:Clean /p:Configuration=Release

    $distPath = Join-Path (Get-ScriptDirectory) "dist"
    

    # Remove dist folder
    if (Test-Path $distPath) {
        Remove-Item -Recurse -Force $distPath
        Write-Host "Deleted dist folder."
    }

}

# Test target: Run unit tests using vstest.console.exe
function Test {
    # Find vstest.console.exe
    $vstestPath = Find-VSTest
    if (-not $vstestPath) {
        Write-Host "vstest.console.exe not found. Exiting script."
        exit 1
    }

    $testDllPath = Join-Path (Get-ScriptDirectory) "Curupira.Tests\bin\Release\Curupira.Tests.dll"

    # Ensure the test DLL exists
    if (-not (Test-Path $testDllPath)) {
        Write-Host "Test DLL not found at: $testDllPath"
        exit 1
    }

    # Execute vstest.console.exe
    & "$vstestPath" $testDllPath
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Test execution failed."
        exit $LASTEXITCODE
    }

    Write-Host "Tests executed successfully."
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

# Find the location of vstest.console.exe using vswhere
function Find-VSTest {
    $vswherePath = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswherePath)) {
        Write-Host "vswhere.exe not found."
        return $null
    }

    $vstestPaths = & "$vswherePath" -latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -find **\vstest.console.exe

    if (-not $vstestPaths) {
        Write-Host "vstest.console.exe not found."
        return $null
    }

    # Take the first result if multiple entries are returned
    $vstestPath = $vstestPaths[0]

    Write-Host "vstest.console.exe found at: $vstestPath"
    return $vstestPath
}

# Execute msbuild with the provided build.proj
function Execute-MSBuild {
    param (
        [string]$msbuildPath
    )

    $buildProjPath = Join-Path (Get-ScriptDirectory) "build-console.proj"
    if (-not (Test-Path $buildProjPath)) {
        Write-Host "build.proj not found."
        exit 1
    }

    & "$msbuildPath" $buildProjPath
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

    Write-Host "Files copied to distribution folder."
}

# Get the directory where the script is located
function Get-ScriptDirectory {
    return (Get-Location).Path
}

# Call the main function, passing the first command line argument
Main $args[0]
