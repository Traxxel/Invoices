# Aufgabe 01: Solution-Struktur und Projektdateien erstellen

## Ziel
Erstelle die komplette .NET 8 Solution-Struktur mit allen 4 Projekten gemäß dem Konzept.

## Projektstruktur
```
Invoice.sln
│
├─ src/
│  ├─ Invoice.WinForms/           # UI Layer
│  │  ├─ Invoice.WinForms.csproj
│  │  ├─ Program.cs
│  │  └─ Forms/
│  │
│  ├─ Invoice.Application/        # Application Layer
│  │  ├─ Invoice.Application.csproj
│  │  ├─ UseCases/
│  │  ├─ DTOs/
│  │  └─ Interfaces/
│  │
│  ├─ Invoice.Domain/             # Domain Layer
│  │  ├─ Invoice.Domain.csproj
│  │  ├─ Entities/
│  │  ├─ ValueObjects/
│  │  └─ Enums/
│  │
│  └─ Invoice.Infrastructure/     # Infrastructure Layer
│     ├─ Invoice.Infrastructure.csproj
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
**Datei:** `Invoice.sln`
- .NET 8 Solution
- Alle 4 Projekte als Solution Items
- Solution Folders für bessere Organisation

### 2. WinForms Projekt
**Datei:** `src/Invoice.WinForms/Invoice.WinForms.csproj`
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
    <ProjectReference Include="..\Invoice.Application\Invoice.Application.csproj" />
  </ItemGroup>
</Project>
```

**Datei:** `src/Invoice.WinForms/Program.cs`
- Standard WinForms Program.cs
- Dependency Injection Setup
- Application.Run mit MainForm

### 3. Application Projekt
**Datei:** `src/Invoice.Application/Invoice.Application.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\Invoice.Domain\Invoice.Domain.csproj" />
  </ItemGroup>
</Project>
```

### 4. Domain Projekt
**Datei:** `src/Invoice.Domain/Invoice.Domain.csproj`
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
**Datei:** `src/Invoice.Infrastructure/Invoice.Infrastructure.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\Invoice.Domain\Invoice.Domain.csproj" />
    <ProjectReference Include="..\Invoice.Application\Invoice.Application.csproj" />
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

