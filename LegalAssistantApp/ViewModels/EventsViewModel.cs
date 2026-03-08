using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Models;
using LegalAssistantApp.Services;
using LegalAssistantApp.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LegalAssistantApp.ViewModels;

public partial class EventsViewModel : ObservableObject
{
    private readonly EventService _eventService;
    private readonly DocumentService _documentService;
    private readonly CounterpartyService _counterpartyService;

    public EventsViewModel(EventService eventService, DocumentService documentService, CounterpartyService counterpartyService)
    {
        _eventService = eventService;
        _documentService = documentService;
        _counterpartyService = counterpartyService;

        UpcomingEvents = new ObservableCollection<EventDisplayItem>();
        _originalEvents = new ObservableCollection<DocumentEvent>();
        Documents = new ObservableCollection<Document>();
        Counterparties = new ObservableCollection<Counterparty>();

        UntilDate = DateTime.Now.AddDays(30);
        
        // Загружаем данные при создании
        _ = LoadEventsAsync();
    }
    
    // Метод для инициализации данных (вызывается из MainWindowViewModel)
    public async Task InitializeAsync()
    {
        await LoadDocumentsAsync();
        await LoadCounterpartiesAsync();
    }

    [ObservableProperty]
    private ObservableCollection<EventDisplayItem> _upcomingEvents = new ObservableCollection<EventDisplayItem>();
    
    // Внутренняя коллекция для хранения оригинальных событий
    private ObservableCollection<DocumentEvent> _originalEvents = new ObservableCollection<DocumentEvent>();

    // Локализованные свойства
    public string EventsUntilLabel => LocalizationService.Instance["EventsUntil"];
    public string UpcomingEventsLabel => LocalizationService.Instance["UpcomingEvents"];
    public string EditEventLabel => LocalizationService.Instance["EditEvent"];

    [ObservableProperty]
    private ObservableCollection<Document> _documents;

    [ObservableProperty]
    private ObservableCollection<Counterparty> _counterparties;

    [ObservableProperty]
    private DateTime? _untilDate = DateTime.Now.AddDays(30);

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private DocumentEvent? _selectedEvent;
    
