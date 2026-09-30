$ErrorActionPreference = "Stop"

$wubiNew = "$env:windir\InputMethod\CHS\ChsWubiNew.lex"

takeown /f $wubiNew
icacls $wubiNew /grant Administrators:F

Remove-Item $wubiNew -Force -ErrorAction SilentlyContinue
$wubiNew98 = Join-Path $PSScriptRoot "ChsWubiNew.lex"
Copy-Item $wubiNew98 $wubiNew -Force
