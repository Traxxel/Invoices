
# KONZEPT.md — WinForms-Rechnungsleser (.NET 8, ML.NET, EF Core, SQL Server)

> Stand: 2025-10-20 17:20

Dieses Konzept dient als **vollständige, feingliederige Vorlage**, damit eine KI daraus ein funktionsfähiges .NET‑Projekt (Windows‑Desktop/WinForms) erzeugen kann. **Es werden ausschließlich .NET‑Komponenten/NuGets** verwendet (keine Python‑Skripte, keine externen Services).

---

## 1. Zielbild & Umfang
- **Ziel:** PDF‑Rechnungen importieren, Felder per ML.NET extrahieren, manuell prüfen, in SQL Server speichern, PDF im Dateisystem ablegen (Verweis in DB).
- **Extraktionsfelder:** Rechnungsnummer, Rechnungsdatum, Ersteller (Firma/Adresse), Gesamtbeträge: Netto, MwSt, Brutto.
- **Out‑of‑Scope:** Web‑API/Cloud‑OCR. Optionaler OCR‑Fallback nur, wenn über **reines .NET‑NuGet** (z. B. kommerzielles .NET‑OCR) verfügbar; default **ohne OCR**.

## 2. Architektur (Schichten)
- **UI (WinForms .NET 8):** Import, Review (PDF vs. erkannte Daten), Training/Labeling, Einstellungen.
- **Application:** Use‑Cases als Services; orchestriert Parser, ML, Repository.
- **Domain:** Entitäten/Value Objects, Validierungen, Policies.
- **Infrastructure:** PDF‑Parser, ML.NET‑Pipelines, EF Core (SQL Server), FileStorage, Logging/Config.
- **Artefakte:** Modelle (*.zip), Trainingsdaten (*.jsonl/*.csv), Logs, Settings.

## 3. Projektstruktur (Solution)
```
Invoice.sln
│
├─ src/
│  ├─ Invoice.WinForms/           # UI
│  ├─ Invoice.Application/        # UseCases/Ports
│  ├─ Invoice.Domain/             # Entities/VO/Policies
│  └─ Invoice.Infrastructure/     # EF/PDF/ML/FileStorage
│
├─ tools/                               # (leer; keine Python-Tools)
├─ data/
│  ├─ samples/                          # Beispiel-PDFs
│  ├─ labeled/                          # Trainingslabels
│  └─ models/                           # ML-Modelle
└─ docs/                                # Doku, Leitfäden
```

## 4. NuGet‑Pakete (nur .NET)
- **ML.NET:** `Microsoft.ML` (+ `Microsoft.ML.Data`)
- **PDF:** `UglyToad.PdfPig` (reines .NET; Text + Layout)
- **EF Core:** `Microsoft.EntityFrameworkCore`, `...SqlServer`, `...Tools`
- **Config/DI:** `Microsoft.Extensions.*`
- **Logging:** `Serilog` (optional, rein .NET)

## 5. Domänenmodell
**Invoice**
- Id (Guid, PK), InvoiceNumber (idx, unique?), InvoiceDate (DateOnly)
- IssuerName, IssuerStreet, IssuerPostalCode, IssuerCity, IssuerCountry?
- NetTotal, VatTotal, GrossTotal (decimal(18,2))
- SourceFilePath, ImportedAt (DateTime), ExtractionConfidence (float), ModelVersion (string)

**InvoiceRawBlock (optional, Nachvollziehbarkeit)**
- Id, InvoiceId (FK), Page, Text (nvarchar(max)), X, Y, Width, Height (float)

## 6. EF Core — Konfiguration
- **DbContext:** `InvoiceDbContext`
- **DbSets:** `Invoices`, `InvoiceRawBlocks`
- **Fluent:** Decimal‑Präzision, Unique‑Index auf InvoiceNumber (optional, abhängig vom Use‑Case).
- **ConnectionString (Beispiel):**
  `Server=(localdb)\MSSQLLocalDB;Database=Invoice;Trusted_Connection=True;TrustServerCertificate=True;`
- **Migrationen:** beim App‑Start `Database.Migrate()`.

## 7. File‑Storage‑Strategie (PDFs)
- **Root:** `storage\invoices\{yyyy}\{MM}\`
- **Ablage:** `{Guid}.pdf`
- **DB‑Verweis:** `SourceFilePath` = relativer Pfad
- **Integrität:** optional `FileHash` (SHA256) + Größe (Bytes)
- **Aufräumen/Retention:** optional Policy (z. B. 10 Jahre)

## 8. PDF‑Parsing (ohne OCR)
- **Lib:** PdfPig (Words/Letters → Zeilen mit Bounding‑Box)
- **Normalisierung:** Unicode NFKC, Whitespace trimmen, Ligaturen lösen, Dezimaltrenner harmonisieren
- **Zeilen‑Features:** Text, Seite, X/Y, LineIndex, Regex‑Hits (Datum, Betrag, Nummer), Kontext (Vor/Nachbarzeile)

## 9. ML‑Problemstellung
- **Task:** Multiklassen‑Klassifikation **pro Zeile**:  
  {{None, InvoiceNumber, InvoiceDate, IssuerAddress, NetTotal, VatTotal, GrossTotal}}
- **Post‑Processing:** Wert‑Extraktion je Klasse via Regex/Parser, Validierung/Kohärenzcheck.

## 10. ML.NET‑Pipeline (Pseudocode)
```csharp
var ctx = new MLContext(seed: 42);
var data = ctx.Data.LoadFromTextFile<InputRow>(path, hasHeader: true, separatorChar: '\t');
var pipeline =
    ctx.Transforms.Text.FeaturizeText("TextFeats", nameof(InputRow.Text))
    .Append(ctx.Transforms.Concatenate("Features",
        "TextFeats", nameof(InputRow.X), nameof(InputRow.Y),
        nameof(InputRow.Page), nameof(InputRow.LineIndex),
        nameof(InputRow.RegexDateHits), nameof(InputRow.RegexAmountHits),
        nameof(InputRow.RegexNumberHits)))
    .Append(ctx.Transforms.Conversion.MapValueToKey("Label"))
    .Append(ctx.MulticlassClassification.Trainers.SdcaMaximumEntropy())
    .Append(ctx.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
var model = pipeline.Fit(data);
ctx.Model.Save(model, data.Schema, "data/models/field_classifier_v1.zip");
```

## 11. Trainingsdaten — Struktur & Erstellung
- **Dateiformate:** `labeled/*.jsonl` oder `*.tsv`
- **Schema (jsonl, pro Zeile):**
  ```json
  {{
    "DocumentId":"A001.pdf","Page":1,"LineIndex":12,"Text":"Rechnungs-Nr.: RE-2025-0172",
    "X":0.12,"Y":0.18,"Label":"InvoiceNumber"
  }}
  ```
- **Label‑Definitionen:** exakt o. g. Klassen (keine Mischlabels). IssuerAddress kann **mehrzeilig** sein → mehrere Zeilen mit identischem Label.
- **Sampling:** mind. 20–30 Lieferanten, 300–1000 Dokumente, ausgewogene Klassen (Down/Up‑Sampling bei Bedarf).
- **Splits:** 80/10/10 (Train/Val/Test), Dokument‑weise splitten (Leckage vermeiden).

## 12. Labeling‑Guidelines (Kurzfassung)
1. **Zeilenbildung prüfen** (kein Wort‑Salat, ggf. Zeilen heuristisch mergen).
2. **InvoiceNumber:** nur Zeile mit der Nummer (ggf. Präfix „RE/INV/…“).
3. **InvoiceDate:** nur die Zeile mit eindeutigem Belegdatum (nicht Lieferdatum).
4. **IssuerAddress:** Block oberhalb/links mit Firmenname + Adresse (mehrzeilig labeln).
5. **Totals:** Bevorzugt die **Summenzeile** (Netto/MwSt/Brutto); Währung in derselben Zeile.
6. **Unklar?** → `None` labeln (verhindert Rauschen).

## 13. Regex‑Bibliothek (Beispiele)
- **InvoiceNumber:** `(?i)\b(RE|INV|RG|RNR|Rechnungs(?:-)?Nr\.?)\s*[:\-]?\s*([A-Z0-9\-\/\.]{{4,}})`
- **InvoiceDate (DE):** `\b(0?[1-9]|[12][0-9]|3[01])\.(0?[1-9]|1[0-2])\.(19|20)\d\d\b`
- **Amount:** `[-+]?\d{{1,3}}(\.\d{{3}})*,\d{{2}}` (DE‑Format), Währung `€|EUR`

## 14. Post‑Processing & Validierung
- **Konfliktauflösung:** Highest confidence pro Feld; bei Krawall → Top‑3 Kandidaten im UI.
- **Betragskonsistenz:** |Netto + MwSt − Brutto| ≤ 0,02
- **Datum:** nicht in der Zukunft (> +7 Tage)
- **Adressblock:** bis zur Leerzeile aggregieren; Name erste Zeile.

## 15. Use‑Cases (Sequenzen)
- **UC‑01 Import:** Datei wählen → Parser → ML → Review → Speichern (EF + FileStore).
- **UC‑02 Batch‑Import:** Mehrere PDFs, Fortschritt, Statusliste, Doppelklick → Review.
- **UC‑03 Review:** PDF‑Viewer links, editierbare Felder rechts, Kandidaten‑Dropdown, Confidence‑Badges.
- **UC‑04 Training:** Labeling‑Grid (Zeile/Klasse), Train‑Button, Metriken, „Modell aktivieren“.
- **UC‑05 Duplikatbehandlung:** Warnung bei gleicher Rechnungsnummer, Auswahl: Überspringen/Anfügen/Aktualisieren.

## 16. Öffentliche Interfaces (Application Layer)
```csharp
public interface IImportInvoiceUseCase { ImportResult Execute(string pdfPath); }
public interface IExtractFieldsUseCase { ExtractionResult Execute(ParsedDocument doc); }
public interface ISaveInvoiceUseCase { Guid Execute(InvoiceDto dto, string sourcePdfAbsolutePath); }
public interface ITrainModelsUseCase { TrainingReport Execute(TrainingOptions options); }
```
**DTOs (Auszug)**
```csharp
public record InvoiceDto(
    string InvoiceNumber, DateOnly InvoiceDate,
    string IssuerName, string IssuerStreet, string IssuerPostalCode, string IssuerCity, string? IssuerCountry,
    decimal NetTotal, decimal VatTotal, decimal GrossTotal,
    float ExtractionConfidence, string ModelVersion);
```

## 17. WinForms‑Screens (Funktionale Anforderungen)
- **MainForm:** Grid (Invoices), Suchfeld, Buttons: Import… | Batch‑Import… | Trainieren… | Einstellungen…
- **ImportDialog:** Dateipicker, Fortschritt, Ergebnis‑Summary (Ampel).
- **ReviewForm:** PDF‑Panel (Seiten‑Navigation), Feld‑Editor, Top‑3 Kandidaten, „Speichern“.
- **TrainingForm:** Grid (Seite, LineIndex, Text, Label‑DropDown), „Trainieren“, „Als aktiv setzen“.
- **Einstellungen:** Pfade (models, labeled, storage), DB‑Connection, Kultur (de‑DE).

## 18. PDF‑Viewer & Hervorhebung
- PDF‑Rendering über PdfPig: Seiten als Vektor/Text → einfache Canvas‑Darstellung (Bounding‑Boxen)
- Markierung der Zeile, die zum aktuell ausgewählten Feld gehört (Farbe/Opacity)
- Zoom/Scroll, Seitennavigation, „Gehe zu Treffer“

## 19. Qualitätssicherung
- **Unit‑Tests:** Regex, Parser‑Zeilenbildung, Post‑Processing, Money/Date‑Parser.
- **Integration:** End‑to‑End (PDF → DB), Migrations‑Smoke‑Test.
- **ML‑Evaluation:** Micro‑/Macro‑F1 ≥ 0,85, Confusion‑Matrix, Per‑Class‑Recall ≥ 0,80.
- **Dataset‑Checks:** Klassenverteilung, Leckage‑Detector (gleiches Dokument nicht in Train/Test).

## 20. Sicherheit & Datenschutz
- **Lokal, offline**, keine Cloud.
- **PII‑Schutz:** Logs ohne Volltexte; optional Hash der Rechnungsnummer.
- **Zugriff:** Windows‑NTFS‑Rechte auf `storage/` und DB.

## 21. Deployment
- **Publish:** Self‑contained x64, inklusive `data/models` + `appsettings.User.json`‑Template.
- **First‑Run:** `Database.Migrate()`, Prüfen/Anlegen von `storage/` und Unterordnern.
- **Konfigurierbarkeit:** alle Pfade/Settings über UI + `appsettings.json`.

## 22. Roadmap
1) **MVP ohne ML:** Parser, manuelle Eingabe, EF+FileStore, Review‑UI  
2) **ML v1:** Zeilenklassifikation + Regex‑Extraktion, Kandidaten im UI  
3) **Labeling‑UI & Training in‑App**  
4) **OCR‑Fallback (rein .NET‑NuGet, optional)**  
5) **Lieferanten‑Profile** (Template‑Matcher)  

## 23. Offene Punkte/Entscheidungen
- Unique‑Constraint auf `InvoiceNumber`?
- OCR‑NuGet (rein .NET) evaluieren oder komplett weglassen?
- Mehrsprachigkeit (EN) & weitere Währungen
- Multi‑Page‑Tabellen (Positionslogik für Summenzeilen)

## 24. Beispiel‑Ablauf (End‑to‑End)
1. User wählt `A001.pdf` → Import  
2. Parser erzeugt 142 Zeilen → ML klassifiziert → Post‑Processing extrahiert Werte  
3. Review: User bestätigt/ändert → Speichern  
4. Datei landet in `storage\invoices\2025\10\{{Guid}}.pdf`, DB‑Satz + Pfad/Confidence/ModelVersion

## 25. Akzeptanzkriterien (kompakt)
- .NET 8, WinForms, **nur .NET‑NuGets**, keine Python/externen Services
- 6 Zielfelder extrahiert, editierbar, validiert, speicherbar
- Modelle versioniert (*.zip), Metriken angezeigt
- Batch‑Import ≥ 100 PDFs (ohne OCR) unter praxisnaher Zeit
