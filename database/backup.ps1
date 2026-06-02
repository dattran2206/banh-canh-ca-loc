param(
    [string]$ServerName,
    [string]$Username,
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForSharedSecuredInformation", "Password")]
    [string]$Password,
    [string]$DatabaseName = "BanhCanhCaLoc"
)

# Determine script root
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition

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
    } catch {
        Write-Warning "Khong the doc cau hinh ket noi tu appsettings.json. Se dung cau hinh mac dinh."
    }
}

# Override with parameters or fallback to parsed values
$server = if ($ServerName) { $ServerName } else { $parsedServer }
$db = if ($DatabaseName) { $DatabaseName } else { $parsedDb }
$user = if ($Username) { $Username } else { $parsedUser }
$pwd = if ($Password) { $Password } else { $parsedPassword }

# Generate filename
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = Join-Path $scriptRoot "backups"
$destFile = Join-Path $backupDir "$($db)_$timestamp.bak"
$tempFile = "C:\Users\Public\$($db)_temp_backup.bak"

Write-Host "--- Bat dau sao luu co so du lieu $db ---" -ForegroundColor Cyan
Write-Host "Server: $server"
Write-Host "Database: $db"
if ($user) {
    Write-Host "User: $user"
} else {
    Write-Host "Xac thuc: Windows Authentication"
}

# Build sqlcmd arguments
$sqlQuery = "BACKUP DATABASE [$db] TO DISK = N'$tempFile' WITH FORMAT, INIT, NAME = N'$db-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10;"

# Check if sqlcmd is available
if (-not (Get-Command "sqlcmd" -ErrorAction SilentlyContinue)) {
    Write-Error "Khong tim thay cong cu 'sqlcmd' trong he thong. Vui long cai dat SQL Server Command Line Utilities hoac thuc hien backup thu cong bang SSMS."
    exit 1
}

# Prepare arguments
$cmdArgs = @("-S", $server, "-Q", $sqlQuery)
if ($user) {
    $cmdArgs += @("-U", $user, "-P", $pwd)
} else {
    $cmdArgs += @("-E")
}

# Execute backup
Write-Host "Dang thuc thi lenh sao luu tren SQL Server..." -ForegroundColor Yellow
& sqlcmd $cmdArgs

if ($LASTEXITCODE -eq 0) {
    if (Test-Path $tempFile) {
        # Ensure backup folder exists
        if (-not (Test-Path $backupDir)) {
            New-Item -ItemType Directory -Path $backupDir | Out-Null
        }
        
        # Move to project backup folder
        Move-Item -Path $tempFile -Destination $destFile -Force
        Write-Host "Sao luu thanh cong!" -ForegroundColor Green
        Write-Host "File backup da luu tai: $destFile" -ForegroundColor Green
    } else {
        Write-Error "Khong tim thay file backup tam thoi tai $tempFile."
    }
} else {
    Write-Error "Lenh backup that bai. Vui long kiem tra quyen truy cap hoac cau hinh ket noi SQL Server."
}
