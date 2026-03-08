using LegalAssistantApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;

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
                        table.Cell().Element(Cell).Text(d.DocumentNumber ?? "");
                        table.Cell().Element(Cell).Text(d.DocumentType ?? "");
                        table.Cell().Element(Cell).Text(d.DocumentDate?.ToString("dd.MM.yyyy") ?? "не указана");
                        table.Cell().Element(Cell).Text(d.Status ?? "");
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

    /// <summary>
    /// Генерация подробного отчёта по одному документу
    /// </summary>
    public static string GenerateDocumentDetailReportPdf(DocumentModel document, string fileName)
    {
        var rootFolder = FileHelper.GetDocumentsRootFolder();
        var path = Path.Combine(rootFolder, fileName);

        var doc = PdfDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Text("Подробная информация о документе")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                page.Content().Column(column =>
                {
                    column.Item().PaddingBottom(10).Text($"Название: {document.Title}")
                        .FontSize(14).SemiBold();

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        AddTableRow(table, "Номер документа:", document.DocumentNumber ?? "не указан");
                        AddTableRow(table, "Тип документа:", document.DocumentType ?? "не указан");
                        AddTableRow(table, "Статус:", document.Status ?? "не указан");
                        AddTableRow(table, "Дата документа:", document.DocumentDate?.ToString("dd.MM.yyyy") ?? "не указана");

                        if (document.Amount.HasValue)
                            AddTableRow(table, "Сумма:", $"{document.Amount:N2} {document.Currency ?? "RUB"}");

                        if (document.Counterparty != null)
                            AddTableRow(table, "Контрагент:", document.Counterparty.Name);

                        AddTableRow(table, "Теги:", document.Tags ?? "нет");
                        AddTableRow(table, "Конфиденциальный:", document.IsConfidential ? "Да" : "Нет");
                        AddTableRow(table, "Создан:", document.CreatedDate.ToString("dd.MM.yyyy HH:mm"));

                        if (document.UpdatedDate.HasValue)
                            AddTableRow(table, "Обновлён:", document.UpdatedDate.Value.ToString("dd.MM.yyyy HH:mm"));
                    });

                    if (!string.IsNullOrEmpty(document.Content))
                    {
                        column.Item().PaddingTop(15).Text("Содержание:")
                            .FontSize(12).SemiBold();
                        column.Item().Text(document.Content)
                            .FontSize(11);
                    }

                    if (document.Events != null && document.Events.Any())
                    {
                        column.Item().PaddingTop(15).Text("События по документу:")
                            .FontSize(12).SemiBold();

                        column.Item().Table(eventsTable =>
                        {
                            eventsTable.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            eventsTable.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Событие");
                                header.Cell().Element(HeaderCell).Text("Дата");
                                header.Cell().Element(HeaderCell).Text("Тип");
                                header.Cell().Element(HeaderCell).Text("Статус");
                            });

                            foreach (var ev in document.Events.OrderBy(e => e.EventDate))
                            {
                                eventsTable.Cell().Element(Cell).Text(ev.Title);
                                eventsTable.Cell().Element(Cell).Text(ev.EventDate.ToString("dd.MM.yyyy HH:mm"));
                                eventsTable.Cell().Element(Cell).Text(ev.EventType ?? "");
                                eventsTable.Cell().Element(Cell).Text(ev.Status ?? "");
                            }
                        });
                    }
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Страница ").FontSize(10);
                    text.CurrentPageNumber().FontSize(10);
                    text.Span(" / ").FontSize(10);
                    text.TotalPages().FontSize(10);
                    text.Span($" | Сформировано: {DateTime.Now:g}").FontSize(10);
                });
            });
        });

        doc.GeneratePdf(path);
        return path;
    }

    /// <summary>
    /// Генерация отчёта по контрагентам
    /// </summary>
    public static string GenerateCounterpartiesReportPdf(Counterparty[] counterparties, string fileName)
    {
        var rootFolder = FileHelper.GetDocumentsRootFolder();
        var path = Path.Combine(rootFolder, fileName);

        var doc = PdfDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Text("Отчёт по контрагентам")
                    .SemiBold().FontSize(20).FontColor(Colors.Purple.Darken2);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);   // #
                        columns.RelativeColumn(3);    // Наименование
                        columns.RelativeColumn(2);    // ИНН
                        columns.RelativeColumn(2);    // ОГРН
                        columns.RelativeColumn(2);    // Телефон
                        columns.RelativeColumn(2);    // Статус
                        columns.RelativeColumn(2);    // Риск
                    });

                    // Заголовок
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("№");
                        header.Cell().Element(HeaderCell).Text("Наименование");
                        header.Cell().Element(HeaderCell).Text("ИНН");
                        header.Cell().Element(HeaderCell).Text("ОГРН");
                        header.Cell().Element(HeaderCell).Text("Телефон");
                        header.Cell().Element(HeaderCell).Text("Статус");
                        header.Cell().Element(HeaderCell).Text("Риск");
                    });

                    for (var i = 0; i < counterparties.Length; i++)
                    {
                        var c = counterparties[i];
                        table.Cell().Element(Cell).Text((i + 1).ToString());
                        table.Cell().Element(Cell).Text(c.Name);
                        table.Cell().Element(Cell).Text(c.INN ?? "");
                        table.Cell().Element(Cell).Text(c.OGRN ?? "");
                        table.Cell().Element(Cell).Text(c.Phone ?? "");
                        table.Cell().Element(Cell).Text(c.Status ?? "");
                        table.Cell().Element(Cell).Text(c.RiskLevel ?? "");
                    }
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span($"Всего контрагентов: {counterparties.Length}").FontSize(10);
                    text.Span($" | Сформировано: {DateTime.Now:g}").FontSize(10);
                });
            });
        });

        doc.GeneratePdf(path);
        return path;
    }

    /// <summary>
    /// Генерация отчёта по событиям
    /// </summary>
    public static string GenerateEventsReportPdf(DocumentEvent[] events, string fileName)
    {
        var rootFolder = FileHelper.GetDocumentsRootFolder();
        var path = Path.Combine(rootFolder, fileName);

        var doc = PdfDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Text("Отчёт по событиям")
                    .SemiBold().FontSize(20).FontColor(Colors.Orange.Darken2);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);   // #
                        columns.RelativeColumn(3);    // Событие
                        columns.RelativeColumn(2);    // Документ
                        columns.RelativeColumn(2);    // Тип
                        columns.RelativeColumn(2);    // Дата
                        columns.RelativeColumn(2);    // Статус
                    });

                    // Заголовок
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("№");
                        header.Cell().Element(HeaderCell).Text("Событие");
                        header.Cell().Element(HeaderCell).Text("Документ");
                        header.Cell().Element(HeaderCell).Text("Тип");
                        header.Cell().Element(HeaderCell).Text("Дата");
                        header.Cell().Element(HeaderCell).Text("Статус");
                    });

                    var sortedEvents = events.OrderBy(e => e.EventDate).ToArray();
                    for (var i = 0; i < sortedEvents.Length; i++)
                    {
                        var e = sortedEvents[i];
                        table.Cell().Element(Cell).Text((i + 1).ToString());
                        table.Cell().Element(Cell).Text(e.Title);
                        table.Cell().Element(Cell).Text(e.Document?.Title ?? "не указан");
                        table.Cell().Element(Cell).Text(e.EventType ?? "");
                        table.Cell().Element(Cell).Text(e.EventDate.ToString("dd.MM.yyyy HH:mm"));
                        table.Cell().Element(Cell).Text(e.Status ?? "");
                    }
                });

                page.Footer().AlignRight().Text(text =>
                {
                    var completed = events.Count(e => e.IsCompleted);
                    var pending = events.Count(e => !e.IsCompleted);

                    text.Span($"Всего: {events.Length}").FontSize(10);
                    text.Span($" | Завершено: {completed}").FontSize(10);
                    text.Span($" | Ожидают: {pending}").FontSize(10);
                    text.Span($" | Сформировано: {DateTime.Now:g}").FontSize(10);
                });
            });
        });

        doc.GeneratePdf(path);
        return path;
    }

    // Вспомогательные методы
    private static void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Element(LabelCell).Text(label);
        table.Cell().Element(ValueCell).Text(value);
    }

    private static IContainer LabelCell(IContainer container)
        => container.DefaultTextStyle(x => x.SemiBold()).Padding(5).Background(Colors.Grey.Lighten4);

    private static IContainer ValueCell(IContainer container)
        => container.Padding(5);

    private static IContainer HeaderCell(IContainer container)
        => container.DefaultTextStyle(x => x.SemiBold()).Padding(2).Background(Colors.Grey.Lighten3);

    private static IContainer Cell(IContainer container)
        => container.Padding(2);
}