. .\build-common.ps1

function Write-EnvFile {
	# Create the target directory if it doesn't exist
	$targetDir = Join-Path (Get-ScriptDirectory) "dist\bin"
	if (!(Test-Path -Path $targetDir)) {
		New-Item -ItemType Directory -Path $targetDir | Out-Null
	}

	# Generate a random API key using multiple GUIDs
	$apiKey = ((New-Guid).Guid -replace "-") + ((New-Guid).Guid -replace "-") + ((New-Guid).Guid -replace "-")

	# Create the .env file content
	$envContent = "API_KEY=$apiKey"

	# Write the content to the .env file
	$envFile = Join-Path $targetDir ".env"
	$envContent | Out-File -FilePath $envFile -Encoding UTF8
}

function Main {
    param (
        [string]$target
    )
	Execute $target "build-def\build-service.proj" "Curupira.WindowsService\bin\Release"
	Write-EnvFile
}

Main $args[0]