    // Свойство для выбранного элемента отображения
    public EventDisplayItem? SelectedDisplayItem
    {
        get
        {
            if (SelectedEvent == null) return null;
            return UpcomingEvents.FirstOrDefault(e => e.Event == SelectedEvent);
        }
        set
        {
            SelectedEvent = value?.Event;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private bool _isLoading;

    // Отдельное свойство для времени события
    [ObservableProperty]
    private string _eventTime = string.Empty;

    // Свойство-обертка для DatePicker (DateTime -> DateTime?)
    public DateTime? SelectedEventDate
    {
        get
        {
            if (SelectedEvent == null) 
                return null;
            
            try
            {
                var date = SelectedEvent.EventDate;
                // Возвращаем только дату без времени с Unspecified kind
                return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в SelectedEventDate get: {ex}");
                return null;
            }
        }
        set
        {
            if (SelectedEvent != null)
            {
                try
                {
                    if (value.HasValue)
                    {
                        // Сохраняем время из текущего EventDate
                        var currentTime = SelectedEvent.EventDate.TimeOfDay;
                        var date = value.Value.Date;
                        // Создаем новую дату с сохранением времени
                        SelectedEvent.EventDate = new DateTime(date.Year, date.Month, date.Day, 
                            currentTime.Hours, currentTime.Minutes, currentTime.Seconds, DateTimeKind.Unspecified);
                        EventTime = SelectedEvent.EventDate.ToString("HH:mm");
                    }
                    else
                    {
                        // Если значение null, устанавливаем дату на сегодня с сохранением времени
                        var today = DateTime.Today;
                        var currentTime = SelectedEvent.EventDate.TimeOfDay;
                        SelectedEvent.EventDate = new DateTime(today.Year, today.Month, today.Day,
                            currentTime.Hours, currentTime.Minutes, currentTime.Seconds, DateTimeKind.Unspecified);
                    }
                    OnPropertyChanged(nameof(SelectedEvent));
                    OnPropertyChanged(nameof(SelectedEventDate));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка в SelectedEventDate set: {ex}");
                }
            }
        }
    }

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _statusColor = "#4a8a6a";

    // Списки для ComboBox
    public List<string> EventTypes { get; } = new List<string>
    {
        "Напоминание",
        "Срок",
        "Встреча",
        "Дедлайн",
        "Звонок",
        "Платеж",
        "Отчет",
        "Другое"
    };

    public List<string> EventStatuses { get; } = new List<string>
    {
        "Активно",
        "Завершено",
        "Отменено",
        "Перенесено"
    };

    public List<ReminderOption> ReminderOptions { get; } = new List<ReminderOption>
    {
        new ReminderOption("За 5 минут", 5),
        new ReminderOption("За час", 60),
        new ReminderOption("За день", 24 * 60),
        new ReminderOption("За неделю", 7 * 24 * 60)
    };

    [ObservableProperty]
    private ReminderOption? _selectedReminderOption;

    public async Task LoadEventsAsync()
    {
        await LoadEventsDataAsync();
    }

    private async Task LoadEventsDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Используем правильную дату для фильтрации
            var until = UntilDate ?? DateTime.Now.AddDays(30);
            var events = await _eventService.GetUpcomingEventsAsync(until);
            
            // Обновляем коллекцию на UI потоке
            if (Dispatcher.UIThread.CheckAccess())
            {
                // Уже на UI потоке
                UpdateEventsCollection(events);
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() => UpdateEventsCollection(events));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки событий: {ex.Message}";
            StatusColor = "#b35454";
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки событий: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private void UpdateEventsCollection(List<DocumentEvent>? events)
    {
        UpcomingEvents.Clear();
        _originalEvents.Clear();
        
        if (events != null && events.Count > 0)
        {
            foreach (var e in events)
            {
                _originalEvents.Add(e);
                var displayItem = new EventDisplayItem(e);
                UpcomingEvents.Add(displayItem);
            }
            
            StatusMessage = $"Загружено {UpcomingEvents.Count} событий";
            System.Diagnostics.Debug.WriteLine($"Загружено {UpcomingEvents.Count} событий в коллекцию");
            System.Diagnostics.Debug.WriteLine($"UpcomingEvents collection type: {UpcomingEvents.GetType().FullName}");
            
            // Принудительно обновляем UI
            OnPropertyChanged(nameof(UpcomingEvents));
            
            // Принудительно обновляем через Dispatcher
            Dispatcher.UIThread.Post(() =>
            {
                OnPropertyChanged(nameof(UpcomingEvents));
                System.Diagnostics.Debug.WriteLine("OnPropertyChanged для UpcomingEvents вызван через Dispatcher");
            }, DispatcherPriority.Render);
        }
        else
        {
            StatusMessage = "События не найдены. Используйте кнопку 'Добавить' для создания нового события.";
            OnPropertyChanged(nameof(UpcomingEvents));
        }
        StatusColor = "#4a8a6a";
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadEventsDataAsync();
        await LoadDocumentsAsync();
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await Refresh();
            return;
        }

        try
        {
            IsLoading = true;
            var events = await _eventService.SearchEventsAsync(SearchText);
            UpcomingEvents.Clear();
            _originalEvents.Clear();
            foreach (var e in events)
            {
                _originalEvents.Add(e);
                var displayItem = new EventDisplayItem(e);
                UpcomingEvents.Add(displayItem);
            }
            StatusMessage = $"Найдено {UpcomingEvents.Count} событий";
            StatusColor = "#4a8a6a";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка поиска: {ex.Message}";
            StatusColor = "#b35454";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CheckReminders()
    {
        try
        {
            var reminders = await _eventService.GetEventsForReminderAsync();
            if (reminders.Count > 0)
            {
                StatusMessage = $"Есть {reminders.Count} напоминаний";
                StatusColor = "#b89a5a";
            }
            else
            {
                StatusMessage = "Нет активных напоминаний";
                StatusColor = "#4a8a6a";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при проверке напоминаний: {ex.Message}";
            StatusColor = "#b35454";
        }
    }

    [RelayCommand]
    private void Export()
    {
        StatusMessage = "Экспорт событий...";
        StatusColor = "#5a8a7a";
        // Логика экспорта
        StatusMessage = "События экспортированы";
        StatusColor = "#4a8a6a";
    }

    [RelayCommand]
    private async Task SaveEvent()
    {
        if (SelectedEvent == null) return;

        // Валидация обязательных полей
        if (string.IsNullOrWhiteSpace(SelectedEvent.Title))
        {
            StatusMessage = "Ошибка: Название события обязательно";
            StatusColor = "#b35454";
            return;
        }

        try
        {
            if (SelectedEvent.Id == 0)
            {
                await _eventService.CreateEventAsync(SelectedEvent);
                StatusMessage = "Событие создано";
            }
            else
            {
                await _eventService.UpdateEventAsync(SelectedEvent);
                StatusMessage = "Событие сохранено";
            }
            StatusColor = "#4a8a6a";
            await Refresh();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения: {ex.Message}";
            StatusColor = "#b35454";
        }
    }

    [RelayCommand]
    private async Task MarkCompleted(DocumentEvent eventItem)
    {
        if (eventItem == null) return;

        try
        {
            await _eventService.CompleteEventAsync(eventItem.Id);
            await LoadEventsDataAsync();
            StatusMessage = "Событие завершено";
            StatusColor = "#4a8a6a";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при завершении события: {ex.Message}";
            StatusColor = "#b35454";
        }
    }

    [RelayCommand]
    private async Task DeleteEvent()
    {
        if (SelectedEvent == null || SelectedEvent.Id == 0) return;

        try
        {
            await _eventService.DeleteEventAsync(SelectedEvent.Id);
            var displayItem = UpcomingEvents.FirstOrDefault(e => e.Event == SelectedEvent);
            if (displayItem != null)
            {
                UpcomingEvents.Remove(displayItem);
            }
            _originalEvents.Remove(SelectedEvent);
            SelectedEvent = null;
            SelectedDisplayItem = null;
            StatusMessage = "Событие удалено";
            StatusColor = "#4a8a6a";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка удаления: {ex.Message}";
            StatusColor = "#b35454";
        }
    }

    [RelayCommand]
    private async Task LoadDocumentsAsync()
    {
        try
        {
            var documents = await _documentService.GetAllDocumentsAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Documents.Clear();
                foreach (var doc in documents)
                {
                    Documents.Add(doc);
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки документов: {ex.Message}";
            StatusColor = "#b35454";
        }
    }

    [RelayCommand]
    private async Task LoadCounterpartiesAsync()
    {
        try
        {
            var counterparties = await _counterpartyService.GetAllCounterpartiesAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Counterparties.Clear();
                foreach (var cp in counterparties)
                {
                    Counterparties.Add(cp);
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки контрагентов: {ex.Message}";
            StatusColor = "#b35454";
        }
    }

    [RelayCommand]
    private void AddNewEvent()
    {
        var eventDate = DateTime.Now.AddDays(1);
        var newEvent = new DocumentEvent
        {
            Title = "Новое событие",
            Description = string.Empty,
            EventDate = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 12, 0, 0, DateTimeKind.Unspecified),
            EventType = "Напоминание",
            Status = "Активно",
            IsCompleted = false,
            HasReminder = false,
            CreatedDate = DateTime.Now,
            CreatedBy = "system",
            NotificationEmail = string.Empty
        };
        _originalEvents.Add(newEvent);
        var displayItem = new EventDisplayItem(newEvent);
        UpcomingEvents.Add(displayItem);
        SelectedEvent = newEvent;
        SelectedDisplayItem = displayItem;
        EventTime = newEvent.EventDate.ToString("HH:mm");
        OnPropertyChanged(nameof(SelectedEventDate));
        StatusMessage = "Создано новое событие";
        StatusColor = "#4a8a6a";
    }

    // Обновление времени при изменении выбранного события
    partial void OnSelectedEventChanged(DocumentEvent? value)
    {
        if (value != null)
        {
            EventTime = value.EventDate.ToString("HH:mm");

            // Поддерживаем синхронизацию выбранной опции напоминания
            if (value.ReminderOffsetMinutes.HasValue)
            {
                SelectedReminderOption = ReminderOptions
                    .FirstOrDefault(r => r.Minutes == value.ReminderOffsetMinutes.Value);
            }
            else
            {
                SelectedReminderOption = null;
            }
        }
        else
        {
            EventTime = string.Empty;
            SelectedReminderOption = null;
        }
        OnPropertyChanged(nameof(SelectedEventDate));
        OnPropertyChanged(nameof(SelectedEvent));
        OnPropertyChanged(nameof(SelectedDisplayItem));
    }
    
    // Обновление событий при изменении UntilDate
    partial void OnUntilDateChanged(DateTime? value)
    {
        // При изменении даты перезагружаем события
        _ = LoadEventsDataAsync();
    }

    // Обновление EventDate при изменении времени
    partial void OnEventTimeChanged(string value)
    {
        if (SelectedEvent != null && !string.IsNullOrEmpty(value))
        {
            if (TimeSpan.TryParse(value, out var time))
            {
                var newDate = SelectedEvent.EventDate.Date + time;
                SelectedEvent.EventDate = newDate;
            }
        }
    }

    partial void OnSelectedReminderOptionChanged(ReminderOption? value)
    {
        if (SelectedEvent != null && value != null)
        {
            SelectedEvent.ReminderOffsetMinutes = value.Minutes;
            OnPropertyChanged(nameof(SelectedEvent));
        }
    }

    public record ReminderOption(string Name, int Minutes)
    {
        public override string ToString() => Name;
    }
}