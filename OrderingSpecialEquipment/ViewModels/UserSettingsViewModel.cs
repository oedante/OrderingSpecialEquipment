using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// ViewModel для окна настроек пользователя
    /// </summary>
    public class UserSettingsViewModel : ViewModelBase
    {
        #region Поля

        private readonly IUserSettingsService _settingsService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IDatabaseService _databaseService;
        private readonly IDbContextFactory _contextFactory;

        private User _selectedUser;
        private List<User> _allUsers;
        private ObservableCollection<SettingItem> _settings;
        private SettingItem _selectedSetting;
        private string _newSettingKey;
        private string _newSettingValue;
        private bool _isLoading;
        private string _statusMessage;
        private bool _isEditing;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор ViewModel настроек пользователя
        /// </summary>
        public UserSettingsViewModel(
            IUserSettingsService settingsService,
            IAuthenticationService authenticationService,
            IDatabaseService databaseService,
            IDbContextFactory contextFactory)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            InitializeCommands();
            _settings = new ObservableCollection<SettingItem>();
        }

        #endregion

        #region Свойства

        /// <summary>
        /// Все пользователи
        /// </summary>
        public List<User> AllUsers
        {
            get => _allUsers;
            set => SetProperty(ref _allUsers, value);
        }

        /// <summary>
        /// Выбранный пользователь
        /// </summary>
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    _ = LoadSettingsAsync();
                }
            }
        }

        /// <summary>
        /// Настройки выбранного пользователя
        /// </summary>
        public ObservableCollection<SettingItem> Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        /// <summary>
        /// Выбранная настройка
        /// </summary>
        public SettingItem SelectedSetting
        {
            get => _selectedSetting;
            set => SetProperty(ref _selectedSetting, value);
        }

        /// <summary>
        /// Ключ новой настройки
        /// </summary>
        public string NewSettingKey
        {
            get => _newSettingKey;
            set => SetProperty(ref _newSettingKey, value);
        }

        /// <summary>
        /// Значение новой настройки
        /// </summary>
        public string NewSettingValue
        {
            get => _newSettingValue;
            set => SetProperty(ref _newSettingValue, value);
        }

        /// <summary>
        /// Загрузка
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Статусное сообщение
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Режим редактирования
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public User CurrentUser => _authenticationService.CurrentUser;

        /// <summary>
        /// Является ли администратором
        /// </summary>
        public bool IsAdmin => _authenticationService.CurrentUserRole?.SPEC_SystemAdmin == true ||
                              _authenticationService.CurrentUserRole?.SPEC_ManageUsers == true;

        #endregion

        #region Команды

        public ICommand LoadUsersCommand { get; private set; }
        public ICommand LoadSettingsCommand { get; private set; }
        public ICommand SaveSettingCommand { get; private set; }
        public ICommand DeleteSettingCommand { get; private set; }
        public ICommand AddSettingCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        public ICommand ClearAllSettingsCommand { get; private set; }
        public ICommand ExportSettingsCommand { get; private set; }
        public ICommand ImportSettingsCommand { get; private set; }

        #endregion

        #region Инициализация команд

        private void InitializeCommands()
        {
            // ИСПРАВЛЕНО: Добавлены параметры object? для всех команд
            LoadUsersCommand = new RelayCommand(async (param) => await LoadUsersAsync());
            LoadSettingsCommand = new RelayCommand(async (param) => await LoadSettingsAsync(),
                (param) => SelectedUser != null);
            SaveSettingCommand = new RelayCommand(async (param) => await SaveSettingAsync(),
                (param) => SelectedUser != null && IsEditing);
            DeleteSettingCommand = new RelayCommand<SettingItem>(async (item) => await DeleteSettingAsync(item),
                (item) => item != null && SelectedUser != null);
            AddSettingCommand = new RelayCommand((param) => StartAddSetting(),
                (param) => SelectedUser != null);
            CancelEditCommand = new RelayCommand((param) => CancelEdit());
            ClearAllSettingsCommand = new RelayCommand(async (param) => await ClearAllSettingsAsync(),
                (param) => SelectedUser != null && Settings.Any());
            ExportSettingsCommand = new RelayCommand((param) => ExportSettings(),
                (param) => SelectedUser != null && Settings.Any());
            ImportSettingsCommand = new RelayCommand(async (param) => await ImportSettingsAsync(),
                (param) => SelectedUser != null);
        }

        #endregion

        #region Методы загрузки

        /// <summary>
        /// Загрузка пользователей
        /// </summary>
        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Загрузка пользователей...";

                using var context = _contextFactory.CreateDbContext();
                var users = await context.Users
                    .Include(u => u.Role)
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                AllUsers = users;

                // Если администратор, показываем всех пользователей
                if (!IsAdmin)
                {
                    // Обычный пользователь видит только свои настройки
                    SelectedUser = AllUsers.FirstOrDefault(u => u.Id == CurrentUser?.Id);
                }
                else if (AllUsers.Any() && SelectedUser == null)
                {
                    SelectedUser = AllUsers.First();
                }

                StatusMessage = $"Загружено пользователей: {AllUsers.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Загрузка настроек выбранного пользователя
        /// </summary>
        private async Task LoadSettingsAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Загрузка настроек пользователя {SelectedUser.FullName}...";

                var settings = await _settingsService.GetAllSettingsAsync(SelectedUser.Id);

                Settings.Clear();
                foreach (var kvp in settings.OrderBy(k => k.Key))
                {
                    Settings.Add(new SettingItem
                    {
                        Key = kvp.Key,
                        Value = kvp.Value,
                        ValueType = kvp.Value?.GetType().Name ?? "string",
                        DisplayValue = GetDisplayValue(kvp.Value)
                    });
                }

                StatusMessage = $"Загружено настроек: {Settings.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Получение отображаемого значения
        /// </summary>
        private string GetDisplayValue(object value)
        {
            if (value == null) return "null";
            if (value is string str) return str.Length > 50 ? str.Substring(0, 47) + "..." : str;
            if (value is bool b) return b ? "true" : "false";
            return value.ToString();
        }

        #endregion

        #region Команды

        /// <summary>
        /// Начало добавления настройки
        /// </summary>
        private void StartAddSetting()
        {
            NewSettingKey = string.Empty;
            NewSettingValue = string.Empty;
            IsEditing = true;
        }

        /// <summary>
        /// Отмена редактирования
        /// </summary>
        private void CancelEdit()
        {
            IsEditing = false;
            NewSettingKey = string.Empty;
            NewSettingValue = string.Empty;
        }

        /// <summary>
        /// Сохранение настройки
        /// </summary>
        private async Task SaveSettingAsync()
        {
            if (SelectedUser == null) return;
            if (string.IsNullOrWhiteSpace(NewSettingKey))
            {
                StatusMessage = "Ключ настройки не может быть пустым";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Сохранение...";

                // Пытаемся определить тип и сохранить
                object valueToSave = NewSettingValue;

                // Пробуем распарсить как число
                if (int.TryParse(NewSettingValue, out int intValue))
                    valueToSave = intValue;
                else if (double.TryParse(NewSettingValue, out double doubleValue))
                    valueToSave = doubleValue;
                else if (bool.TryParse(NewSettingValue, out bool boolValue))
                    valueToSave = boolValue;
                else if (NewSettingValue?.ToLower() == "null")
                    valueToSave = null;

                await _settingsService.SaveSettingAsync(SelectedUser.Id, NewSettingKey, valueToSave);

                await LoadSettingsAsync();
                IsEditing = false;
                StatusMessage = "Настройка сохранена";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Удаление настройки
        /// </summary>
        private async Task DeleteSettingAsync(SettingItem item)
        {
            if (SelectedUser == null || item == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Удаление настройки {item.Key}...";

                await _settingsService.DeleteSettingAsync(SelectedUser.Id, item.Key);

                Settings.Remove(item);
                StatusMessage = "Настройка удалена";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Очистка всех настроек
        /// </summary>
        private async Task ClearAllSettingsAsync()
        {
            if (SelectedUser == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Вы действительно хотите удалить ВСЕ настройки пользователя {SelectedUser.FullName}?",
                "Подтверждение",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Очистка настроек...";

                    await _settingsService.ClearAllSettingsAsync(SelectedUser.Id);

                    Settings.Clear();
                    StatusMessage = "Все настройки удалены";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Ошибка: {ex.Message}";
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        /// <summary>
        /// Экспорт настроек в JSON
        /// </summary>
        private void ExportSettings()
        {
            if (SelectedUser == null || !Settings.Any()) return;

            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    FileName = $"settings_{SelectedUser.WindowsLogin}_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                    DefaultExt = "json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var settingsDict = Settings.ToDictionary(s => s.Key, s => s.Value);
                    var json = JsonSerializer.Serialize(settingsDict, new JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(saveDialog.FileName, json);

                    StatusMessage = $"Настройки экспортированы в {saveDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
            }
        }

        /// <summary>
        /// Импорт настроек из JSON
        /// </summary>
        private async Task ImportSettingsAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var json = System.IO.File.ReadAllText(openDialog.FileName);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                    if (settings == null || !settings.Any())
                    {
                        StatusMessage = "Файл не содержит настроек";
                        return;
                    }

                    var result = System.Windows.MessageBox.Show(
                        $"Импортировать {settings.Count} настроек? Существующие настройки с такими же ключами будут перезаписаны.",
                        "Подтверждение импорта",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Question);

                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        IsLoading = true;
                        StatusMessage = "Импорт настроек...";

                        var dictToSave = new Dictionary<string, object>();
                        foreach (var kvp in settings)
                        {
                            switch (kvp.Value.ValueKind)
                            {
                                case JsonValueKind.String:
                                    dictToSave[kvp.Key] = kvp.Value.GetString();
                                    break;
                                case JsonValueKind.Number:
                                    if (kvp.Value.TryGetInt32(out int intVal))
                                        dictToSave[kvp.Key] = intVal;
                                    else if (kvp.Value.TryGetDouble(out double doubleVal))
                                        dictToSave[kvp.Key] = doubleVal;
                                    else
                                        dictToSave[kvp.Key] = kvp.Value.GetDecimal();
                                    break;
                                case JsonValueKind.True:
                                case JsonValueKind.False:
                                    dictToSave[kvp.Key] = kvp.Value.GetBoolean();
                                    break;
                                default:
                                    dictToSave[kvp.Key] = kvp.Value.ToString();
                                    break;
                            }
                        }

                        await _settingsService.SaveSettingsAsync(SelectedUser.Id, dictToSave);
                        await LoadSettingsAsync();

                        StatusMessage = $"Импортировано {settings.Count} настроек";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка импорта: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Вспомогательный класс

        /// <summary>
        /// Элемент настройки для отображения
        /// </summary>
        public class SettingItem : ViewModelBase
        {
            private string _key;
            private object _value;
            private string _valueType;
            private string _displayValue;

            public string Key
            {
                get => _key;
                set => SetProperty(ref _key, value);
            }

            public object Value
            {
                get => _value;
                set => SetProperty(ref _value, value);
            }

            public string ValueType
            {
                get => _valueType;
                set => SetProperty(ref _valueType, value);
            }

            public string DisplayValue
            {
                get => _displayValue;
                set => SetProperty(ref _displayValue, value);
            }
        }

        #endregion
    }
}