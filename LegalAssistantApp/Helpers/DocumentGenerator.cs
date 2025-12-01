using LegalAssistantApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace LegalAssistantApp.Helpers;

using DocumentModel = LegalAssistantApp.Models.Document;
using PdfDocument = QuestPDF.Fluent.Document;

public static class DocumentGenerator
{
    /// <summary>
    /// Простая генерация PDF-отчёта по списку документов.
    /// </summary>
    public static string GenerateDocumentsReportPdf(DocumentModel[] documents, string fileName)
    {
        var rootFolder = FileHelper.GetDocumentsRootFolder();
        var path = Path.Combine(rootFolder, fileName);

        var doc = PdfDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Text("Отчёт по документам")
                    .SemiBold().FontSize(20).FontColor(Colors.Green.Darken2);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);   // #
                        columns.RelativeColumn(3);    // Название
                        columns.RelativeColumn(2);    // Номер
                        columns.RelativeColumn(2);    // Тип
                        columns.RelativeColumn(2);    // Дата
                        columns.RelativeColumn(2);    // Статус
                    });

                    // Заголовок
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("№");
                        header.Cell().Element(HeaderCell).Text("Название");
                        header.Cell().Element(HeaderCell).Text("Номер");
                        header.Cell().Element(HeaderCell).Text("Тип");
                        header.Cell().Element(HeaderCell).Text("Дата");
                        header.Cell().Element(HeaderCell).Text("Статус");
                    });

                    for (var i = 0; i < documents.Length; i++)
                    {
                        var d = documents[i];
                        table.Cell().Element(Cell).Text((i + 1).ToString());
                        table.Cell().Element(Cell).Text(d.Title);
                        table.Cell().Element(Cell).Text(d.DocumentNumber);
                        table.Cell().Element(Cell).Text(d.DocumentType);
                        table.Cell().Element(Cell).Text(d.DocumentDate.ToShortDateString());
                        table.Cell().Element(Cell).Text(d.Status);
                    }

                    static IContainer HeaderCell(IContainer container)
                        => container.DefaultTextStyle(x => x.SemiBold()).Padding(2).Background(Colors.Grey.Lighten3);

                    static IContainer Cell(IContainer container)
                        => container.Padding(2);
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Сформировано: ").FontSize(10);
                    text.Span(DateTime.Now.ToString("g")).FontSize(10);
                });
            });
        });

        doc.GeneratePdf(path);
        return path;
    }
}
