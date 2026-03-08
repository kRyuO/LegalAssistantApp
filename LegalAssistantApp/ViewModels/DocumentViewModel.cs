using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Helpers;
using LegalAssistantApp.Models;
using LegalAssistantApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LegalAssistantApp.ViewModels;

public partial class DocumentViewModel : ObservableObject
{
    private readonly DocumentService _service;
    private readonly CounterpartyService _counterpartyService;
    private Document? _selectedDocument;

    public DocumentViewModel(DocumentService service)
    {
        _service = service;
        _counterpartyService = new CounterpartyService();
        Documents = new ObservableCollection<DocumentDisplayItem>();
        _originalDocuments = new ObservableCollection<Document>();

        _ = LoadDocumentsAsync();
        _ = LoadCounterpartiesAsync();
    }

    [RelayCommand]
    private async Task ScanDisksAsync()
    {
        try
        {
            StatusMessage = "Сканирование дисков...";
            var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt" };
            var scannedFiles = new List<string>();

             var drives = new[] { "C:\\", "D:\\" };

            foreach (var drive in drives)
            {
                if (Directory.Exists(drive))
                {
                    try
                    {
                        await ScanDirectoryAsync(drive, allowedExtensions, scannedFiles);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        StatusMessage = $"Нет доступа к диску {drive}";
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при сканировании {drive}: {ex.Message}");
                    }
                }
            }

            foreach (var filePath in scannedFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    var existingDoc = _originalDocuments.FirstOrDefault(d => d.FilePath == filePath);

                    if (existingDoc == null)
                    {
                        var doc = new Document
                        {
                            Title = Path.GetFileNameWithoutExtension(filePath),
                            DocumentNumber = "",
                            DocumentType = GetDocumentType(fileInfo.Extension),
                            Status = "Найден на диске",
                            DocumentDate = fileInfo.LastWriteTime,
                            FileName = fileInfo.Name,
                            FileExtension = fileInfo.Extension,
                            FilePath = filePath,
                            FileSize = fileInfo.Length,
                            FileCreatedDate = fileInfo.CreationTime,
                            FileModifiedDate = fileInfo.LastWriteTime,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow,
                            CreatedByUserId = 1
                        };

                        await _service.CreateDocumentAsync(doc);
                        _originalDocuments.Add(doc);
                        var displayItem = new DocumentDisplayItem(doc);
                        Documents.Add(displayItem);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при добавлении файла {filePath}: {ex.Message}");
                }
            }

            StatusMessage = $"Найдено и добавлено {scannedFiles.Count} файлов";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сканирования: {ex.Message}";
        }
    }

