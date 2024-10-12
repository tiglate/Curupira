# Define the main function
function Main {
    param (
        [string]$format = "XML",
		[ValidateSet("Debug", "Release")]
		[string]$mode = "Debug"
    )
	
	if ([string]::IsNullOrEmpty($format)) {
		$format = "XML"
	}
	
	if ([string]::IsNullOrEmpty($mode)) {
		$mode = "Debug"
	}

    # Find the vstest.console.exe path
    $vsTestConsolePath = Find-VSTestConsole
    if (-not $vsTestConsolePath) {
        Write-Host "vstest.console.exe not found. Exiting script."
        exit 1
    }
    
    # Ensure dotCover is installed
    $dotCoverPath = Ensure-DotCoverInstalled
    if (-not $dotCoverPath) {
        Write-Host "dotCover installation failed. Exiting script."
        exit 1
    }

    # List the DLLs to be tested
    $dllsToTest = @(
        "Curupira.Plugins.Tests\bin\$mode\Curupira.Plugins.Tests.dll",
        "Curupira.AppClient.Tests\bin\$mode\Curupira.AppClient.Tests.dll",
        "Curupira.WindowsService.Tests\bin\$mode\Curupira.WindowsService.Tests.dll"
    )
    
    # Convert array of DLLs to a space-separated string
    $dlls = $dllsToTest -join ' '

    # Set default coverage output parameters
    $outputFile = ""
    $reportType = ""

    # Check the format option and adjust the arguments accordingly
    if ($format -eq "HTML") {
        $outputFile = Join-Path $PSScriptRoot "dotCover.Output.html"
        $reportType = "HTML"
    } elseif ($format -eq "XML") {
        $outputFile = Join-Path $PSScriptRoot "coverage.xml"
        $reportType = "XML"
    } else {
        Write-Host "Invalid format specified. Use 'XML' or 'HTML'."
        exit 1
    }

    $solutionPath = Join-Path $PSScriptRoot "..\"

    # Run dotCover with the selected format
    & $dotCoverPath cover --TargetExecutable $vsTestConsolePath `
                          --TargetArguments "$dlls" `
                          --Output $outputFile `
                          --ReportType $reportType `
                          --TargetWorkingDir $solutionPath `
                          --filters="+:module=Curupira*" `
						  --attributeFilters=System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute
}

function Ensure-DotCoverInstalled {
    # Check if dotCover is already installed
    $dotCoverPath = "$env:USERPROFILE\.dotnet\tools\dotnet-dotCover.exe"
    
    if (Test-Path $dotCoverPath) {
        Write-Host "dotCover is already installed at $dotCoverPath"
        return $dotCoverPath
    } else {
        Write-Host "dotCover is not installed. Installing now..."

        # Install dotCover using dotnet CLI
        $installResult = dotnet tool install --global JetBrains.dotCover.CommandLineTools
        
        if ($installResult.ExitCode -eq 0) {
            Write-Host "dotCover installed successfully."
            return "$env:USERPROFILE\.dotnet\tools\dotnet-dotCover.exe"
        } else {
            Write-Host "Failed to install dotCover."
            return $null
        }
    }
}

function Find-VSTestConsole {
    $vswherePath = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"

    # Check if vswhere.exe exists
    if (-Not (Test-Path $vswherePath)) {
        Write-Host "vswhere.exe not found at $vswherePath"
        return $null
    }

    # Run vswhere.exe to find Visual Studio installations with vstest.console.exe
    $vsTestConsolePath = & $vswherePath -latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -find **\TestPlatform\vstest.console.exe

    # Check if vstest.console.exe was found
    if (-Not [string]::IsNullOrWhiteSpace($vsTestConsolePath)) {
        Write-Host "vstest.console.exe found at: $vsTestConsolePath"
        return $vsTestConsolePath
    } else {
        Write-Host "vstest.console.exe not found in any Visual Studio installation"
        return $null
    }
}

# Start the script and allow passing format as a parameter
Main @args
