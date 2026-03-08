using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LegalAssistantApp.Localization;

public class LocalizationService : INotifyPropertyChanged
{
    private static LocalizationService? _instance;
    private Dictionary<string, Dictionary<string, string>> _translations;
    private string _currentLanguage = "ru-RU";

    public static LocalizationService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LocalizationService();
            }
            return _instance;
        }
    }

    private LocalizationService()
    {
        _translations = new Dictionary<string, Dictionary<string, string>>();
        InitializeTranslations();
    }

    private void InitializeTranslations()
    {
        // Русский язык
        var ru = new Dictionary<string, string>
        {
            { "WelcomeMessage", "Добро пожаловать в систему" },
            { "Documents", "Документы" },
            { "Events", "События" },
            { "Counterparties", "Контрагенты" },
            { "Settings", "Настройки" },
            { "DocumentManagement", "Управление документами" },
            { "EventCalendar", "Календарь событий" },
            { "ApplicationSettings", "Настройки приложения" },
            { "Theme", "Тема оформления" },
            { "Language", "Язык интерфейса" },
            { "SaveSettings", "Сохранить настройки" },
            { "SettingsSaved", "Настройки сохранены" },
            { "ThemeChanged", "Тема изменена" },
            { "LanguageChanged", "Язык изменён" },
            { "DocumentList", "Список документов" },
            { "UpcomingEvents", "Предстоящие события" },
            { "EditDocument", "Редактирование документа" },
            { "EditEvent", "Редактирование события" },
            { "SelectDocument", "Выберите документ из списка для редактирования" },
            { "SelectEvent", "Выберите событие из списка для редактирования" },
            { "Save", "Сохранить" },
            { "Delete", "Удалить" },
            { "Add", "Добавить" },
            { "Open", "Открыть" },
            { "Complete", "Завершить" },
            { "Refresh", "Обновить" },
            { "Export", "Экспорт" },
            { "Search", "Поиск" },
            { "ScanDisks", "Сканировать диски" },
            { "PDFReport", "Отчёт PDF" },
            { "Name", "Название" },
            { "DocumentNumber", "Номер документа" },
            { "DocumentType", "Тип документа" },
            { "Status", "Статус" },
            { "Date", "Дата" },
            { "Confidential", "Конфиденциальный документ" },
            { "File", "Файл" },
            { "Content", "Содержание (краткое описание)" },
            { "EventTitle", "Название события" },
            { "Description", "Описание" },
            { "EventDate", "Дата события" },
            { "EventTime", "Время (HH:mm)" },
            { "EventType", "Тип события" },
            { "RelatedDocument", "Связанный документ" },
            { "Counterparty", "Контрагент" },
            { "DocumentsLoaded", "Загружено {0} документов" },
            { "EventsLoaded", "Загружено {0} событий" },
            { "DocumentsCount", "Количество документов: {0}" },
            { "EventsCount", "Количество событий: {0}" },
            { "Navigation", "Навигация" },
            { "Logout", "Выйти" },
            { "UpcomingEventsTitle", "Ближайшие события" },
            { "EventsUntil", "События до:" },
            { "CounterpartiesList", "Список контрагентов" },
            { "CompanyName", "Наименование" },
            { "INN", "ИНН" },
            { "KPP", "КПП" },
            { "OGRN", "ОГРН" },
            { "Address", "Адрес" },
            { "Phone", "Телефон" },
            { "Email", "Email" },
            { "ContactPerson", "Контактное лицо" },
            { "FnsApi", "API ФНС:" },
            { "FnsApiKeyPlaceholder", "Введите ключ доступа к API ФНС" },
            { "Check", "Проверить" },
            { "EditCounterparty", "Редактирование контрагента" },
            { "SelectCounterparty", "Выберите контрагента из списка для редактирования" }
        };

        // Английский язык
        var en = new Dictionary<string, string>
        {
            { "WelcomeMessage", "Welcome to the system" },
            { "Documents", "Documents" },
            { "Events", "Events" },
            { "Counterparties", "Counterparties" },
            { "Settings", "Settings" },
            { "DocumentManagement", "Document Management" },
            { "EventCalendar", "Event Calendar" },
            { "ApplicationSettings", "Application Settings" },
            { "Theme", "Theme" },
            { "Language", "Interface Language" },
            { "SaveSettings", "Save Settings" },
            { "SettingsSaved", "Settings saved" },
            { "ThemeChanged", "Theme changed" },
            { "LanguageChanged", "Language changed" },
            { "DocumentList", "Document List" },
            { "UpcomingEvents", "Upcoming Events" },
            { "EditDocument", "Edit Document" },
            { "EditEvent", "Edit Event" },
            { "SelectDocument", "Select a document from the list for editing" },
            { "SelectEvent", "Select an event from the list for editing" },
            { "Save", "Save" },
            { "Delete", "Delete" },
            { "Add", "Add" },
            { "Open", "Open" },
            { "Complete", "Complete" },
            { "Refresh", "Refresh" },
            { "Export", "Export" },
            { "Search", "Search" },
            { "ScanDisks", "Scan Disks" },
            { "PDFReport", "PDF Report" },
            { "Name", "Name" },
            { "DocumentNumber", "Document Number" },
            { "DocumentType", "Document Type" },
            { "Status", "Status" },
            { "Date", "Date" },
            { "Confidential", "Confidential Document" },
            { "File", "File" },
            { "Content", "Content (brief description)" },
            { "EventTitle", "Event Title" },
            { "Description", "Description" },
            { "EventDate", "Event Date" },
            { "EventTime", "Time (HH:mm)" },
            { "EventType", "Event Type" },
            { "RelatedDocument", "Related Document" },
            { "Counterparty", "Counterparty" },
            { "DocumentsLoaded", "{0} documents loaded" },
            { "EventsLoaded", "{0} events loaded" },
            { "DocumentsCount", "Number of documents: {0}" },
            { "EventsCount", "Number of events: {0}" },
            { "Navigation", "Navigation" },
            { "Logout", "Logout" },
            { "UpcomingEventsTitle", "Upcoming Events" },
            { "EventsUntil", "Events until:" },
            { "CounterpartiesList", "Counterparties List" },
            { "CompanyName", "Company Name" },
            { "INN", "INN" },
            { "KPP", "KPP" },
            { "OGRN", "OGRN" },
            { "Address", "Address" },
            { "Phone", "Phone" },
            { "Email", "Email" },
            { "ContactPerson", "Contact Person" },
            { "FnsApi", "FNS API:" },
            { "FnsApiKeyPlaceholder", "Enter FNS API access key" },
            { "Check", "Check" },
            { "EditCounterparty", "Edit Counterparty" },
            { "SelectCounterparty", "Select a counterparty from the list for editing" }
        };

        _translations["ru-RU"] = ru;
        _translations["en-US"] = en;
    }

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentCulture));
                
                // Устанавливаем культуру для приложения
                var culture = new CultureInfo(value);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
        }
    }

    public CultureInfo CurrentCulture => new CultureInfo(_currentLanguage);

    public string this[string key]
    {
        get
        {
            if (_translations.TryGetValue(_currentLanguage, out var langDict) &&
                langDict.TryGetValue(key, out var value))
            {
                return value;
            }
            
            // Fallback на русский
            if (_translations.TryGetValue("ru-RU", out var ruDict) &&
                ruDict.TryGetValue(key, out var ruValue))
            {
                return ruValue;
            }
            
            return key;
        }
    }

    public string GetString(string key, params object[] args)
    {
        var format = this[key];
        return args.Length > 0 ? string.Format(format, args) : format;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

