using System;
using LegalAssistantApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LegalAssistantApp.ViewModels
{
    /// <summary>
    /// Обертка для DocumentEvent с строковыми свойствами для отображения в DataGrid
    /// </summary>
    public class EventDisplayItem : ObservableObject
    {
        private readonly DocumentEvent _event;

        public EventDisplayItem(DocumentEvent eventItem)
        {
            _event = eventItem ?? throw new ArgumentNullException(nameof(eventItem));
        }

        public DocumentEvent Event => _event;

        public int Id => _event.Id;

        public string Title
        {
            get => _event.Title ?? string.Empty;
            set
            {
                if (_event.Title != value)
                {
                    _event.Title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string EventType
        {
            get => _event.EventType ?? string.Empty;
            set
            {
                if (_event.EventType != value)
                {
                    _event.EventType = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => _event.Status ?? string.Empty;
            set
            {
                if (_event.Status != value)
                {
                    _event.Status = value;
                    OnPropertyChanged();
                }
            }
        }

        public string EventDateString
        {
            get => _event.EventDate.ToString("dd.MM.yyyy HH:mm");
        }

        public string DocumentTitle
        {
            get => _event.Document?.Title ?? "-";
        }
    }
}

