using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Unique invoice number"),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false, comment: "Invoice date"),
                    IssuerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Name of the invoice issuer"),
                    IssuerStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Issuer street address"),
                    IssuerPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "Issuer postal code"),
                    IssuerCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Issuer city"),
                    IssuerCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Issuer country"),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Net total amount"),
                    VatTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "VAT total amount"),
                    GrossTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Gross total amount"),
                    SourceFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Path to source PDF file"),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Import timestamp"),
                    ExtractionConfidence = table.Column<float>(type: "real", nullable: false, comment: "ML extraction confidence (0.0-1.0)"),
                    ModelVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "ML model version used for extraction")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.CheckConstraint("CK_Invoices_Amount_Consistency", "ABS((NetTotal + VatTotal) - GrossTotal) <= 0.02");
                    table.CheckConstraint("CK_Invoices_Confidence_Range", "ExtractionConfidence >= 0.0 AND ExtractionConfidence <= 1.0");
                    table.CheckConstraint("CK_Invoices_GrossTotal_Positive", "GrossTotal >= 0");
                    table.CheckConstraint("CK_Invoices_NetTotal_Positive", "NetTotal >= 0");
                    table.CheckConstraint("CK_Invoices_VatTotal_Positive", "VatTotal >= 0");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceRawBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Foreign key to Invoice"),
                    Page = table.Column<int>(type: "int", nullable: false, comment: "Page number in PDF"),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false, comment: "Extracted text content"),
                    X = table.Column<float>(type: "real", nullable: false, comment: "X coordinate of bounding box"),
                    Y = table.Column<float>(type: "real", nullable: false, comment: "Y coordinate of bounding box"),
                    Width = table.Column<float>(type: "real", nullable: false, comment: "Width of bounding box"),
                    Height = table.Column<float>(type: "real", nullable: false, comment: "Height of bounding box"),
                    LineIndex = table.Column<int>(type: "int", nullable: false, comment: "Line index within page"),
                    PredictedLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "ML predicted label"),
                    PredictionConfidence = table.Column<float>(type: "real", nullable: true, comment: "ML prediction confidence"),
                    ActualLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "Manually assigned label"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Creation timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceRawBlocks", x => x.Id);
                    table.CheckConstraint("CK_InvoiceRawBlocks_Confidence_Range", "PredictionConfidence IS NULL OR (PredictionConfidence >= 0.0 AND PredictionConfidence <= 1.0)");
                    table.CheckConstraint("CK_InvoiceRawBlocks_Height_Positive", "Height > 0");
                    table.CheckConstraint("CK_InvoiceRawBlocks_LineIndex_Positive", "LineIndex >= 0");
                    table.CheckConstraint("CK_InvoiceRawBlocks_Page_Positive", "Page > 0");
                    table.CheckConstraint("CK_InvoiceRawBlocks_Width_Positive", "Width > 0");
                    table.ForeignKey(
                        name: "FK_InvoiceRawBlocks_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_Actual_Predicted",
                table: "InvoiceRawBlocks",
                columns: new[] { "ActualLabel", "PredictedLabel" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_ActualLabel",
                table: "InvoiceRawBlocks",
                column: "ActualLabel");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_CreatedAt",
                table: "InvoiceRawBlocks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_Invoice_LineIndex",
                table: "InvoiceRawBlocks",
                columns: new[] { "InvoiceId", "LineIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_Invoice_Page",
                table: "InvoiceRawBlocks",
                columns: new[] { "InvoiceId", "Page" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_InvoiceId",
                table: "InvoiceRawBlocks",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_Label_Confidence",
                table: "InvoiceRawBlocks",
                columns: new[] { "PredictedLabel", "PredictionConfidence" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_Page",
                table: "InvoiceRawBlocks",
                column: "Page");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_PredictedLabel",
                table: "InvoiceRawBlocks",
                column: "PredictedLabel");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRawBlocks_PredictionConfidence",
                table: "InvoiceRawBlocks",
                column: "PredictionConfidence");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Amount_Date",
                table: "Invoices",
                columns: new[] { "GrossTotal", "InvoiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Date_Issuer",
                table: "Invoices",
                columns: new[] { "InvoiceDate", "IssuerName" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ExtractionConfidence",
                table: "Invoices",
                column: "ExtractionConfidence");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ImportedAt",
                table: "Invoices",
                column: "ImportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceDate",
                table: "Invoices",
                column: "InvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber_Unique",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_IssuerName",
                table: "Invoices",
                column: "IssuerName");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ModelVersion",
                table: "Invoices",
                column: "ModelVersion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceRawBlocks");

            migrationBuilder.DropTable(
                name: "Invoices");
        }
    }
}
