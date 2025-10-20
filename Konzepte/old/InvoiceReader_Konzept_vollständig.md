# Konzept: WinForms-App (.NET 8) zur Erkennung von Rechnungsfeldern aus PDF mit ML.NET & Speicherung via EF Core (lokale SQL Server DB)

## 1) Ziel & Funktionsumfang
- **Ziel:** PDF-Rechnungen einlesen, relevante Felder automatisch erkennen, validieren und in einer lokalen **MS SQL Server**-Datenbank speichern – **ohne Web-API**, reine Desktop-App (WinForms).
- **Zu extrahierende Felder:**  
  1) **Rechnungsnummer**  
  2) **Rechnungsdatum**  
  3) **Ersteller (Firmenadresse)**  
  4) **Gesamtbetrag Netto**  
  5) **Gesamtbetrag MwSt**  
  6) **Gesamtbetrag Brutto**
- **ML-Ansatz:** ML.NET-basierte Klassifikation relevanter Zeilen/Textbereiche + regelbasierte Wert-Extraktion (z. B. Regex) aus den vorhergesagten Kandidaten.
- **PDF-Parsing:** Text + Layout-Infos (Zeilen, Koordinaten). Optional OCR-Fallback bei gescannten PDFs.
- **Persistenz:** EF Core gegen lokale SQL Server-Instanz (z. B. `.\SQLEXPRESS` oder LocalDB).
- **Kein Server/keine API.**

---

## 2) Lösungsarchitektur (High Level)
- **UI (WinForms):** Import-Dialog, Vorschau der extrahierten Felder, manuelle Korrektur, Speichern, Trainingsdaten-Labeling.
- **Application-Layer:** Use-Cases (PDF verarbeiten, Felder extrahieren, speichern, nachtrainieren).
- **Domain-Layer:** Entitäten (Invoice, Address, AmountBreakdown), Validierungslogik, Value Objects.
- **Infrastructure-Layer:**  
  - **PDF-Adapter:** Parser für Text + Layout.  
  - **ML-Adapter:** Laden/Scoren der ML.NET-Modelle, Trainingspipeline.  
  - **EF Core:** DbContext, Migrations, Repositories.
- **Model-/Datenverwaltung:** Versionierte ML-Modelle (`.zip`), annotierte Trainingsdaten (JSON/CSV), Logs.

---

## 3) Projekt-/Solution-Struktur

```plaintext
InvoiceReader.sln
│
├─ src/
│  ├─ InvoiceReader.WinForms/                 # WinForms UI (.NET 8, exe)
│  │  ├─ Forms/
│  │  │  ├─ MainForm.cs
│  │  │  ├─ ImportForm.cs
│  │  │  ├─ ReviewForm.cs                     # Feldvorschau + Korrektur
│  │  │  └─ TrainingForm.cs                   # Labeling/Training aus UI
│  │  ├─ ViewModels/
│  │  ├─ Presenters/                          # optional MVP/MVVM-light
│  │  └─ Program.cs
│  │
│  ├─ InvoiceReader.Application/              # Use-Cases, Services
│  │  ├─ UseCases/
│  │  │  ├─ ImportInvoiceUseCase.cs
│  │  │  ├─ ExtractFieldsUseCase.cs
│  │  │  ├─ SaveInvoiceUseCase.cs
│  │  │  └─ TrainModelsUseCase.cs
│  │  ├─ Interfaces/
│  │  │  ├─ IPdfParser.cs
│  │  │  ├─ IFieldExtractor.cs
│  │  │  ├─ IInvoiceRepository.cs
│  │  │  └─ IModelStore.cs
│  │  └─ DTOs/
│  │
│  ├─ InvoiceReader.Domain/                   # Entitäten & Logik
│  │  ├─ Entities/
│  │  │  ├─ Invoice.cs
│  │  │  ├─ CompanyAddress.cs
│  │  │  └─ AmountBreakdown.cs
│  │  ├─ ValueObjects/
│  │  │  ├─ Money.cs
│  │  │  └─ InvoiceNumber.cs
│  │  └─ Services/
│  │
│  ├─ InvoiceReader.Infrastructure/           # EF Core, ML, PDF
│  │  ├─ Persistence/
│  │  │  ├─ InvoiceDbContext.cs
│  │  │  ├─ EntityTypeConfigurations/
│  │  │  ├─ Migrations/
│  │  │  └─ SqlServerConnectionFactory.cs
│  │  ├─ Pdf/
│  │  │  ├─ PdfPigParser.cs                   # UglyToad.PdfPig o.ä.
│  │  │  └─ OcrFallback.cs                    # optional (Tesseract)
│  │  ├─ Ml/
│  │  │  ├─ FieldClassificationScorer.cs
│  │  │  ├─ FieldClassificationTrainer.cs
│  │  │  ├─ FeatureEngineering.cs
│  │  │  └─ ModelStore.cs
│  │  └─ Logging/
│  │
│  ├─ InvoiceReader.MLConsole/                # optional: CLI Training
│  │  └─ Program.cs
│  │
│  └─ InvoiceReader.Tests/                    # Unit/Integration Tests
│     ├─ Domain.Tests/
│     ├─ Infrastructure.Tests/
│     └─ Application.Tests/
│
├─ data/
│  ├─ samples/                                # Beispiel-PDFs
│  ├─ labeled/                                # Label-Dateien (JSON/CSV)
│  └─ models/
│     ├─ field_classifier_v1.zip
│     └─ model.meta.json
│
└─ docs/
   ├─ dataset_schema.md
   ├─ labeling_guidelines.md
   └─ model_card.md
```

