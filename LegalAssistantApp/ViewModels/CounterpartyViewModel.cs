using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Models;
using LegalAssistantApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;

namespace LegalAssistantApp.ViewModels;

public partial class CounterpartyViewModel : ObservableObject
{
    private readonly CounterpartyService _service;

    public CounterpartyViewModel(CounterpartyService service)
    {
        _service = service;
        Counterparties = new ObservableCollection<Counterparty>();
    }

    public ObservableCollection<Counterparty> Counterparties { get; }

    [ObservableProperty]
    private Counterparty? _selectedCounterparty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _fnsApiKey = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private AuditHistory? _lastAuditHistory;

    public async Task LoadCounterpartiesAsync()
    {
        Counterparties.Clear();
        var items = await _service.GetCounterpartiesAsync();
        foreach (var c in items)
        {
            Counterparties.Add(c);
        }
    }

    private async Task LoadLastAuditAsync()
    {
        if (SelectedCounterparty == null || SelectedCounterparty.Id == 0)
        {
            LastAuditHistory = null;
            return;
        }

        LastAuditHistory = await _service.GetLastAuditHistoryAsync(SelectedCounterparty.Id);
    }

    [RelayCommand]
    private async Task Search()
    {
        Counterparties.Clear();
        var items = await _service.GetCounterpartiesAsync(SearchText);
        foreach (var c in items)
        {
            Counterparties.Add(c);
        }
    }

    [RelayCommand]
    private void AddNew()
    {
        var counterparty = new Counterparty
        {
            Name = "Новый контрагент",
            Status = "Active",
            RiskLevel = "Unknown",
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            CreatedByUserId = 1 // временно: администратор по умолчанию
        };
        Counterparties.Add(counterparty);
        SelectedCounterparty = counterparty;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (SelectedCounterparty == null)
            return;

        SelectedCounterparty.UpdatedDate = DateTime.UtcNow;
        await _service.SaveCounterpartyAsync(SelectedCounterparty);
        await LoadCounterpartiesAsync();
        await LoadLastAuditAsync();
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedCounterparty == null || SelectedCounterparty.Id == 0)
            return;

        await _service.DeleteCounterpartyAsync(SelectedCounterparty.Id);
        await LoadCounterpartiesAsync();
        LastAuditHistory = null;
    }

    [RelayCommand]
    private async Task CheckWithFns()
    {
        if (SelectedCounterparty == null || string.IsNullOrWhiteSpace(SelectedCounterparty.INN))
        {
            StatusMessage = "Выберите контрагента с заполненным ИНН.";
            return;
        }

        if (string.IsNullOrWhiteSpace(FnsApiKey))
        {
            StatusMessage = "Укажите ключ API ФНС/провайдера (FNS API Key).";
            return;
        }

        StatusMessage = "Запрос к API ФНС...";

        var history = await _service.CheckWithFnsAsync(SelectedCounterparty.Id, SelectedCounterparty.INN, FnsApiKey, CancellationToken.None);

        if (history != null)
        {
            LastAuditHistory = history;
        }

        StatusMessage = history != null
            ? "Данные ФНС получены и сохранены в истории проверок."
            : "Не удалось получить данные от API ФНС.";
    }
}
