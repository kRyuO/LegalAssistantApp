using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Models;
using LegalAssistantApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LegalAssistantApp.ViewModels;

public partial class EventsViewModel : ObservableObject
{
    private readonly DocumentService _documentService;

    public EventsViewModel(DocumentService documentService)
    {
        _documentService = documentService;
        UpcomingEvents = new ObservableCollection<DocumentEvent>();
    }

    public ObservableCollection<DocumentEvent> UpcomingEvents { get; }

    [ObservableProperty]
    private DateTime _untilDate = DateTime.UtcNow.AddDays(30);

    public async Task LoadEventsAsync()
    {
        UpcomingEvents.Clear();
        var items = await _documentService.GetUpcomingEventsAsync(UntilDate);
        foreach (var e in items)
        {
            UpcomingEvents.Add(e);
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadEventsAsync();
    }

    [RelayCommand]
    private async Task MarkCompleted(DocumentEvent? documentEvent)
    {
        if (documentEvent == null)
            return;

        await _documentService.MarkEventCompletedAsync(documentEvent.Id);
        await LoadEventsAsync();
    }
}