---

## 4) Domänenmodell (EF Core)

**Entitäten & Tabellen**
- `Invoice`  
  - `Id` (PK, GUID)  
  - `InvoiceNumber` (string, index, unique optional)  
  - `InvoiceDate` (date)  
  - `IssuerName` (string)  
  - `IssuerStreet` (string)  
  - `IssuerPostalCode` (string)  
  - `IssuerCity` (string)  
  - `IssuerCountry` (string, nullable)  
  - `NetTotal` (decimal(18,2))  
  - `VatTotal` (decimal(18,2))  
  - `GrossTotal` (decimal(18,2))  
  - `SourceFilePath` (string)  
  - `ImportedAt` (datetime)  
  - `ExtractionConfidence` (float; 0–1)  
  - `ModelVersion` (string)
- (Optional) `InvoiceRawBlock`  
  - `Id` (PK)  
  - `InvoiceId` (FK)  
  - `Page` (int)  
  - `Text` (nvarchar(max))  
  - `X`, `Y`, `Width`, `Height` (float)

**DbContext**
- `DbSet<Invoice> Invoices`  
- `DbSet<InvoiceRawBlock> InvoiceRawBlocks` (optional)

**Migrations & Connection**
- Connection String via `appsettings.json` (WinForms: `user.config`), z. B.:  
  `Server=(localdb)\MSSQLLocalDB;Database=InvoiceReader;Trusted_Connection=True;TrustServerCertificate=True;`

---

## 5) PDF-Parsing & Vorverarbeitung

**Bibliotheken**
- Primär: `UglyToad.PdfPig` (Text + Koordinaten, open-source).  
- Alternativ: `iText7` (kommerziell) oder `PdfSharp` (limitiert).  
- OCR-Fallback (optional): `Tesseract`-NuGet (bei Scans).

**Schritte**
1. **PDF laden → Seiten iterieren.**  
2. **Zeilenbildung:** Gruppiere `Words`/`Letters` zu **logischen Zeilen** inkl. Bounding Box.  
3. **Normierung:** Unicode (NFKC), Whitespace säubern, Ligaturen auflösen, Dezimaltrennzeichen harmonisieren.  
4. **Merkmale (pro Zeile):** Textmerkmale (n-Gramme, Keywords), Layout (X/Y, Seite, Index), Regex-Hinweise (Datum, Beträge, Nummer), Kontext (Nachbarzeilen).  
5. **Kandidatenbildung:** Alle Zeilen sind Kandidaten; ML klassifiziert die **Feldkategorie pro Zeile**.

---

## 6) ML-Konzept (ML.NET)

**Zielbild**  
- **Multiklassen-Klassifikation** pro Zeile: Label ∈ {None, InvoiceNumber, InvoiceDate, IssuerAddress, NetTotal, VatTotal, GrossTotal}.  
- **Wert-Extraktion** je Feld:  
  - **InvoiceNumber:** Regex-Extraktion (alphanumerisch, Präfixe `RE`, `INV`, `2025-...`).  
  - **InvoiceDate:** Regex + Date-Parser (dd.MM.yyyy etc.).  
  - **IssuerAddress:** Aggregation benachbarter Zeilen (mehrzeilig), Adress-Regex/Heuristik.  
  - **Totals:** Betrags-Regex, Währungszeichen, Dezimalerkennung; Tausendertrennzeichen entfernen.

**Feature Engineering**
- `FeaturizeText(Text)` (TF-IDF/WordBag).  
- Numerische Features (X, Y, PageIndex, LineIndex, RegexHitCounts) via `Concatenate`.  
- Optional `NormalizeMinMax`.

**Pipelines (Pseudo)**
- `textFeats = FeaturizeText(Text)`  
- `features = Concatenate(textFeats, X, Y, Page, Line, regex_invoiceNo, regex_date, regex_amount, kw_invoice, kw_total, kw_vat, kw_net, kw_gross, prevTextFeats, nextTextFeats)`  
- Trainer: `SdcaMaximumEntropy` **oder** `LbfgsMaximumEntropy`.

