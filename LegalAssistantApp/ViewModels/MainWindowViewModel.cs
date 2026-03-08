using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Services;
using LegalAssistantApp.Views;
using LegalAssistantApp.Localization;
using System;
using System.Timers;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using System.Collections.ObjectModel;
using LegalAssistantApp.Models;

namespace LegalAssistantApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly CounterpartyService _counterpartyService;
    private readonly DocumentService _documentService;
    private readonly EventService _eventService;
    private readonly EmailService _emailService;
    private readonly SettingsService _settingsService;
    private Timer _timer;
    private CounterpartyViewModel? _counterpartyViewModel;
    private DocumentViewModel? _documentViewModel;
    private EventsViewModel? _eventsViewModel;
    private SettingsViewModel? _settingsViewModel;

    private object? _currentContent;
    public object? CurrentContent
    {
        get => _currentContent;
        set
        {
            if (SetProperty(ref _currentContent, value))
            {
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    private string _welcomeMessage = "Добро пожаловать в систему";

    public string NavigationLabel => LocalizationService.Instance["Navigation"];
    public string CounterpartiesLabel => LocalizationService.Instance["Counterparties"];
    public string DocumentsLabel => LocalizationService.Instance["Documents"];
    public string EventsLabel => LocalizationService.Instance["Events"];
    public string SettingsLabel => LocalizationService.Instance["Settings"];
    public string LogoutLabel => LocalizationService.Instance["Logout"];
    public string UpcomingEventsTitle => LocalizationService.Instance["UpcomingEventsTitle"];

    public void OnLanguageChanged()
    {
        var loc = LocalizationService.Instance;
        if (CurrentContent == this)
        {
            WelcomeMessage = $"{loc["WelcomeMessage"]}, {CurrentUserName}! Сегодня {DateTime.Now:dd.MM.yyyy}";
        }
        else if (CurrentContent is DocumentViewModel)
        {
            WelcomeMessage = $"{loc["Documents"]} - {CurrentUserName}";
        }
        else if (CurrentContent is EventsViewModel)
        {
            WelcomeMessage = $"{loc["EventCalendar"]} - {CurrentUserName}";
        }
        else if (CurrentContent is CounterpartyViewModel)
        {
            WelcomeMessage = $"{loc["Counterparties"]} - {CurrentUserName}";
        }
        else if (CurrentContent is SettingsViewModel)
        {
            WelcomeMessage = $"{loc["Settings"]} - {CurrentUserName}";
        }
        OnPropertyChanged(nameof(WelcomeMessage));
        OnPropertyChanged(nameof(NavigationLabel));
        OnPropertyChanged(nameof(CounterpartiesLabel));
        OnPropertyChanged(nameof(DocumentsLabel));
        OnPropertyChanged(nameof(EventsLabel));
        OnPropertyChanged(nameof(SettingsLabel));
        OnPropertyChanged(nameof(LogoutLabel));
        OnPropertyChanged(nameof(UpcomingEventsTitle));
    }

    [ObservableProperty]
    private string _currentUserName = "Пользователь";

    [ObservableProperty]
    private string _currentUserRole = string.Empty;

    [ObservableProperty]
    private int _currentUserId;

    [ObservableProperty]
    private DateTime _currentDateTime = DateTime.Now;

    [ObservableProperty]
    private int _reminderCount;

    [ObservableProperty]
    private ObservableCollection<DocumentEvent> _homeUpcomingEvents = new();

    public MainWindowViewModel(
        CounterpartyService counterpartyService,
        DocumentService documentService,
        EventService eventService,
        string? currentUserName = null,
        int currentUserId = 0,
        string? currentUserRole = null)
    {
        _counterpartyService = counterpartyService;
        _documentService = documentService;
        _eventService = eventService;
        _settingsService = new SettingsService();
        _emailService = new EmailService(_settingsService);

        CurrentUserName = string.IsNullOrWhiteSpace(currentUserName)
            ? "Пользователь"
            : currentUserName;
        CurrentUserId = currentUserId;
        CurrentUserRole = currentUserRole ?? string.Empty;

        CurrentContent = this; 

        _timer = new Timer(1000);
        _timer.Elapsed += (s, e) => CurrentDateTime = DateTime.Now;
        _timer.Start();

        _ = LoadHomeUpcomingEventsAsync();

        StartReminderCheck();
    }

    public string ReminderText => ReminderCount > 0 ? $" ({ReminderCount})" : "";

    private void StartReminderCheck()
    {
        var reminderTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        reminderTimer.Elapsed += async (s, e) =>
        {
            await CheckRemindersAsync();
        };
        reminderTimer.AutoReset = true;
        reminderTimer.Start();
    }

    private async Task CheckRemindersAsync()
    {
        try
        {
            var reminders = await _eventService.GetEventsForReminderAsync();
            ReminderCount = reminders.Count;

            if (reminders.Count > 0)
            {
                Console.WriteLine($"Найдено {reminders.Count} напоминаний");

                // Отправляем Email-уведомления по каждому напоминанию
                foreach (var reminder in reminders)
                {
                    try
                    {
                        await _emailService.SendReminderEmailAsync(reminder);
                    }
                    catch (Exception sendEx)
                    {
                        Console.WriteLine($"Ошибка при отправке email по напоминанию {reminder.Id}: {sendEx.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при проверке напоминаний: {ex.Message}");
        }
    }

    private async Task LoadHomeUpcomingEventsAsync()
    {
        try
        {
            var events = await _eventService.GetUpcomingEventsAsync(DateTime.Now.AddDays(7));
            HomeUpcomingEvents.Clear();
            foreach (var e in events)
            {
                HomeUpcomingEvents.Add(e);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке событий для главной панели: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Logout()
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();

        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }

    [RelayCommand]
    private async Task ShowCounterparties()
    {
        _counterpartyViewModel ??= new CounterpartyViewModel(_counterpartyService);

        CurrentContent = _counterpartyViewModel;
        var loc = LocalizationService.Instance;
        WelcomeMessage = $"{loc["Counterparties"]} - {CurrentUserName}";

        await _counterpartyViewModel.LoadCounterpartiesAsync();
    }

    [RelayCommand]
    private async Task ShowDocuments()
    {
        try
        {
            if (_documentViewModel == null)
            {
                _documentViewModel = new DocumentViewModel(_documentService);
            }

            CurrentContent = _documentViewModel;
            WelcomeMessage = $"Документы - {CurrentUserName}";
            

            await _documentViewModel.LoadDocumentsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при открытии раздела документов: {ex}");
        }
    }

    [RelayCommand]
    private async Task ShowEvents()
    {
        if (_eventsViewModel == null)
        {
            _eventsViewModel = new EventsViewModel(_eventService, _documentService, _counterpartyService);
            await _eventsViewModel.InitializeAsync();
            await _eventsViewModel.LoadEventsAsync();
        }

        CurrentContent = _eventsViewModel;
        var loc = LocalizationService.Instance;
        WelcomeMessage = $"{loc["EventCalendar"]} - {CurrentUserName}";
        
        if (_eventsViewModel.UpcomingEvents.Count == 0)
        {
            await _eventsViewModel.LoadEventsAsync();
        }

        await CheckRemindersAsync();
    }

    [RelayCommand]
    private void ShowSettings()
    {
        if (_settingsViewModel == null)
        {
            var settingsService = new Services.SettingsService();
            _settingsViewModel = new ViewModels.SettingsViewModel(settingsService);
        }
        CurrentContent = _settingsViewModel;
        var loc = LocalizationService.Instance;
        WelcomeMessage = $"{loc["Settings"]} - {CurrentUserName}";
    }

    [RelayCommand]
    private void ShowHome()
    {
        CurrentContent = this;
        var loc = LocalizationService.Instance;
        WelcomeMessage = $"{loc["WelcomeMessage"]}, {CurrentUserName}! Сегодня {DateTime.Now:dd.MM.yyyy}";
    }

    [RelayCommand]
    private async Task RefreshReminders()
    {
        await CheckRemindersAsync();

        if (CurrentContent is EventsViewModel eventsViewModel)
        {
            await eventsViewModel.CheckReminders();
        }
    }

    partial void OnCurrentUserNameChanged(string value)
    {
        if (CurrentContent == this)
        {
            WelcomeMessage = $"Добро пожаловать, {value}! Сегодня {DateTime.Now:dd.MM.yyyy}";
        }
    }

    partial void OnCurrentDateTimeChanged(DateTime value)
    {
        if (CurrentContent == this)
        {
            WelcomeMessage = $"Добро пожаловать, {CurrentUserName}! Сегодня {value:dd.MM.yyyy}";
        }
    }

    public T? GetCurrentViewModel<T>() where T : class
    {
        return CurrentContent as T;
    }

    public bool IsCurrentContent<T>() where T : class
    {
        return CurrentContent is T;
    }

    public string GetStatisticsSummary()
    {
        int counterpartiesCount = _counterpartyViewModel?.Counterparties.Count ?? 0;
        int documentsCount = _documentViewModel?.Documents.Count ?? 0;
        int eventsCount = _eventsViewModel?.UpcomingEvents.Count ?? 0;

        return $"Контрагенты: {counterpartiesCount}, Документы: {documentsCount}, События: {eventsCount}, Напоминания: {ReminderCount}";
    }

    ~MainWindowViewModel()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}