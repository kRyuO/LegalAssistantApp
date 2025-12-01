using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Helpers;
using LegalAssistantApp.Models;
using LegalAssistantApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LegalAssistantApp.ViewModels;

public partial class DocumentViewModel : ObservableObject
{
    private readonly DocumentService _service;

    public DocumentViewModel(DocumentService service)
    {
        _service = service;
        Documents = new ObservableCollection<Document>();
    }

    public ObservableCollection<Document> Documents { get; }

    [ObservableProperty]
    private Document? _selectedDocument;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public async Task LoadDocumentsAsync()
    {
        Documents.Clear();
        var items = await _service.GetDocumentsAsync();
        foreach (var d in items)
        {
            Documents.Add(d);
        }
    }

    [RelayCommand]
    private async Task Search()
    {
        Documents.Clear();
        var items = await _service.GetDocumentsAsync(SearchText);
        foreach (var d in items)
        {
            Documents.Add(d);
        }
    }

    [RelayCommand]
    private void AddNew()
    {
        var doc = new Document
        {
            Title = "Новый документ",
            Status = "Draft",
            DocumentDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            CreatedByUserId = 1 // временно: администратор по умолчанию
        };
        Documents.Add(doc);
        SelectedDocument = doc;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (SelectedDocument == null)
            return;

        SelectedDocument.UpdatedDate = DateTime.UtcNow;
        await _service.SaveDocumentAsync(SelectedDocument);
        await LoadDocumentsAsync();
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedDocument == null || SelectedDocument.Id == 0)
            return;

        await _service.DeleteDocumentAsync(SelectedDocument.Id);
        await LoadDocumentsAsync();
    }

    [RelayCommand]
    private void ExportReport()
    {
        if (Documents.Count == 0)
        {
            StatusMessage = "Нет данных для отчёта.";
            return;
        }

        var fileName = $"report-documents-{DateTime.Now:yyyyMMddHHmmss}.pdf";
        var path = DocumentGenerator.GenerateDocumentsReportPdf(Documents.ToArray(), fileName);
        StatusMessage = $"Отчёт сохранён: {path}";
    }
}
