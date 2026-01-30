// SettingsViewModel.cs
using OrderingSpecialEquipment.Commands;
using OrderingSpecialEquipment.Services;
using System;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IThemeService _themeService;
        private readonly IConnectionService _connectionService;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        private string _selectedTheme;
        private string _connectionType;
        private string _connectionString;

        public SettingsViewModel(
            IThemeService themeService,
            IConnectionService connectionService,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _themeService = themeService;
            _connectionService = connectionService;
            _messageService = messageService;
            _authorizationService = authorizationService;

            if (!_authorizationService.IsCurrentUserSystemAdmin())
            {
                _messageService.ShowErrorMessage("У вас нет прав на изменение настроек приложения.", "Нет доступа");
                return;
            }

            _selectedTheme = _themeService.LoadThemePreference();
            var connSettings = _connectionService.LoadConnectionSettings();
            _connectionType = connSettings.type.ToString();
            _connectionString = connSettings.connectionString;

            SaveSettingsCommand = new RelayCommand(_ => SaveSettings(), _ => true);
            TestConnectionCommand = new RelayCommand(async _ => await TestConnectionAsync(), _ => !string.IsNullOrWhiteSpace(ConnectionString));
        }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set => SetProperty(ref _selectedTheme, value);
        }

        public string ConnectionType
        {
            get => _connectionType;
            set => SetProperty(ref _connectionType, value);
        }

        public string ConnectionString
        {
            get => _connectionString;
            set => SetProperty(ref _connectionString, value);
        }

        public string[] AvailableThemes => _themeService.GetAvailableThemes();
        public string[] AvailableConnectionTypes => Enum.GetNames(typeof(ConnectionType));

        public ICommand SaveSettingsCommand { get; }
        public ICommand TestConnectionCommand { get; }

        private void SaveSettings()
        {
            try
            {
                // Сохранение темы
                _themeService.ApplyTheme(SelectedTheme);
                _themeService.SaveThemePreference(SelectedTheme);

                // Сохранение настроек подключения
                if (Enum.TryParse<ConnectionType>(ConnectionType, out var connTypeParsed))
                {
                    _connectionService.SaveConnectionSettings(connTypeParsed, ConnectionString);
                    // Сообщаем пользователю, что для применения настроек подключения требуется перезапуск
                    _messageService.ShowInfoMessage("Настройки сохранены. Изменения в настройках подключения вступят в силу после перезапуска приложения.", "Успех");
                }
                else
                {
                    _messageService.ShowErrorMessage("Некорректный тип подключения.", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка сохранения настроек: {ex.Message}", "Ошибка");
            }
        }

        private async Task TestConnectionAsync()
        {
            try
            {
                if (Enum.TryParse<ConnectionType>(ConnectionType, out var connTypeParsed))
                {
                    bool success = _connectionService.TestConnection(ConnectionString);
                    if (success)
                    {
                        _messageService.ShowInfoMessage("Подключение успешно установлено.", "Успех");
                    }
                    else
                    {
                        _messageService.ShowErrorMessage("Не удалось подключиться к базе данных.", "Ошибка подключения");
                    }
                }
                else
                {
                    _messageService.ShowErrorMessage("Некорректный тип подключения.", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка тестирования подключения: {ex.Message}", "Ошибка");
            }
        }
    }
}