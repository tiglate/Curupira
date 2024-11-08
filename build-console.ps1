. .\build-common.ps1

function Main {
    param (
        [string]$target
    )
	Execute $target "build-def\build-console.proj" "Curupira.AppClient\bin\Release"
}

Main $args[0]