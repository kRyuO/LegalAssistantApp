using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Models;
using LegalAssistantApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LegalAssistantApp.ViewModels;

public partial class CounterpartyViewModel : ObservableObject
{
    private readonly CounterpartyService _service;
    private Counterparty? _selectedCounterparty;
    private AuditHistory? _lastAuditHistory;

    public CounterpartyViewModel(CounterpartyService service)
    {
        _service = service;
        Counterparties = new ObservableCollection<Counterparty>();
    }

    [ObservableProperty]
    private ObservableCollection<Counterparty> _counterparties;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _fnsApiKey = string.Empty;

    public Counterparty? SelectedCounterparty
    {
        get => _selectedCounterparty;
        set
        {
            if (SetProperty(ref _selectedCounterparty, value) && value != null)
            {
                _ = LoadLastAuditHistoryAsync();
            }
        }
    }

    public AuditHistory? LastAuditHistory
    {
        get => _lastAuditHistory;
        set => SetProperty(ref _lastAuditHistory, value);
    }

    // Вычисляемое свойство для цвета статуса
    public string StatusColor
    {
        get
        {
            if (SelectedCounterparty == null)
                return "Gray";

            // Логика определения цвета
            return SelectedCounterparty.RiskLevel switch
            {
                "Low" => "Green",
                "Medium" => "Orange",
                "High" => "Red",
                _ => "Gray"
            };
        }
    }

    // Свойства для удобства привязки в XAML
    public string LastCheckDate => LastAuditHistory?.CheckDate.ToString("dd.MM.yyyy HH:mm") ?? "Не проверялся";
    public string LastCheckSource => LastAuditHistory?.Source ?? "Нет данных";
    public string LastCheckResult => LastAuditHistory?.RiskLevel ?? "Неизвестно";
    public bool HasProblems => LastAuditHistory?.HasProblems ?? false;

    // Публичный метод для загрузки контрагентов
    [RelayCommand]
    public async Task LoadCounterpartiesAsync()
    {
        try
        {
            var counterparties = await _service.GetAllCounterpartiesAsync();
            Counterparties.Clear();
            foreach (var cp in counterparties)
            {
                Counterparties.Add(cp);
            }
            StatusMessage = $"Загружено {Counterparties.Count} контрагентов";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            var counterparties = await _service.SearchCounterpartiesAsync(SearchText);
            Counterparties.Clear();
            foreach (var cp in counterparties)
            {
                Counterparties.Add(cp);
            }
            StatusMessage = $"Найдено {Counterparties.Count} контрагентов";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка поиска: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddNew()
    {
        var newCounterparty = new Counterparty
        {
            Name = "Новый контрагент",
            Status = "Active",
            RiskLevel = "Medium",
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
        Counterparties.Add(newCounterparty);
        SelectedCounterparty = newCounterparty;
        StatusMessage = "Создан новый контрагент";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedCounterparty == null) return;

        try
        {
            if (SelectedCounterparty.Id == 0)
            {
                await _service.CreateCounterpartyAsync(SelectedCounterparty);
                StatusMessage = "Контрагент создан";
            }
            else
            {
                await _service.UpdateCounterpartyAsync(SelectedCounterparty);
                StatusMessage = "Контрагент сохранен";
            }
            await LoadCounterpartiesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedCounterparty == null || SelectedCounterparty.Id == 0) return;

        try
        {
            await _service.DeleteCounterpartyAsync(SelectedCounterparty.Id);
            Counterparties.Remove(SelectedCounterparty);
            SelectedCounterparty = null;
            LastAuditHistory = null;
            StatusMessage = "Контрагент удален";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка удаления: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CheckWithFnsAsync()
    {
        if (SelectedCounterparty == null)
        {
            StatusMessage = "Выберите контрагента";
            return;
        }

        if (string.IsNullOrEmpty(FnsApiKey))
        {
            StatusMessage = "Введите ключ API ФНС";
            return;
        }

        if (string.IsNullOrEmpty(SelectedCounterparty.INN))
        {
            StatusMessage = "У контрагента не указан ИНН";
            return;
        }

        StatusMessage = "Проверка через ФНС...";

        try
        {
            var history = await _service.CheckWithFnsAsync(
                SelectedCounterparty.Id,
                SelectedCounterparty.INN,
                FnsApiKey);

            if (history != null)
            {
                LastAuditHistory = history;
                StatusMessage = "Проверка через ФНС выполнена успешно";
                OnPropertyChanged(nameof(LastCheckDate));
                OnPropertyChanged(nameof(LastCheckSource));
                OnPropertyChanged(nameof(LastCheckResult));
                OnPropertyChanged(nameof(HasProblems));
                OnPropertyChanged(nameof(StatusColor));
            }
            else
            {
                StatusMessage = "Не удалось получить данные из ФНС";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при проверке: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCounterpartiesAsync();
    }

    private async Task LoadLastAuditHistoryAsync()
    {
        if (SelectedCounterparty == null)
        {
            LastAuditHistory = null;
            return;
        }

        try
        {
            LastAuditHistory = await _service.GetLastAuditHistoryAsync(SelectedCounterparty.Id);
            OnPropertyChanged(nameof(LastCheckDate));
            OnPropertyChanged(nameof(LastCheckSource));
            OnPropertyChanged(nameof(LastCheckResult));
            OnPropertyChanged(nameof(HasProblems));
            OnPropertyChanged(nameof(StatusColor));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке истории аудита: {ex.Message}");
            LastAuditHistory = null;
        }
    }

    // Метод для обновления статуса после изменения данных
    public void UpdateStatus()
    {
        OnPropertyChanged(nameof(StatusColor));
        OnPropertyChanged(nameof(LastCheckDate));
        OnPropertyChanged(nameof(LastCheckSource));
        OnPropertyChanged(nameof(LastCheckResult));
        OnPropertyChanged(nameof(HasProblems));
    }
}