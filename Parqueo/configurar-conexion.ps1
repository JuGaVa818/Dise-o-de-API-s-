$ErrorActionPreference = "Stop"

$projectPath = Join-Path $PSScriptRoot "Parqueo.csproj"
$project = [xml](Get-Content -Raw $projectPath)
$userSecretsId = $project.Project.PropertyGroup.UserSecretsId |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($userSecretsId)) {
    throw "No se encontró UserSecretsId en Parqueo.csproj."
}

$hostName = Read-Host "Host PostgreSQL [postgresql-hecferme.postgres.database.azure.com]"
if ([string]::IsNullOrWhiteSpace($hostName)) {
    $hostName = "postgresql-hecferme.postgres.database.azure.com"
}

$database = Read-Host "Base de datos [advanced_programming]"
if ([string]::IsNullOrWhiteSpace($database)) {
    $database = "advanced_programming"
}

$username = Read-Host "Usuario PostgreSQL [user_03]"
if ([string]::IsNullOrWhiteSpace($username)) {
    $username = "user_03"
}

$securePassword = Read-Host "Contraseña PostgreSQL" -AsSecureString
if ($securePassword.Length -eq 0) {
    throw "La contraseña no puede estar vacía."
}

$passwordPointer = [IntPtr]::Zero
try {
    $passwordPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)

    $builder = [System.Data.Common.DbConnectionStringBuilder]::new()
    $builder.Add("Host", $hostName)
    $builder.Add("Port", 5432)
    $builder.Add("Database", $database)
    $builder.Add("Username", $username)
    $builder.Add("Password", $password)
    $builder.Add("SSL Mode", "Require")
    $builder.Add("Trust Server Certificate", $true)

    $settingsDirectory = Join-Path $env:APPDATA "Microsoft\UserSecrets\$userSecretsId"
    $settingsPath = Join-Path $settingsDirectory "secrets.json"
    $settings = [ordered]@{}

    if (Test-Path $settingsPath) {
        $existing = Get-Content -Raw $settingsPath | ConvertFrom-Json
        foreach ($property in $existing.PSObject.Properties) {
            $settings[$property.Name] = $property.Value
        }
    }

    if (-not $settings.Contains("ConnectionStrings")) {
        $settings["ConnectionStrings"] = [ordered]@{}
    }
    elseif ($settings["ConnectionStrings"] -isnot [System.Collections.IDictionary]) {
        $existingConnections = [ordered]@{}
        foreach ($property in $settings["ConnectionStrings"].PSObject.Properties) {
            $existingConnections[$property.Name] = $property.Value
        }

        $settings["ConnectionStrings"] = $existingConnections
    }

    $settings["ConnectionStrings"]["Parqueo"] = $builder.ConnectionString
    New-Item -ItemType Directory -Force -Path $settingsDirectory | Out-Null
    $settings | ConvertTo-Json -Depth 100 |
        Set-Content -Path $settingsPath -Encoding UTF8

    Write-Output "Cadena guardada en User Secrets para el proyecto Parqueo. No se imprimió la contraseña."
}
finally {
    if ($passwordPointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer)
    }

    if ($null -ne $password) {
        $password = $null
    }

    if ($null -ne $securePassword) {
        $securePassword.Dispose()
    }
}
