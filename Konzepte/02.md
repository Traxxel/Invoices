# Aufgabe 02: NuGet-Pakete definieren für alle 4 Projekte

## Ziel
Alle erforderlichen NuGet-Pakete für die 4 Projekte gemäß Konzept definieren.

## NuGet-Pakete nach Projekt

### 1. InvoiceReader.Domain
**Keine externen Pakete** - reine Domain-Logik

### 2. InvoiceReader.Application
**Keine externen Pakete** - nur Domain-Referenz

### 3. InvoiceReader.Infrastructure
**Datei:** `src/InvoiceReader.Infrastructure/InvoiceReader.Infrastructure.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\InvoiceReader.Domain\InvoiceReader.Domain.csproj" />
    <ProjectReference Include="..\InvoiceReader.Application\InvoiceReader.Application.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <!-- ML.NET -->
    <PackageReference Include="Microsoft.ML" Version="3.0.1" />
    <PackageReference Include="Microsoft.ML.Data" Version="3.0.1" />
    
    <!-- PDF Processing -->
    <PackageReference Include="UglyToad.PdfPig" Version="0.1.8" />
    
    <!-- Entity Framework Core -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
    
    <!-- Configuration & DI -->
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="8.0.0" />
    
    <!-- Logging -->
    <PackageReference Include="Serilog" Version="4.0.0" />
    <PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="8.0.0" />
    
    <!-- File System -->
    <PackageReference Include="System.IO.Abstractions" Version="19.1.0" />
  </ItemGroup>
</Project>
```

### 4. InvoiceReader.WinForms
**Datei:** `src/InvoiceReader.WinForms/InvoiceReader.WinForms.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\InvoiceReader.Application\InvoiceReader.Application.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <!-- WinForms Controls -->
    <PackageReference Include="System.Windows.Forms" Version="8.0.0" />
    
    <!-- PDF Viewer (optional) -->
    <PackageReference Include="PdfiumViewer" Version="2.13.0" />
    
    <!-- Configuration -->
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    
    <!-- Logging -->
    <PackageReference Include="Serilog" Version="4.0.0" />
    <PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
  </ItemGroup>
</Project>
```

## Paket-Versionen Begründung

### ML.NET 3.0.1
- Neueste stabile Version für .NET 8
- Vollständige ML.NET Funktionalität

### UglyToad.PdfPig 0.1.8
- Reines .NET PDF-Parser
- Keine externen Dependencies
- Text + Layout-Informationen

### Entity Framework Core 8.0.0
- Neueste Version für .NET 8
- SQL Server Support
- Migration Tools

### Serilog 4.0.0
- Strukturiertes Logging
- Konfigurierbar über appsettings.json
- Console + File Sinks

### System.IO.Abstractions 19.1.0
- Testbare File System Operations
- Abstraktion für File Storage

### PdfiumViewer 2.13.0
- Optional für PDF-Anzeige in WinForms
- Native PDF-Rendering

## Wichtige Hinweise
- Alle Pakete auf neueste stabile Versionen für .NET 8
- Keine Beta/Preview Versionen
- Nur .NET-native Pakete (keine Python/External Services)
- Infrastructure Projekt hat die meisten Dependencies
- WinForms Projekt minimal halten