    private async Task ScanDirectoryAsync(string directory, string[] allowedExtensions, List<string> results)
    {
        try
        {
            var systemFolders = new[] { "Windows", "Program Files", "Program Files (x86)", "ProgramData", "$Recycle.Bin", "System Volume Information", "AppData", "Temp", "Temporary" };
            var dirName = Path.GetFileName(directory.TrimEnd('\\'));

            if (systemFolders.Contains(dirName, StringComparer.OrdinalIgnoreCase))
                return;

            try
            {
                var files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    try
                    {
                        var ext = Path.GetExtension(file).ToLowerInvariant();
                        if (allowedExtensions.Contains(ext))
                        {
                            results.Add(file);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            try
            {
                var subdirs = Directory.GetDirectories(directory);
                foreach (var subdir in subdirs.Take(5)) 
                {
                    if (results.Count >= 50) break; 
                    await ScanDirectoryAsync(subdir, allowedExtensions, results);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при сканировании {directory}: {ex.Message}");
        }
    }

    private string GetDocumentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "PDF",
            ".docx" or ".doc" => "Word",
            ".xlsx" or ".xls" => "Excel",
            ".pptx" or ".ppt" => "PowerPoint",
            _ => "Документ"
        };
    }

    [ObservableProperty]
    private ObservableCollection<DocumentDisplayItem> _documents = new ObservableCollection<DocumentDisplayItem>();
    
    private ObservableCollection<Document> _originalDocuments = new ObservableCollection<Document>();

    [ObservableProperty]
    private ObservableCollection<Counterparty> _counterparties = new ObservableCollection<Counterparty>();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public Document? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            SetProperty(ref _selectedDocument, value);
            OnPropertyChanged(nameof(SelectedDocumentDate));
            OnPropertyChanged(nameof(SelectedDisplayItem));
        }
    }
    
    public DocumentDisplayItem? SelectedDisplayItem
    {
        get
        {
            if (SelectedDocument == null) return null;
            var item = Documents.FirstOrDefault(d => d.Document.Id == SelectedDocument.Id);
            System.Diagnostics.Debug.WriteLine($"SelectedDisplayItem get: SelectedDocument={SelectedDocument?.Id}, Found item={item?.Id}");
            return item;
        }
        set
        {
            System.Diagnostics.Debug.WriteLine($"SelectedDisplayItem set: value={value?.Id}");
            if (value != null)
            {
                SelectedDocument = value.Document;
                System.Diagnostics.Debug.WriteLine($"SelectedDocument установлен: {SelectedDocument?.Id}");
            }
            else
            {
                SelectedDocument = null;
                System.Diagnostics.Debug.WriteLine("SelectedDocument установлен в null");
            }
            OnPropertyChanged();
        }
    }

    public DateTime? SelectedDocumentDate
    {
        get
        {
            if (SelectedDocument?.DocumentDate == null)
                return null;
            
            try
            {
                var date = SelectedDocument.DocumentDate.Value;
                return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в SelectedDocumentDate get: {ex}");
                return null;
            }
        }
        set
        {
            if (SelectedDocument != null)
            {
                try
                {
                    if (value.HasValue)
                    {
                        var date = value.Value.Date;
                        SelectedDocument.DocumentDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
                    }
                    else
                    {
                        SelectedDocument.DocumentDate = null;
                    }
                    OnPropertyChanged(nameof(SelectedDocument));
                    OnPropertyChanged(nameof(SelectedDocumentDate));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка в SelectedDocumentDate set: {ex}");
                }
            }
        }
    }

    [RelayCommand]
    public async Task LoadDocumentsAsync()
    {
        try
        {
            StatusMessage = "Загрузка документов...";
            System.Diagnostics.Debug.WriteLine("Начало загрузки документов...");
            
            var documents = await _service.GetAllDocumentsAsync();
            System.Diagnostics.Debug.WriteLine($"Получено {documents?.Count ?? 0} документов из сервиса");
            
            if (Dispatcher.UIThread.CheckAccess())
            {
                UpdateDocumentsCollection(documents);
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() => UpdateDocumentsCollection(documents));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки документов: {ex}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    [RelayCommand]
    private async Task LoadCounterpartiesAsync()
    {
        try
        {
            var items = await _counterpartyService.GetAllCounterpartiesAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Counterparties.Clear();
                foreach (var cp in items)
                {
                    Counterparties.Add(cp);
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки контрагентов: {ex.Message}";
        }
    }
    
    private void UpdateDocumentsCollection(List<Document>? documents)
    {
        try
        {
            Documents.Clear();
            _originalDocuments.Clear();
            
            if (documents != null && documents.Count > 0)
            {
                foreach (var doc in documents)
                {
                    _originalDocuments.Add(doc);
                    var displayItem = new DocumentDisplayItem(doc);
                    Documents.Add(displayItem);
                }
                
                StatusMessage = $"Загружено {Documents.Count} документов";
                System.Diagnostics.Debug.WriteLine($"Загружено {Documents.Count} документов в коллекцию");
                System.Diagnostics.Debug.WriteLine($"Documents collection type: {Documents.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"Documents is INotifyCollectionChanged: {Documents is System.Collections.Specialized.INotifyCollectionChanged}");
                
                OnPropertyChanged(nameof(Documents));
                
                if (Documents.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Первый документ: {Documents[0].Title}, ID: {Documents[0].Id}");
                    System.Diagnostics.Debug.WriteLine($"Первый документ тип: {Documents[0].GetType().FullName}");
                }
                
                Dispatcher.UIThread.Post(() =>
                {
                    OnPropertyChanged(nameof(Documents));
                    System.Diagnostics.Debug.WriteLine("OnPropertyChanged вызван через Dispatcher");
                    System.Diagnostics.Debug.WriteLine($"После OnPropertyChanged: Documents.Count = {Documents.Count}");
                }, DispatcherPriority.Render);
            }
            else
            {
                StatusMessage = "Документы не найдены. Используйте кнопку 'Добавить' для создания нового документа.";
                OnPropertyChanged(nameof(Documents));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка обновления коллекции: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Ошибка в UpdateDocumentsCollection: {ex}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    [RelayCommand]
    private async Task ExportReportAsync()
    {
        if (Documents.Count == 0)
        {
            StatusMessage = "Нет документов для экспорта";
            return;
        }

        try
        {
            StatusMessage = "Экспорт отчета...";

            var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
            {
                StatusMessage = "Не удалось получить окно приложения";
                return;
            }

            var storageProvider = mainWindow.StorageProvider;
            var fileName = $"Документы_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";
            
            var file = await storageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                SuggestedFileName = fileName,
                DefaultExtension = "csv",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("CSV файлы")
                    {
                        Patterns = new[] { "*.csv" }
                    },
                    new Avalonia.Platform.Storage.FilePickerFileType("Текстовые файлы")
                    {
                        Patterns = new[] { "*.txt" }
                    }
                }
            });

            if (file == null)
            {
                StatusMessage = "Экспорт отменен";
                return;
            }

            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream);
            await CreateCsvReportAsync(writer);

            StatusMessage = $"Отчет успешно экспортирован: {file.Name}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка экспорта: {ex.Message}";
        }
    }

    private async Task CreateCsvReportAsync(StreamWriter writer)
    {
        await writer.WriteLineAsync("ID;Название;Номер;Дата документа;Тип;Статус;Теги;Дата создания;Дата обновления");

        foreach (var displayItem in Documents)
        {
            var doc = displayItem.Document;
            var line = $"{doc.Id};" +
                      $"{EscapeCsvField(doc.Title)};" +
                      $"{EscapeCsvField(doc.DocumentNumber)};" +
                      $"{doc.DocumentDate:dd.MM.yyyy};" +
                      $"{EscapeCsvField(doc.DocumentType)};" +
                      $"{EscapeCsvField(doc.Status)};" +
                      $"{EscapeCsvField(doc.Tags)};" +
                      $"{doc.CreatedDate:dd.MM.yyyy HH:mm};" +
                      $"{(doc.UpdatedDate?.ToString("dd.MM.yyyy HH:mm") ?? "")}";

            await writer.WriteLineAsync(line);
        }
    }

    private string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        if (field.Contains(';') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            var documents = await _service.SearchDocumentsAsync(SearchText);
            Documents.Clear();
            _originalDocuments.Clear();
            foreach (var doc in documents)
            {
                _originalDocuments.Add(doc);
                var displayItem = new DocumentDisplayItem(doc);
                Documents.Add(displayItem);
            }
            StatusMessage = $"Найдено {Documents.Count} документов";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка поиска: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddNew()
    {
        var newDocument = new Document
        {
            Title = "Новый документ",
            DocumentNumber = "",
            DocumentType = "Документ", 
            Status = "Черновик",
            DocumentDate = DateTime.Now.Date, 
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            CreatedByUserId = 1,
            Currency = "RUB",
            Tags = "",
            Content = "",
            FileName = "",
            FilePath = "",
            FileExtension = ""
        };
        _originalDocuments.Add(newDocument);
        var displayItem = new DocumentDisplayItem(newDocument);
        Documents.Add(displayItem);
        SelectedDocument = newDocument;
        SelectedDisplayItem = displayItem;
        OnPropertyChanged(nameof(SelectedDocumentDate));
        StatusMessage = "Создан новый документ";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedDocument == null) return;

        if (string.IsNullOrWhiteSpace(SelectedDocument.Title))
        {
            StatusMessage = "Ошибка: Название документа обязательно";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedDocument.DocumentType))
        {
            StatusMessage = "Ошибка: Тип документа обязателен";
            return;
        }

        try
        {
            if (SelectedDocument.Id == 0)
            {
                await _service.CreateDocumentAsync(SelectedDocument);
                StatusMessage = "Документ создан";
            }
            else
            {
                await _service.UpdateDocumentAsync(SelectedDocument);
                StatusMessage = "Документ сохранен";
            }
            await LoadDocumentsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedDocument == null || SelectedDocument.Id == 0) return;

        try
        {
            await _service.DeleteDocumentAsync(SelectedDocument.Id);
            var displayItem = Documents.FirstOrDefault(d => d.Document == SelectedDocument);
            if (displayItem != null)
            {
                Documents.Remove(displayItem);
            }
            _originalDocuments.Remove(SelectedDocument);
            SelectedDocument = null;
            SelectedDisplayItem = null;
            StatusMessage = "Документ удален";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка удаления: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDocumentsAsync();
    }

    [RelayCommand]
    private async Task AttachFileAsync()
    {
        if (SelectedDocument == null)
        {
            StatusMessage = "Сначала выберите документ";
            return;
        }

        try
        {
            var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
            {
                StatusMessage = "Не удалось получить окно приложения";
                return;
            }

            var storageProvider = mainWindow.StorageProvider;
            var files = await storageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Выбор файла документа",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Документы")
                    {
                        Patterns = new[] { "*.pdf", "*.docx", "*.doc", "*.xlsx", "*.xls", "*.pptx", "*.ppt" }
                    },
                    new Avalonia.Platform.Storage.FilePickerFileType("Все файлы")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files == null || files.Count == 0)
            {
                StatusMessage = "Файл не выбран";
                return;
            }

            var file = files[0];
            var path = file.Path.LocalPath;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                StatusMessage = "Файл не найден";
                return;
            }

            var fileInfo = new FileInfo(path);
            var safeName = FileHelper.SanitizeFileName(fileInfo.Name);
            var targetFolder = FileHelper.GetDocumentsRootFolder();
            var targetPath = Path.Combine(targetFolder, safeName);

            File.Copy(path, targetPath, overwrite: true);

            SelectedDocument.FileName = safeName;
            SelectedDocument.FileExtension = fileInfo.Extension;
            SelectedDocument.FileSize = fileInfo.Length;
            SelectedDocument.FileCreatedDate = fileInfo.CreationTime;
            SelectedDocument.FileModifiedDate = fileInfo.LastWriteTime;
            SelectedDocument.FilePath = targetPath;

            await SaveAsync();
            StatusMessage = "Файл успешно привязан к документу";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при привязке файла: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenFile()
    {
        if (SelectedDocument == null)
        {
            StatusMessage = "Выберите документ для открытия";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedDocument.FilePath) || !File.Exists(SelectedDocument.FilePath))
        {
            StatusMessage = "Для документа не указан файл или он отсутствует на диске";
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = SelectedDocument.FilePath,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Не удалось открыть файл: {ex.Message}";
        }
    }
}