param([string]$versionName, [string][ValidateSet("live", "pts")]$serverType = "live")

$outputDirectory = Join-Path $PWD "gameparams/out"

Push-Location -Path DataConverter
dotnet run -c Release -- $serverType $outputDirectory $versionName
Pop-Location
