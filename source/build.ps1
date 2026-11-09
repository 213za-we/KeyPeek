$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$pkg = Join-Path $root 'build\_sdk\pkg'
$lib = Join-Path $pkg 'lib\net462'

Write-Host "[1/4] 复制 WebView2 运行库"
Copy-Item -LiteralPath (Join-Path $lib 'Microsoft.Web.WebView2.Core.dll') -Destination $root -Force
Copy-Item -LiteralPath (Join-Path $lib 'Microsoft.Web.WebView2.WinForms.dll') -Destination $root -Force
Copy-Item -LiteralPath (Join-Path $pkg 'runtimes\win-x64\native\WebView2Loader.dll') -Destination $root -Force

Write-Host "[2/4] 生成图标"
$icoPath = Join-Path $root 'build\app.ico'
if (-not (Test-Path -LiteralPath $icoPath)) {
    Add-Type -AssemblyName System.Drawing
    $bmp = New-Object System.Drawing.Bitmap 64, 64
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(14, 124, 102))
    $g.FillEllipse($brush, 4, 4, 56, 56)
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), 5
    $g.DrawLine($pen, 32, 44, 32, 24)
    $g.DrawLine($pen, 32, 24, 22, 34)
    $g.DrawLine($pen, 32, 24, 42, 34)
    $g.Dispose()
    $icon = [System.Drawing.Icon]::FromHandle($bmp.GetHicon())
    $fs = [System.IO.File]::Create($icoPath)
    $icon.Save($fs)
    $fs.Close()
    $bmp.Dispose()
    Write-Host "      图标已生成"
} else { Write-Host "      图标已存在，跳过" }

Write-Host "[3/4] 编译"
$out = Join-Path $root 'KeyPeek.exe'
if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Force }
$refs = @(
    (Join-Path $root 'Microsoft.Web.WebView2.Core.dll'),
    (Join-Path $root 'Microsoft.Web.WebView2.WinForms.dll'),
    'System.dll', 'System.Core.dll', 'System.Drawing.dll', 'System.Windows.Forms.dll'
)
$args = @(
    '/nologo', '/target:winexe', '/platform:x64', '/optimize', '/langversion:5', '/codepage:65001',
    ('/out:' + $out),
    ('/win32icon:' + $icoPath),
    ('/reference:' + $refs[0]), ('/reference:' + $refs[1]),
    '/reference:System.dll', '/reference:System.Core.dll',
    '/reference:System.Drawing.dll', '/reference:System.Windows.Forms.dll',
    (Join-Path $root 'source\Launcher.cs')
)
& $csc @args
if ($LASTEXITCODE -ne 0) { throw "编译失败，退出码 $LASTEXITCODE" }

Write-Host "[4/4] 结果"
Get-ChildItem -LiteralPath $root -Filter *.exe | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
Get-ChildItem -LiteralPath $root -Filter *.dll | Select-Object Name, Length | Format-Table -AutoSize
