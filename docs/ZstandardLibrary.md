# Backupport Zstandard Library from dotnet


## Steps to Build

### Create Project

``` shell
cd ./src
dotnet new classlib -n System.IO.Compression.Zstandard
```

### checkout dotnet runtime code
``` shell
 git clone --branch v11.0.0-preview.3.26207.106 --depth 1 https://github.com/dotnet/runtime dotnet11temp
``` shell

### Copy raw Files to new Project
``` shell
$newCodeLocation=""
$dotnetCodePath=""

$ZstandardSrc=Join-Path $dotnetCodePath "src/libraries/System.IO.Compression/src/System/IO/Compression/Zstandard"
$ZstandardDst=Join-Path $newCodeLocation "src/System.IO.Compression.Zstandard"
Remove-Item Join-Path $ZstandardDst "Zstandard.PlatformNotSupported.cs"

```

``` xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
  <ItemGroup>
    <EmbeddedResource Update="Resources\Strings.resx">
      <LogicalName>System.IO.Compression.Zstandard.Resources.Strings.Resources</LogicalName>
    </EmbeddedResource>
  </ItemGroup>
</Project>

```