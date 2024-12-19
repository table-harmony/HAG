$ErrorActionPreference = 'Stop'

# Define installation paths and URLs
$appName = "HagViewer"
$installDir = "$env:LOCALAPPDATA\$appName"
$downloadUrl = "https://github.com/tableharmony/HAG/releases/latest/download/HagViewer.exe"
$exePath = "$installDir\HagViewer.exe"

Write-Host "Installing $appName..."

# Create installation directory
New-Item -ItemType Directory -Force -Path $installDir | Out-Null

# Download the executable
Write-Host "Downloading application..."
Invoke-WebRequest -Uri $downloadUrl -OutFile $exePath

# Create desktop shortcut
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\$appName.lnk")
$Shortcut.TargetPath = $exePath
$Shortcut.Save()

# Register file association for .hag files
Write-Host "Setting up file associations..."
New-Item -Path "HKCU:\Software\Classes\.hag" -Force | Out-Null
Set-ItemProperty -Path "HKCU:\Software\Classes\.hag" -Name "(Default)" -Value "HagViewer.File"
New-Item -Path "HKCU:\Software\Classes\HagViewer.File" -Force | Out-Null
New-Item -Path "HKCU:\Software\Classes\HagViewer.File\shell\open\command" -Force | Out-Null
Set-ItemProperty -Path "HKCU:\Software\Classes\HagViewer.File\shell\open\command" -Name "(Default)" -Value "`"$exePath`" `"%1`""

Write-Host "Installation complete! You can now open .hag files with $appName" 