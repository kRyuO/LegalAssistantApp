using System;
using LegalAssistantApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LegalAssistantApp.ViewModels
{
    /// <summary>
    /// Обертка для Document с строковыми свойствами для отображения в DataGrid
    /// </summary>
    public class DocumentDisplayItem : ObservableObject
    {
        private readonly Document _document;

        public DocumentDisplayItem(Document document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public Document Document => _document;

        public int Id => _document.Id;

        public string Title
        {
            get => _document.Title ?? string.Empty;
            set
            {
                if (_document.Title != value)
                {
                    _document.Title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DocumentNumber
        {
            get => _document.DocumentNumber ?? string.Empty;
            set
            {
                if (_document.DocumentNumber != value)
                {
                    _document.DocumentNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DocumentType
        {
            get => _document.DocumentType ?? string.Empty;
            set
            {
                if (_document.DocumentType != value)
                {
                    _document.DocumentType = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => _document.Status ?? string.Empty;
            set
            {
                if (_document.Status != value)
                {
                    _document.Status = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DocumentDateString
        {
            get => _document.DocumentDate?.ToString("dd.MM.yyyy") ?? string.Empty;
        }
    }
}