**Trainingsdaten**
- CSV/JSON: `DocumentId, Page, LineIndex, Text, X, Y, Label`  
- Entsteht aus UI-Labeling (TrainingForm) oder separater MLConsole.

**Inference-Flow**
1. PDF → Zeilen + Features.  
2. ML-Modell → pro Zeile **Feldklasse**.  
3. Post-Processing: Regex-Extraktion + Validierung (Datum, Betragskonsistenz).  
4. Confidence-Aggregation: Klassen-Score × Regex-Qualität.  
5. Ergebnis + Confidence + ModelVersion an UI.

---

## 7) WinForms-UI Flows

**Hauptansichten**
- **MainForm:** Liste importierter Rechnungen, Suche/Filter, Buttons: *Importieren*, *Trainieren*, *Einstellungen*.  
- **ImportForm:** PDF auswählen (einzeln/mehrfach), Fortschritt, Ergebnisvorschau (Ampel/Confidence), → „Zur Prüfung öffnen“.  
- **ReviewForm:** Formularfelder editierbar; **Top-3 Kandidaten** pro Feld + Confidence; Seitenvorschau mit Treffer-Markierung; **Speichern**.  
- **TrainingForm:** Labeln (Klasse-DropDown), Trainieren (Metriken), „Modell aktivieren“, Datenexport.

**Einstellungen**
- Pfade (Modelle, Trainingsdaten), Standardspeicherort, DB-Connection String, Sprache/Region, OCR-Fallback On/Off.

---

## 8) Persistenz & Speichern (EF Core)

**Workflow**
1. Nutzer bestätigt/korrektiert Felder in `ReviewForm`.  
2. `SaveInvoiceUseCase` mappt UI-DTO → `Invoice`.  
3. `InvoiceDbContext.SaveChanges()`; setzt `ImportedAt`, `ModelVersion`, `ExtractionConfidence`.  
4. Optional: Rohblöcke/Zeilen mit speichern (Nachvollziehbarkeit).

**Validierung vor Save**
- Pflichtfelder: Nummer, Datum, Brutto.  
- **Kohärenz:** |Netto + MwSt − Brutto| ≤ 0,02.  
- Datum plausibel (nicht weit in der Zukunft).

---

## 9) NuGet-Abhängigkeiten (Vorschlag)
- **ML.NET:** `Microsoft.ML`, optional `Microsoft.ML.AutoML`.  
- **PDF:** `UglyToad.PdfPig`.  
- **OCR (optional):** `Tesseract`.  
- **EF Core:** `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`.  
- **Konfiguration/DI/Logging:** `Microsoft.Extensions.Configuration.Json`, `Microsoft.Extensions.DependencyInjection`, optional `Serilog`.

---

## 10) Konfiguration & Umgebungen
- `appsettings.json` (WinForms) mit DB-Connection & Pfaden (`data/models`, `data/labeled`).  
- Benutzerbezogen: `appsettings.User.json` (git-ignored).  
- **ModelVersioning:** `model.meta.json` mit `version`, `trainedAt`, `trainSetSize`, `metrics` (F1 micro/macro), `notes`.

---

## 11) Qualitätssicherung & Tests
- **Unit-Tests:** Regex-Extraktionen (Datum/Betrag), FeatureEngineering, Post-Processing-Validierungen.  
- **Integrationstests:** End-to-End (PDF → Felder), EF Core gegen Test-DB (LocalDB/InMemory, Schema/Migrationstest gegen echte SQL).  
- **ML-Evaluation:** Hold-out, ggf. Cross-Val; Monitoring auf Drift und periodisches Retraining.

---

## 12) Datenschutz & Nachvollziehbarkeit
- Lokal & offline, keine Übertragung sensibler Daten.  
- Logging: technische Metadaten; Inhalte optional anonymisiert (z. B. Hash der Rechnungsnummer).  
- Reproduzierbarkeit: Trainingsdaten-Snapshot + Modell-Artefakte versionieren.

---

## 13) Performance & Robustheit
- Batch-Import: Hintergrund-Tasks/Queue, UI reaktiv halten.  
- Zeitlimits für Parsing/OCR.  
- Fallback: Bei geringer Confidence → regelbasierte Heuristik.  
- Culture-Handling: `CultureInfo` pro Datei ableiten (`,` vs `.`).

---

## 14) Daten- & Label-Schemas

