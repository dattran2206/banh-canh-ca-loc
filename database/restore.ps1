param(
    [Parameter(Mandatory = $true)]
    [string]$BackupFile,
    [string]$ServerName,
    [string]$Username,
    [string]$Password,
    [string]$DatabaseName = "BanhCanhCaLoc"
)

# Determine script root
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Normalize path of backup file
$absoluteBackupFile = Resolve-Path $BackupFile -ErrorAction SilentlyContinue
if (-not $absoluteBackupFile) {
    # Try relative to script root
    $absoluteBackupFile = Resolve-Path (Join-Path $scriptRoot "../$BackupFile") -ErrorAction SilentlyContinue
}
if (-not $absoluteBackupFile) {
    # Try as direct path
    $absoluteBackupFile = $BackupFile
}

if (-not (Test-Path $absoluteBackupFile)) {
    Write-Error "Khong tim thay file backup tai: $BackupFile"
    exit 1
}

# Default values from appsettings
$parsedServer = "localhost"
$parsedDb = "BanhCanhCaLoc"
$parsedUser = ""
$parsedPassword = ""

$appSettingsPath = Join-Path $scriptRoot "../backend/BanhCanhCaLoc.Api/appsettings.json"
if (Test-Path $appSettingsPath) {
    try {
        $config = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        $connStr = $config.ConnectionStrings.DefaultConnection
        if ($connStr) {
            if ($connStr -match "Server=([^;]+)") { $parsedServer = $Matches[1] }
            if ($connStr -match "Database=([^;]+)") { $parsedDb = $Matches[1] }
            if ($connStr -match "User Id=([^;]+)") { $parsedUser = $Matches[1] }
            if ($connStr -match "Password=([^;]+)") { $parsedPassword = $Matches[1] }
        }
    }
    catch {
        Write-Warning "Khong the doc cau hinh ket noi tu appsettings.json. Se dung cau hinh mac dinh."
    }
}

# Override with parameters or fallback to parsed values
$server = if ($ServerName) { $ServerName } else { $parsedServer }
$db = if ($DatabaseName) { $DatabaseName } else { $parsedDb }
$user = if ($Username) { $Username } else { $parsedUser }
$pwd = if ($Password) { $Password } else { $parsedPassword }

# Move file to temp directory so SQL Server service has permissions to read it
$tempFile = "C:\Users\Public\$($db)_temp_restore.bak"
Write-Host "Dang chuan bi file backup..." -ForegroundColor Yellow
Copy-Item -Path $absoluteBackupFile -Destination $tempFile -Force

Write-Host "--- Bat dau khoi phuc co so du lieu $db ---" -ForegroundColor Cyan
Write-Host "File nguon: $absoluteBackupFile"
Write-Host "Server: $server"
Write-Host "Database dich: $db"
if ($user) {
    Write-Host "User: $user"
}
else {
    Write-Host "Xac thuc: Windows Authentication"
}

# Check if sqlcmd is available
if (-not (Get-Command "sqlcmd" -ErrorAction SilentlyContinue)) {
    Write-Error "Khong tim thay cong cu 'sqlcmd' trong he thong. Vui long cai dat SQL Server Command Line Utilities hoac khoi phuc thu cong bang SSMS."
    # Clean up temp file
    Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    exit 1
}

# Build SQL query to kick out active connections and restore database
$sqlQuery = @"
ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [$db] FROM DISK = N'$tempFile' WITH REPLACE, RECOVERY;
ALTER DATABASE [$db] SET MULTI_USER;
"@

# Prepare arguments
$cmdArgs = @("-S", $server, "-Q", $sqlQuery)
if ($user) {
    $cmdArgs += @("-U", $user, "-P", $pwd)
}
else {
    $cmdArgs += @("-E")
}

Write-Host "Dang khoi phuc tren SQL Server (Co the ngat cac ket noi active)..." -ForegroundColor Yellow
& sqlcmd $cmdArgs

# Clean up temp file
Remove-Item $tempFile -Force -ErrorAction SilentlyContinue

if ($LASTEXITCODE -eq 0) {
    Write-Host "Khoi phuc co so du lieu thanh cong!" -ForegroundColor Green
}
else {
    Write-Error "Khoi phuc that bai. Vui long kiem tra log loi tu sqlcmd."
}
