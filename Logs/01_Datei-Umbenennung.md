# Log 01: Umbenennung der Konzept-Dateien

## Datum
22. Oktober 2025

## Aufgabe
Alle Markdown-Dateien im Verzeichnis `/Konzepte` von der einfachen Nummerierung (01.md, 02.md, etc.) in aussagekräftige Namen (01-xxx.md) umbenennen, wobei `xxx` eine kurze Beschreibung des Inhalts ist.

## Durchgeführte Aktionen

### Umbenannte Dateien (01-40)

1. **01.md** → **01-Solution-Structure.md** - .NET 8 Projektstruktur mit 4 Layern
2. **02.md** → **02-NuGet-Packages.md** - NuGet-Pakete für alle Projekte
3. **03.md** → **03-appsettings-Configuration.md** - Konfigurationsdateien und -modelle
4. **04.md** → **04-Logging-Infrastructure.md** - Serilog-basierte Logging-Infrastruktur
5. **05.md** → **05-Dependency-Injection.md** - DI-Container-Konfiguration
6. **06.md** → **06-Invoice-Entity.md** - Invoice Domain Entity
7. **07.md** → **07-InvoiceRawBlock-Entity.md** - InvoiceRawBlock Domain Entity
8. **08.md** → **08-Value-Objects.md** - Value Objects (Money, Address, etc.)
9. **09.md** → **09-Domain-Validations-Business-Rules.md** - Domain-Validierungen und Business Rules
10. **10.md** → **10-Domain-Enums-Constants.md** - Domain Enums und Konstanten
11. **11.md** → **11-InvoiceDbContext.md** - Entity Framework DbContext
12. **12.md** → **12-Entity-Configurations.md** - EF Core Fluent API Konfigurationen
13. **13.md** → **13-Migration-Setup.md** - EF Core Migrations Setup
14. **14.md** → **14-Repository-Pattern.md** - Repository Pattern Implementation
15. **15.md** → **15-Database-Seeding.md** - Datenbank-Seeding für Initialdaten
16. **16.md** → **16-FileStorageService.md** - File Storage Service mit GUID-Pfadlogik
17. **17.md** → **17-File-Hash-Integrity-Check.md** - File Hash Service (SHA256)
18. **18.md** → **18-Retention-Policy.md** - Retention Policy Service (optional)
19. **19.md** → **19-PdfPig-Integration.md** - PdfPig Integration für PDF-Parsing
20. **20.md** → **20-Line-Formation-Normalization.md** - Text-Normalisierung und Line Formation
21. **21.md** → **21-Feature-Extraction.md** - ML Feature Extraction Service
22. **22.md** → **22-InputRow-DataView-Schema.md** - ML.NET Datenmodelle und Schema
23. **23.md** → **23-ML-Pipeline-Definition.md** - ML.NET Pipeline für Klassifizierung
24. **24.md** → **24-Model-Training-Storage.md** - ML Model Training Service
25. **25.md** → **25-Prediction-Engine-Model-Loading.md** - ML Prediction Engine Service
26. **26.md** → **26-DTOs.md** - Data Transfer Objects für alle Layer
27. **27.md** → **27-IImportInvoiceUseCase.md** - Import Invoice Use Case
28. **28.md** → **28-IExtractFieldsUseCase.md** - Extract Fields Use Case
29. **29.md** → **29-ISaveInvoiceUseCase.md** - Save Invoice Use Case
30. **30.md** → **30-ITrainModelsUseCase.md** - Train Models Use Case
31. **31.md** → **31-Regex-Patterns.md** - Regex Pattern Service
32. **32.md** → **32-Parser-Helpers.md** - Parser Helper Service
33. **33.md** → **33-MainForm.md** - WinForms Hauptformular
34. **34.md** → **34-ImportDialog.md** - WinForms Import Dialog
35. **35.md** → **35-BatchImportDialog.md** - WinForms Batch Import Dialog
36. **36.md** → **36-ReviewForm.md** - WinForms Review/Edit Formular
37. **37.md** → **37-PDF-Viewer-Control.md** - WinForms PDF Viewer Control
38. **38.md** → **38-Field-Editor-Control.md** - WinForms Field Editor Control
39. **39.md** → **39-TrainingForm.md** - WinForms ML Training Formular
40. **40.md** → **40-SettingsForm.md** - WinForms Settings/Konfiguration Formular

## Ergebnis

✅ **Erfolgreich**: Alle 40 Konzept-Dateien wurden mit aussagekräftigen Namen versehen
✅ **Struktur**: Nummerierung (01-40) beibehalten für logische Reihenfolge
✅ **Lesbarkeit**: Dateinamen beschreiben jetzt klar den Inhalt

## Verzeichnisstruktur nach Umbenennung

```
/Konzepte/
├── 01-Solution-Structure.md
├── 02-NuGet-Packages.md
├── ...
├── 40-SettingsForm.md
├── KONZEPT_improved.md
├── Logs/
└── old/
    ├── InvoiceReader_Konzept_vollständig.md
    └── KONZEPTPROMPT.txt
```

## Nächste Schritte

- Weitere Aufgaben werden in zusätzlichen Log-Dateien (02_xxx.md, 03_xxx.md, etc.) dokumentiert
- Jeder Schritt wird am Ende zusammengefasst für eine gute Gesamtübersicht

