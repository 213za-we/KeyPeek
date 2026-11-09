# 从本机 IDEA 安装目录的插件 jar 里抽官方 Live Templates，合并进 packs
# 只做“读官方 XML -> 转条目 -> 追加到目标 pack”，不改动已有条目
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$ideaRoot = 'D:\AppInst\IntelliJ IDEA 2026.1.2'
$packsRoot = 'E:\DouBao\0917\KeyPeek\packs'

function Get-Templates($jarName, $entryName) {
  $jar = Get-ChildItem -LiteralPath $ideaRoot -Recurse -File -Filter $jarName -ErrorAction SilentlyContinue | Select-Object -First 1
  if (-not $jar) { throw "找不到 $jarName" }
  $zip = [IO.Compression.ZipFile]::OpenRead($jar.FullName)
  try {
    $e = $zip.GetEntry($entryName)
    if (-not $e) { throw "找不到 $entryName（$jarName）" }
    $sr = New-Object IO.StreamReader($e.Open(), [Text.Encoding]::UTF8)
    $xml = [xml]$sr.ReadToEnd()
    $sr.Close()
    return ,$xml.SelectNodes('//template')
  } finally { $zip.Dispose() }
}

function Clean-Code($v) {
  if ($null -eq $v) { return '' }
  $s = [string]$v
  $s = $s -replace '\$END\$', ''
  $s = $s -replace "`r`n", "`n"
  return $s.TrimEnd()
}

function Sanitize($n) {
  $s = ([string]$n).ToLowerInvariant()
  $s = [regex]::Replace($s, '[^a-z0-9]+', '-')
  return $s.Trim('-')
}

# 目标包 / 语言 / 组名 / id 前缀
$specs = @(
  @{ pack = 'webstorm.json'; jar = 'intellij.vuejs.backend.jar';        entry = 'liveTemplates/Vue.xml';                 lang = 'Vue';            prefix = 'ws.vue' },
  @{ pack = 'webstorm.json'; jar = 'intellij.angular.backend.jar';      entry = 'liveTemplates/Angular2.xml';            lang = 'Angular';        prefix = 'ws.ng' },
  @{ pack = 'webstorm.json'; jar = 'intellij.react.ultimate.jar';       entry = 'liveTemplates/react.xml';               lang = 'React';          prefix = 'ws.react' },
  @{ pack = 'webstorm.json'; jar = 'intellij.react.ultimate.jar';       entry = 'liveTemplates/react-hooks.xml';         lang = 'React Hooks';    prefix = 'ws.hooks' },
  @{ pack = 'webstorm.json'; jar = 'javascript-plugin.jar';             entry = 'liveTemplates/javascript_testing.xml';  lang = 'JS 测试';        prefix = 'ws.jstest' },
  @{ pack = 'webstorm.json'; jar = 'javascript-plugin.jar';             entry = 'liveTemplates/javascript.xml';          lang = 'JavaScript';     prefix = 'ws.js' },
  @{ pack = 'idea.json';     jar = 'intellij.database.sql.impl.jar';    entry = 'liveTemplates/sqlLiveTemplates.xml';    lang = 'SQL';            prefix = 'idea.sql' },
  @{ pack = 'idea.json';     jar = 'intellij.sh.backend.jar';           entry = 'liveTemplates/ShellScript.xml';         lang = 'Shell';          prefix = 'idea.sh' }
)

foreach ($sp in $specs) {
  $path = Join-Path $packsRoot $sp.pack
  $pack = Get-Content -LiteralPath $path -Raw -Encoding utf8 | ConvertFrom-Json
  $group = '代码模板·' + $sp.lang

  # 已有 id / 已有“组+缩写”都跳过，保证可重复执行
  $ids = @{}
  $keysInGroup = @{}
  foreach ($s in $pack.shortcuts) {
    $ids[$s.id] = 1
    if ($s.group -eq $group -and $s.keys.Count -ge 1) { $keysInGroup[($s.keys -join '+')] = 1 }
  }

  $nodes = Get-Templates $sp.jar $sp.entry
  $added = 0
  $skipped = 0
  foreach ($t in $nodes) {
    $name = ([string]$t.name).Trim()
    if (-not $name) { continue }
    if ($keysInGroup.ContainsKey($name)) { $skipped++; continue }
    $suffix = Sanitize $name
    if (-not $suffix) { $skipped++; continue }
    $id = $sp.prefix + '.' + $suffix
    if ($ids.ContainsKey($id)) { $skipped++; continue }

    $desc = ([string]$t.description).Trim()
    $code = Clean-Code $t.value
    $entry = [ordered]@{
      id      = $id
      action  = $(if ($desc) { $desc } else { $name })
      aliases = @($name, $desc) | Where-Object { $_ } | ForEach-Object { $_ }
      keys    = @($name)
      group   = $group
    }
    if ($desc) { $entry['note'] = $desc }
    if ($code) { $entry['code'] = $code }

    $pack.shortcuts += [pscustomobject]$entry
    $ids[$id] = 1
    $keysInGroup[$name] = 1
    $added++
  }

  $json = $pack | ConvertTo-Json -Depth 10
  [IO.File]::WriteAllText($path, $json, (New-Object Text.UTF8Encoding($false)))
  "$($sp.pack)  $group  新增 $added  跳过 $skipped  现在共 $($pack.shortcuts.Count) 条"
}
