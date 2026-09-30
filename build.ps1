$publishFolder = "LexBuilder/bin/Release/net10.0/win-x64/publish"

dotnet publish LexBuilder -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true

Copy-Item README.md $publishFolder
Copy-Item Replace-Wubi.ps1 $publishFolder