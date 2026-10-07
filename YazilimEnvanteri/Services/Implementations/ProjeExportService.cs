using ClosedXML.Excel;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using YazilimEnvanteri.Models.ViewModels;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Services.Implementations
{
    public class ProjeExportService : IProjeExportService
    {
        private static readonly string[] ColumnHeaders =
        {
            "Proje Kodu", "Proje Adı", "Hizmet Alanı", "Birim", "Yazılım Uzmanı", "Sunucu / URL", "Durum", "Kritiklik"
        };

        public byte[] ExportToExcel(IReadOnlyList<ProjeListItemViewModel> projeler)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Projeler");

            for (var i = 0; i < ColumnHeaders.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = ColumnHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2F6FED");
            }

            var row = 2;
            foreach (var p in projeler)
            {
                worksheet.Cell(row, 1).Value = p.ProjeKodu;
                worksheet.Cell(row, 2).Value = p.ProjeAdi;
                worksheet.Cell(row, 3).Value = p.ProjeHizmetAlani;
                worksheet.Cell(row, 4).Value = p.Birim ?? string.Empty;
                worksheet.Cell(row, 5).Value = p.YazilimUzmaniAdSoyad ?? string.Empty;
                worksheet.Cell(row, 6).Value = SunucuUrl(p);
                worksheet.Cell(row, 7).Value = p.ProjeDurum;
                worksheet.Cell(row, 8).Value = p.ProjeKritiklik;
                row++;
            }

            worksheet.SheetView.FreezeRows(1);
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportToPdf(IReadOnlyList<ProjeListItemViewModel> projeler)
        {
            var document = new Document();
            var section = document.AddSection();

            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = Unit.FromPoint(24);
            section.PageSetup.BottomMargin = Unit.FromPoint(24);
            section.PageSetup.LeftMargin = Unit.FromPoint(24);
            section.PageSetup.RightMargin = Unit.FromPoint(24);

            // Document Title
            var titleParagraph = section.AddParagraph("Projelerim");
            titleParagraph.Format.Font.Size = 16;
            titleParagraph.Format.Font.Bold = true;
            titleParagraph.Format.SpaceAfter = Unit.FromPoint(10);

            // Table definition
            var table = section.AddTable();
            table.TopPadding = Unit.FromPoint(5);
            table.BottomPadding = Unit.FromPoint(5);
            table.LeftPadding = Unit.FromPoint(4);
            table.RightPadding = Unit.FromPoint(4);

            // Relative widths mapped to approx 27.5 cm (A4 Landscape minus 2x24pt margins)
            double[] colWidths = { 2.0, 4.8, 3.5, 3.1, 3.8, 5.1, 2.6, 2.6 };
            foreach (var width in colWidths)
            {
                table.AddColumn(Unit.FromCentimeter(width));
            }

            // Header row
            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Shading.Color = new Color(47, 111, 237); // #2F6FED

            for (var i = 0; i < ColumnHeaders.Length; i++)
            {
                var cell = headerRow.Cells[i];
                cell.VerticalAlignment = VerticalAlignment.Center;
                var para = cell.AddParagraph(ColumnHeaders[i]);
                para.Format.Font.Bold = true;
                para.Format.Font.Color = Colors.White;
                para.Format.Font.Size = 9;
            }

            // Data rows
            var borderColor = new Color(224, 224, 224); // #E0E0E0
            foreach (var p in projeler)
            {
                var row = table.AddRow();
                row.Borders.Bottom.Width = 0.5;
                row.Borders.Bottom.Color = borderColor;

                AddCell(row.Cells[0], p.ProjeKodu);
                AddCell(row.Cells[1], p.ProjeAdi);
                AddCell(row.Cells[2], p.ProjeHizmetAlani);
                AddCell(row.Cells[3], p.Birim ?? "-");
                AddCell(row.Cells[4], p.YazilimUzmaniAdSoyad ?? "-");
                AddCell(row.Cells[5], SunucuUrl(p));
                AddCell(row.Cells[6], p.ProjeDurum);
                AddCell(row.Cells[7], p.ProjeKritiklik);
            }

            // Footer (Page numbers)
            var footerPara = section.Footers.Primary.AddParagraph();
            footerPara.Format.Alignment = ParagraphAlignment.Center;
            footerPara.Format.Font.Size = 9;
            footerPara.AddPageField();
            footerPara.AddText(" / ");
            footerPara.AddNumPagesField();

            var renderer = new PdfDocumentRenderer
            {
                Document = document
            };
            renderer.RenderDocument();

            using var stream = new MemoryStream();
            renderer.PdfDocument.Save(stream, false);
            return stream.ToArray();
        }

        private static void AddCell(Cell cell, string text)
        {
            cell.VerticalAlignment = VerticalAlignment.Center;
            var para = cell.AddParagraph(string.IsNullOrWhiteSpace(text) ? "-" : text);
            para.Format.Font.Size = 9;
        }

        private static string SunucuUrl(ProjeListItemViewModel p)
        {
            var parts = new[] { p.Sunucu, p.WebsiteUrl }.Where(v => !string.IsNullOrWhiteSpace(v));
            var combined = string.Join(" / ", parts);
            return string.IsNullOrEmpty(combined) ? "-" : combined;
        }
    }
}
