using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LegalAssistantApp.ViewModels;
using System;
using DataGrid = Avalonia.Controls.DataGrid;

namespace LegalAssistantApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
    }
    public MainWindow(string userName) : this()
    {
        var context = new Data.AppDbContext();
        var counterpartyService = new Services.CounterpartyService(context);
        var documentService = new Services.DocumentService(context);
        var eventService = new Services.EventService(context);

        DataContext = new MainWindowViewModel(
            counterpartyService,
            documentService,
            eventService,
            userName
        );
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void MainWindow_Loaded(object? sender, EventArgs e)
    {
        this.Loaded -= MainWindow_Loaded;
    }

    private void OnDocumentsDataGridAttached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (sender is DataGrid dataGrid)
        {
            System.Diagnostics.Debug.WriteLine("DocumentsDataGrid attached to visual tree");
            
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (dataGrid.DataContext is DocumentViewModel docVM)
                {
                    var currentSource = dataGrid.ItemsSource;
                    dataGrid.ItemsSource = null;
                    dataGrid.ItemsSource = docVM.Documents;
                    System.Diagnostics.Debug.WriteLine($"Принудительно установлен ItemsSource для DocumentsDataGrid: {docVM.Documents.Count} элементов");
                    
                    System.Diagnostics.Debug.WriteLine($"DataGrid.ItemsSource type: {dataGrid.ItemsSource?.GetType().FullName}");
                    System.Diagnostics.Debug.WriteLine($"DataGrid.ItemsSource count: {(dataGrid.ItemsSource as System.Collections.ICollection)?.Count ?? -1}");
                    System.Diagnostics.Debug.WriteLine($"DataGrid.Height: {dataGrid.Height}");
                    System.Diagnostics.Debug.WriteLine($"DataGrid.MinHeight: {dataGrid.MinHeight}");
                    System.Diagnostics.Debug.WriteLine($"DataGrid.IsVisible: {dataGrid.IsVisible}");
                    System.Diagnostics.Debug.WriteLine($"DataGrid.Opacity: {dataGrid.Opacity}");
                    
                    dataGrid.InvalidateVisual();
                    dataGrid.InvalidateArrange();
                    dataGrid.InvalidateMeasure();
                }
            }, Avalonia.Threading.DispatcherPriority.Loaded);
        }
    }

    private void OnEventsDataGridAttached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (sender is DataGrid dataGrid)
        {
            System.Diagnostics.Debug.WriteLine("EventsDataGrid attached to visual tree");
            
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (dataGrid.DataContext is EventsViewModel eventsVM)
                {
                    var currentSource = dataGrid.ItemsSource;
                    dataGrid.ItemsSource = null;
                    dataGrid.ItemsSource = eventsVM.UpcomingEvents;
                    System.Diagnostics.Debug.WriteLine($"Принудительно установлен ItemsSource для EventsDataGrid: {eventsVM.UpcomingEvents.Count} элементов");
                    
                    System.Diagnostics.Debug.WriteLine($"EventsDataGrid.ItemsSource type: {dataGrid.ItemsSource?.GetType().FullName}");
                    System.Diagnostics.Debug.WriteLine($"EventsDataGrid.ItemsSource count: {(dataGrid.ItemsSource as System.Collections.ICollection)?.Count ?? -1}");
                    System.Diagnostics.Debug.WriteLine($"EventsDataGrid.Height: {dataGrid.Height}");
                    System.Diagnostics.Debug.WriteLine($"EventsDataGrid.IsVisible: {dataGrid.IsVisible}");
                    
                    dataGrid.InvalidateVisual();
                    dataGrid.InvalidateArrange();
                    dataGrid.InvalidateMeasure();
                }
            }, Avalonia.Threading.DispatcherPriority.Loaded);
        }
    }
}