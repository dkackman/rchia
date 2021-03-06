$version = "0.9.1"

# clean build and publish outputs
Remove-Item '.\rchia\bin' -Recurse -Force
Remove-Item '.\publish' -Recurse -Force

# stand alone
dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/standalone/win-x64 --self-contained True --runtime win-x64 --verbosity Normal /property:PublishTrimmed=True /property:PublishSingleFile=True /property:IncludeNativeLibrariesForSelfExtract=True /property:DebugType=None /property:DebugSymbols=False
dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/standalone/linux-x64 --self-contained True --runtime linux-x64 --verbosity Normal /property:PublishTrimmed=True /property:PublishSingleFile=True /property:IncludeNativeLibrariesForSelfExtract=True /property:DebugType=None /property:DebugSymbols=False
dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/standalone/osx.11.0-x64 --self-contained True --runtime osx.11.0-x64 --verbosity Normal /property:PublishTrimmed=True /property:PublishSingleFile=True /property:IncludeNativeLibrariesForSelfExtract=True /property:DebugType=None /property:DebugSymbols=False
dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/standalone/linux-arm64 --self-contained True --runtime linux-arm64 --verbosity Normal /property:PublishTrimmed=True /property:PublishSingleFile=True /property:IncludeNativeLibrariesForSelfExtract=True /property:DebugType=None /property:DebugSymbols=False

Compress-Archive -CompressionLevel Optimal -Path publish/standalone/win-x64/* -DestinationPath publish/rchia-$version-standalone-win-x64.zip
Compress-Archive -CompressionLevel Optimal -Path publish/standalone/linux-x64/* -DestinationPath publish/rchia-$version-standalone-linux-x64.zip
Compress-Archive -CompressionLevel Optimal -Path publish/standalone/osx.11.0-x64/* -DestinationPath publish/rchia-$version-standalone-osx.11.0-x64.zip
Compress-Archive -CompressionLevel Optimal -Path publish/standalone/linux-arm64/* -DestinationPath publish/rchia-$version-standalone-linux-arm64.zip

# single file but requires dotnet
dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/singlefile/win-x64 --self-contained False --runtime win-x64 --verbosity Normal /property:PublishSingleFile=True /property:IncludeNativeLibrariesForSelfExtract=True /property:DebugType=None /property:DebugSymbols=False
dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/singlefile/linux-x64 --self-contained False --runtime linux-x64 --verbosity Normal /property:PublishSingleFile=True /property:IncludeNativeLibrariesForSelfExtract=True /property:DebugType=None /property:DebugSymbols=False
dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/singlefile/osx.11.0-x64 --self-contained False --runtime osx.11.0-x64 --verbosity Normal /property:PublishSingleFile=True /property:IncludeNativeLibrariesForSelfExtract=True /property:DebugType=None /property:DebugSymbols=False
dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/singlefile/linux-arm64 --self-contained False --runtime linux-arm64 --verbosity Normal /property:PublishSingleFile=True /property:IncludeNativeLibrariesForSelfExtract=True /property:DebugType=None /property:DebugSymbols=False

Compress-Archive -CompressionLevel Optimal -Path publish/singlefile/win-x64/* -DestinationPath publish/rchia-$version-singlefile-win-x64.zip
Compress-Archive -CompressionLevel Optimal -Path publish/singlefile/linux-x64/* -DestinationPath publish/rchia-$version-singlefile-linux-x64.zip
Compress-Archive -CompressionLevel Optimal -Path publish/singlefile/osx.11.0-x64/* -DestinationPath publish/rchia-$version-singlefile-osx.11.0-x64.zip
Compress-Archive -CompressionLevel Optimal -Path publish/singlefile/linux-arm64/* -DestinationPath publish/rchia-$version-singlefile-linux-arm64.zip

# files
# dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/files/win-x64 --self-contained False --runtime win-x64 --verbosity Normal /property:PublishSingleFile=False /property:DebugType=None /property:DebugSymbols=False
# dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/files/linux-x64 --self-contained False --runtime linux-x64 --verbosity Normal /property:PublishSingleFile=False /property:DebugType=None /property:DebugSymbols=False
# dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/files/osx.11.0-x64 --self-contained False --runtime osx.11.0-x64 --verbosity Normal  /property:PublishSingleFile=False /property:DebugType=None /property:DebugSymbols=False
# dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/files/linux-arm64 --self-contained False --runtime linux-arm64 --verbosity Normal /property:PublishSingleFile=False /property:DebugType=None /property:DebugSymbols=False
dotnet publish ./rchia/rchia.csproj --configuration Release --framework net6.0 --output publish/files/any-cpu --self-contained False --verbosity Normal /property:PublishSingleFile=False /property:DebugType=None /property:DebugSymbols=False

# Compress-Archive -CompressionLevel Optimal -Path publish/files/win-x64/* -DestinationPath publish/rchia-$version-files-win-x64.zip
# Compress-Archive -CompressionLevel Optimal -Path publish/files/linux-x64/* -DestinationPath publish/rchia-$version-files-linux-x64.zip
# Compress-Archive -CompressionLevel Optimal -Path publish/files/osx.11.0-x64/* -DestinationPath publish/rchia-$version-files-osx.11.0-x64.zip
# Compress-Archive -CompressionLevel Optimal -Path publish/files/linux-arm64/* -DestinationPath publish/rchia-$version-files-linux-arm64.zip
Compress-Archive -CompressionLevel Optimal -Path publish/files/any-cpu/* -DestinationPath publish/rchia-$version-files-any-cpu.zip

#nuget
Copy-Item ./rchia/bin/release/rchia.$version.nupkg -Destination ./publish