**`labeled/*.jsonl` (Beispiel)**
```json
{"DocumentId":"A001.pdf","Page":1,"LineIndex":12,"Text":"Rechnungs-Nr.: RE-2025-0172","X":0.12,"Y":0.18,"Label":"InvoiceNumber"}
{"DocumentId":"A001.pdf","Page":1,"LineIndex":13,"Text":"Datum: 14.10.2025","X":0.12,"Y":0.21,"Label":"InvoiceDate"}
{"DocumentId":"A001.pdf","Page":1,"LineIndex":27,"Text":"Netto: 1.234,50 €","X":0.62,"Y":0.77,"Label":"NetTotal"}
```

**`models/model.meta.json`**
```json
{
  "version": "v1.0.0",
  "trainedAt": "2025-10-20T10:30:00Z",
  "trainSetSize": 842,
  "testMetrics": { "microF1": 0.93, "macroF1": 0.91 },
  "notes": "SdcaMaximumEntropy; PdfPig features; regex-assisted extraction"
}
```

---

## 15) Anwendungsfälle (Use-Case Sequenzen)

**UC-01: Einzelimport & Speichern**
1. Nutzer wählt PDF.  
2. System parst Zeilen + Features → ML-Klassifikation → Post-Processing.  
3. ReviewForm zeigt Felder + Confidence + Kandidaten.  
4. Nutzer passt ggf. an → Speichern → EF Core.

**UC-02: Batch-Import**
1. Nutzer wählt mehrere PDFs.  
2. Fortschrittsdialog; Ergebnisse in Tabelle (Status, Confidence).  
3. Doppelklick öffnet Review; Sammel-Speichern möglich.

**UC-03: Labeln & Trainieren**
1. Nutzer markiert in TrainingForm Zeilen als Feldklassen.  
2. Start Training → Metriken anzeigen.  
3. „Modell aktivieren“ setzt `model.zip` als aktiv; Version wird in UI angezeigt.

---

## 16) Fehlerbehandlung & Beobachtbarkeit
- **Duplikate:** Gleiche `InvoiceNumber` → Hinweis, Überspringen/Überschreiben wählen.  
- **Parsing-Fehler:** Log + UI-Badge; OCR-Fallback versuchen (optional).  
- **Model-Status:** Fehlt/alt → Warnung, Fallback auf Regex-Heuristik.

---

## 17) Roadmap (Iterationen)
1. **MVP:** PDF-Parsing, manuelles Ausfüllen, EF-Speichern (ohne ML).  
2. **ML v1:** Zeilenklassifikation + Regex-Extraktion, Review-UI.  
3. **Labeling-UI & Training in-App.**  
4. **OCR-Fallback & Seitenvorschau mit Highlight.**  
5. **Lieferanten-Profile/Schablonen** zur Genauigkeitssteigerung.  
6. **Mehrseiten & Tabellenstrukturen** (optional).

---

## 18) Akzeptanzkriterien (Auszug)
- **Technik:** .NET 8, WinForms, EF Core (SQL Server), ML.NET-Modelle als `.zip`.  
- **Funktional:** 6 Zielfelder extrahierbar + manuell korrigierbar; Speicherung inkl. Audit; Trainingsdaten Import/Export; Modellversion & Metriken sichtbar.  
- **Qualität:** Testset **Micro-F1 ≥ 0,85**; Verarbeitung von **≥ 100 PDFs in < 5 Min** (ohne OCR, hardwareabhängig).

---

## 19) Sicherheits- & Lizenzhinweise
- Lizenzen der PDF/OCR-Bibliotheken prüfen (kommerziell vs OSS).  
- Daten bleiben lokal; optional Verschlüsselung des Datenverzeichnisses.

---

## 20) Build & Deployment
- **Publish** (Self-contained, x64) inkl. `data/models` und `appsettings.User.json`-Template.  
- **DB-Migration** beim ersten Start (Create/Update).  
- **Portable-Variante** möglich (LocalDB erforderlich).

---

## 21) Benennungskonventionen (Beispiele)
- **Namespaces:** `InvoiceReader.{WinForms|Application|Domain|Infrastructure}`  
- **Modelldatei:** `data/models/field_classifier_{version}.zip`  
- **Logs:** `logs/{yyyy-MM-dd}.log`

---

## 22) Offene Punkte (erweiterbar)
- **Adresszusammenführung:** Heuristik zur Mehrzeilen-Adressrekonstruktion (IssuerAddress).  
- **Währungen:** EUR Standard, Erweiterung auf weitere Währungen.  
- **Mehrsprachigkeit:** Deutsch/Englisch Labels & Regex-Wortlisten.

---

> **Ergebnis:** Dieses Konzept liefert eine klar strukturierte, **feingliederige** Vorlage, sodass eine nachgeschaltete KI die WinForms-App unter .NET 8 mit ML.NET-Feldklassifikation, PDF-Parsing und EF Core-Persistenz automatisiert erstellen kann.
