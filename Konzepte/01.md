# Aufgabe 01: Solution-Struktur und Projektdateien erstellen

## Ziel
Erstelle die komplette .NET 8 Solution-Struktur mit allen 4 Projekten gemäß dem Konzept.

## Projektstruktur
```
InvoiceReader.sln
│
├─ src/
│  ├─ InvoiceReader.WinForms/           # UI Layer
│  │  ├─ InvoiceReader.WinForms.csproj
│  │  ├─ Program.cs
│  │  └─ Forms/
│  │
│  ├─ InvoiceReader.Application/        # Application Layer
│  │  ├─ InvoiceReader.Application.csproj
│  │  ├─ UseCases/
│  │  ├─ DTOs/
│  │  └─ Interfaces/
│  │
│  ├─ InvoiceReader.Domain/             # Domain Layer
│  │  ├─ InvoiceReader.Domain.csproj
│  │  ├─ Entities/
│  │  ├─ ValueObjects/
│  │  └─ Enums/
│  │
│  └─ InvoiceReader.Infrastructure/     # Infrastructure Layer
│     ├─ InvoiceReader.Infrastructure.csproj
│     ├─ Data/
│     ├─ Services/
│     └─ ML/
│
├─ data/
│  ├─ samples/                          # Beispiel-PDFs
│  ├─ labeled/                          # Trainingslabels
│  └─ models/                           # ML-Modelle
│
└─ docs/                                # Doku, Leitfäden
```

## Zu erstellende Dateien

### 1. Solution-Datei
**Datei:** `InvoiceReader.sln`
- .NET 8 Solution
- Alle 4 Projekte als Solution Items
- Solution Folders für bessere Organisation

### 2. WinForms Projekt
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
</Project>
```

**Datei:** `src/InvoiceReader.WinForms/Program.cs`
- Standard WinForms Program.cs
- Dependency Injection Setup
- Application.Run mit MainForm

### 3. Application Projekt
**Datei:** `src/InvoiceReader.Application/InvoiceReader.Application.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\InvoiceReader.Domain\InvoiceReader.Domain.csproj" />
  </ItemGroup>
</Project>
```

### 4. Domain Projekt
**Datei:** `src/InvoiceReader.Domain/InvoiceReader.Domain.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### 5. Infrastructure Projekt
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
</Project>
```

## Verzeichnisse erstellen
- Alle leeren Verzeichnisse gemäß Struktur anlegen
- `data/samples/`, `data/labeled/`, `data/models/` erstellen
- `docs/` Verzeichnis anlegen

## Wichtige Hinweise
- Alle Projekte auf .NET 8
- WinForms Projekt als Windows-spezifisch markieren
- Korrekte Projekt-Referenzen setzen
- Solution Folders für bessere Organisation verwenden
- Alle Verzeichnisse leer anlegen (werden in späteren Aufgaben gefüllt)

