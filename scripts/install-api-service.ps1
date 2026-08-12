<#
.SYNOPSIS
    Publica Inventario.Api y la instala/actualiza como Windows Service local en esta PC de sucursal.

.DESCRIPTION
    Cada sucursal corre su propia Inventario.Api como servicio de Windows, independiente del
    Desktop, escuchando solo en localhost (Inventario.Web y Inventario.Desktop de esa misma PC
    son los únicos consumidores). Este script:
      1) Publica Inventario.Api en modo Release (self-contained opcional vía -SelfContained).
      2) Detiene y borra el servicio si ya existía (para poder actualizar el binario).
      3) Crea el servicio con sc.exe apuntando al .exe publicado.
      4) Lo deja configurado con arranque automático e inicia el servicio.

    Debe ejecutarse como Administrador.

.PARAMETER ServiceName
    Nombre del servicio de Windows. Por defecto "InventarioApi".

.PARAMETER InstallPath
    Carpeta donde se publica y desde donde corre la Api. Por defecto
    "C:\InventarioApp\Api".

.PARAMETER SelfContained
    Si se pasa, publica un ejecutable self-contained (no requiere el runtime de .NET instalado
    en la PC de la sucursal). Por defecto usa el runtime compartido (framework-dependent).

.EXAMPLE
    .\install-api-service.ps1
    Publica e instala/actualiza el servicio con los valores por defecto.

.EXAMPLE
    .\install-api-service.ps1 -ServiceName "InventarioApi-Sucursal2" -InstallPath "D:\Inventario\Api"
#>
[CmdletBinding()]
param(
    [string]$ServiceName = "InventarioApi",
    [string]$InstallPath = "C:\InventarioApp\Api",
    [switch]$SelfContained
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $repoRoot "Inventario.Api\Inventario.Api.csproj"

if (-not (Test-Path $apiProject)) {
    throw "No se encontró $apiProject. Ejecuta este script desde el checkout del repo."
}

Write-Host "Publicando Inventario.Api en $InstallPath ..." -ForegroundColor Cyan
$publishArgs = @(
    "publish", $apiProject,
    "-c", "Release",
    "-o", $InstallPath,
    "--self-contained", $SelfContained.IsPresent.ToString().ToLower()
)
dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish falló (código $LASTEXITCODE)."
}

$existente = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existente) {
    Write-Host "Deteniendo y eliminando el servicio existente '$ServiceName' para actualizarlo..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

$exePath = Join-Path $InstallPath "Inventario.Api.exe"
if (-not (Test-Path $exePath)) {
    throw "No se encontró $exePath tras publicar. Revisa la salida de dotnet publish."
}

Write-Host "Creando el servicio '$ServiceName' -> $exePath" -ForegroundColor Cyan
sc.exe create $ServiceName binPath= "`"$exePath`"" start= auto DisplayName= "Inventario Api ($ServiceName)" | Out-Null
sc.exe description $ServiceName "Api local de Inventario para esta sucursal (SQLite en %AppData%\InventarioApp)." | Out-Null

Start-Service -Name $ServiceName
Write-Host "Servicio '$ServiceName' instalado e iniciado." -ForegroundColor Green
Write-Host "Revisa el puerto configurado (ASPNETCORE_URLS / appsettings.json) y que Inventario.Web apunte a esa misma URL local." -ForegroundColor Yellow
