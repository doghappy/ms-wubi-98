$publishFolder = "LexBuilder/bin/Release/net10.0/win-x64/publish"

dotnet publish LexBuilder -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true

Copy-Item README.md $publishFolder
Copy-Item Replace-Wubi.ps1 $publishFolder
Copy-Item "LexBuilder/98五笔含词表-【单义】.txt" $publishFolder

Compress-Archive -Path "$publishFolder/*" -DestinationPath "LexBuilder/bin/Release/win-x64.zip" -Force