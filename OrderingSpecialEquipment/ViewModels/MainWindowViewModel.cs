using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;
using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// ViewModel главного окна приложения
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        #region Поля

        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDatabaseService _databaseService;
        private readonly IDbContextFactory _contextFactory;
        private readonly IShiftRequestService _shiftRequestService;
        private readonly IEquipmentService _equipmentService;
        private readonly IThemeService _themeService;
        private readonly IUserSettingsService _userSettingsService; // ДОБАВЛЕНО

        private bool _isLeftPanelVisible = false;
        private bool _isOnlyFavorites;
        private DateTime _selectedDate;
        private ObservableCollection<EquipmentItemViewModel> _equipmentItems;
        private ObservableCollection<ShiftRequestViewModel> _shiftRequests;
        private ShiftRequestViewModel _selectedRequest;
        private EquipmentItemViewModel _selectedEquipment;
        private bool _isEditMode;
        private ShiftRequestViewModel _editingRequest;
        private bool _isPopupOpen;
        private bool _isLoading;
        private readonly SemaphoreSlim _equipmentSemaphore = new SemaphoreSlim(1, 1);
        private string _statusMessage = "Готов";
        private Department _selectedDepartment;
        private List<Department> _accessibleDepartments = new List<Department>();
        private List<Warehouse> _accessibleWarehouses = new List<Warehouse>();
        private List<Equipment> _allEquipments = new List<Equipment>();
        private List<LicensePlate> _allLicensePlates = new List<LicensePlate>();
        private List<LessorOrganization> _allLessorOrganizations = new List<LessorOrganization>();
        private Dictionary<string, decimal> _monthlyHoursLeft = new Dictionary<string, decimal>();
        private TimeSpan? _startTime;
        private TimeSpan? _endTime;
        private bool _hasLunchBreak = true;
        private ICollectionView _groupedShiftRequests;
        private double _leftPanelWidth = 0;
        private bool _isDarkTheme;

        #endregion

        #region Свойства

        /// <summary>
        /// Темная тема активна
        /// </summary>
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set => SetProperty(ref _isDarkTheme, value);
        }

        /// <summary>
        /// Ширина левой панели
        /// </summary>
        public double LeftPanelWidth
        {
            get => _leftPanelWidth;
            set
            {
                if (SetProperty(ref _leftPanelWidth, value))
                {
                    OnPropertyChanged(nameof(LeftPanelButtonText));
                    OnPropertyChanged(nameof(LeftPanelButtonToolTip));
                }
            }
        }

        /// <summary>
        /// Видимость левой панели
        /// </summary>
        public bool IsLeftPanelVisible
        {
            get => _isLeftPanelVisible;
            set
            {
                if (SetProperty(ref _isLeftPanelVisible, value))
                {
                    _ = UpdateDisplayedShiftsAsync();
                }
            }
        }

        /// <summary>
        /// Текст кнопки левой панели
        /// </summary>
        public string LeftPanelButtonText => IsLeftPanelVisible ? "◀" : "▶";

        /// <summary>
        /// Подсказка кнопки левой панели
        /// </summary>
        public string LeftPanelButtonToolTip => IsLeftPanelVisible ? "Скрыть панель техники" : "Показать панель техники";

        /// <summary>
        /// Показывать только избранное
        /// </summary>
        public bool IsOnlyFavorites
        {
            get => _isOnlyFavorites;
            set
            {
                if (SetProperty(ref _isOnlyFavorites, value))
                {
                    _ = LoadEquipmentAsync();
                    _ = SaveUserPreferenceAsync("OnlyFavorites", value); // ИСПРАВЛЕНО
                }
            }
        }

        /// <summary>
        /// Выбранная дата
        /// </summary>
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value.ToUniversalTime().Date))
                {
                    OnPropertyChanged(nameof(DisplayDate));
                    OnPropertyChanged(nameof(DisplayDayOfWeek));
                    _groupedShiftRequests = null;
                    _ = UpdateDisplayedShiftsAsync();
                    _ = SaveUserPreferenceAsync("LastDate", value.ToString("yyyy-MM-dd")); // ДОБАВЛЕНО
                }
            }
        }

        /// <summary>
        /// Отображаемая дата
        /// </summary>
        public string DisplayDate => _selectedDate.ToString("dd.MM.yyyy");

        /// <summary>
        /// Отображаемый день недели
        /// </summary>
        public string DisplayDayOfWeek
        {
            get
            {
                return _selectedDate.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Понедельник",
                    DayOfWeek.Tuesday => "Вторник",
                    DayOfWeek.Wednesday => "Среда",
                    DayOfWeek.Thursday => "Четверг",
                    DayOfWeek.Friday => "Пятница",
                    DayOfWeek.Saturday => "Суббота",
                    DayOfWeek.Sunday => "Воскресенье",
                    _ => ""
                };
            }
        }

        /// <summary>
        /// Коллекция техники для отображения
        /// </summary>
        public ObservableCollection<EquipmentItemViewModel> EquipmentItems
        {
            get => _equipmentItems;
            set => SetProperty(ref _equipmentItems, value);
        }

        /// <summary>
        /// Коллекция заявок для отображения
        /// </summary>
        public ObservableCollection<ShiftRequestViewModel> ShiftRequests
        {
            get => _shiftRequests;
            set
            {
                if (SetProperty(ref _shiftRequests, value))
                {
                    _groupedShiftRequests = null;
                    OnPropertyChanged(nameof(GroupedShiftRequests));
                }
            }
        }

        /// <summary>
        /// Группированное представление заявок
        /// </summary>
        public ICollectionView GroupedShiftRequests
        {
            get
            {
                if (_shiftRequests == null) return null;
                if (_groupedShiftRequests != null) return _groupedShiftRequests;

                _groupedShiftRequests = CollectionViewSource.GetDefaultView(_shiftRequests);
                _groupedShiftRequests.GroupDescriptions.Clear();
                _groupedShiftRequests.GroupDescriptions.Add(new PropertyGroupDescription("GroupDisplayString"));
                _groupedShiftRequests.SortDescriptions.Clear();
                _groupedShiftRequests.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Ascending));
                _groupedShiftRequests.SortDescriptions.Add(new SortDescription("Shift", ListSortDirection.Ascending));

                return _groupedShiftRequests;
            }
        }

        /// <summary>
        /// Выбранная заявка
        /// </summary>
        public ShiftRequestViewModel SelectedRequest
        {
            get => _selectedRequest;
            set => SetProperty(ref _selectedRequest, value);
        }

        /// <summary>
        /// Выбранная техника
        /// </summary>
        public EquipmentItemViewModel SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                if (SetProperty(ref _selectedEquipment, value))
                {
                    // Автоматическое создание заявки убрано
                }
            }
        }

        /// <summary>
        /// Режим редактирования активен
        /// </summary>
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        /// <summary>
        /// Редактируемая заявка
        /// </summary>
        public ShiftRequestViewModel EditingRequest
        {
            get => _editingRequest;
            set
            {
                if (SetProperty(ref _editingRequest, value))
                {
                    if (value != null)
                    {
                        value.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(ShiftRequestViewModel.Shift))
                            {
                                UpdateTimeBasedOnShift();
                            }
                        };

                        if (value.WorkedHours.HasValue)
                        {
                            double hours = (double)value.WorkedHours.Value;
                            StartTime = TimeSpan.FromHours(8);
                            EndTime = StartTime.Value.Add(TimeSpan.FromHours(hours));
                            HasLunchBreak = hours > 5;
                        }
                        else
                        {
                            UpdateTimeBasedOnShift();
                        }
                    }
                    OnPropertyChanged(nameof(FilteredLicensePlates));
                }
            }
        }

        /// <summary>
        /// Открыто ли всплывающее окно редактирования
        /// </summary>
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                if (_isPopupOpen != value)
                {
                    _isPopupOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Идет загрузка
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
        /// Текущий пользователь
        /// </summary>
        public User CurrentUser => _authenticationService.CurrentUser;

        /// <summary>
        /// Подключена ли база данных
        /// </summary>
        public bool IsDatabaseConnected => _databaseService.IsConnected;

        /// <summary>
        /// Заголовок окна
        /// </summary>
        public string WindowTitle => $"Управление заявками на спецтехнику - {CurrentUser?.FullName ?? "Не авторизован"}";

        /// <summary>
        /// Выбранный отдел
        /// </summary>
        public Department SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                {
                    _groupedShiftRequests = null;
                    _ = LoadEquipmentAsync();
                    _ = LoadShiftRequestsAsync();
                    _ = LoadMonthlyHoursLeftAsync();
                    _ = SaveUserPreferenceAsync("LastDepartment", value?.Id); // ИСПРАВЛЕНО
                }
            }
        }

        /// <summary>
        /// Доступные отделы
        /// </summary>
        public List<Department> AccessibleDepartments
        {
            get => _accessibleDepartments;
            set => SetProperty(ref _accessibleDepartments, value);
        }

        /// <summary>
        /// Доступные склады
        /// </summary>
        public List<Warehouse> AccessibleWarehouses
        {
            get => _accessibleWarehouses;
            set => SetProperty(ref _accessibleWarehouses, value);
        }

        /// <summary>
        /// Вся техника
        /// </summary>
        public List<Equipment> AllEquipments
        {
            get => _allEquipments;
            set => SetProperty(ref _allEquipments, value);
        }

        /// <summary>
        /// Все госномера
        /// </summary>
        public List<LicensePlate> AllLicensePlates
        {
            get => _allLicensePlates;
            set => SetProperty(ref _allLicensePlates, value);
        }

        /// <summary>
        /// Отфильтрованные госномера для текущего арендодателя
        /// </summary>
        public List<LicensePlate> FilteredLicensePlates
        {
            get
            {
                if (EditingRequest == null || string.IsNullOrEmpty(EditingRequest.LessorOrganizationId))
                    return AllLicensePlates ?? new List<LicensePlate>();
                return AllLicensePlates?
                    .Where(lp => lp.LessorOrganizationId == EditingRequest.LessorOrganizationId)
                    .ToList() ?? new List<LicensePlate>();
            }
        }

        /// <summary>
        /// Все арендодатели
        /// </summary>
        public List<LessorOrganization> AllLessorOrganizations
        {
            get => _allLessorOrganizations;
            set => SetProperty(ref _allLessorOrganizations, value);
        }

        /// <summary>
        /// Доступная техника для выбранного отдела
        /// </summary>
        public List<Equipment> AvailableEquipmentsForDepartment
        {
            get
            {
                if (SelectedDepartment == null || AllEquipments == null)
                    return new List<Equipment>();

                try
                {
                    using var context = _databaseService.CreateDbContext();
                    var tpEquipmentIds = context.TransportProgram
                        .Where(tp => tp.DepartmentId == SelectedDepartment.Id && tp.Year == _selectedDate.Year)
                        .Select(tp => tp.EquipmentId)
                        .ToHashSet();

                    var available = AllEquipments
                        .Where(e => tpEquipmentIds.Contains(e.Id))
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"AvailableEquipmentsForDepartment: {available.Count} items for department {SelectedDepartment.Name}");

                    if (available.Count == 0 && AllEquipments != null)
                        return AllEquipments.ToList();

                    return available;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки доступной техники: {ex.Message}");
                    return new List<Equipment>();
                }
            }
        }

        /// <summary>
        /// Время начала работы
        /// </summary>
        public TimeSpan? StartTime
        {
            get => _startTime;
            set
            {
                if (SetProperty(ref _startTime, value))
                {
                    UpdateWorkedHours();
                }
            }
        }

        /// <summary>
        /// Время окончания работы
        /// </summary>
        public TimeSpan? EndTime
        {
            get => _endTime;
            set
            {
                if (SetProperty(ref _endTime, value))
                {
                    UpdateWorkedHours();
                }
            }
        }

        /// <summary>
        /// Вычитать обеденный перерыв
        /// </summary>
        public bool HasLunchBreak
        {
            get => _hasLunchBreak;
            set
            {
                if (SetProperty(ref _hasLunchBreak, value))
                {
                    UpdateWorkedHours();
                }
            }
        }

        #endregion

        #region Команды

        public ICommand ToggleThemeCommand { get; set; }
        public ICommand ToggleLeftPanelCommand { get; set; }
        public ICommand AddRequestCommand { get; set; }
        public ICommand EditRequestCommand { get; set; }
        public ICommand SaveRequestCommand { get; set; }
        public ICommand CancelEditCommand { get; set; }
        public ICommand DeleteRequestCommand { get; set; }
        public ICommand ExportToExcelCommand { get; set; }
        public ICommand ToggleFavoriteCommand { get; set; }
        public ICommand PreviousDayCommand { get; set; }
        public ICommand NextDayCommand { get; set; }
        public ICommand OpenConnectionSettingsCommand { get; set; }
        public ICommand OpenDepartmentsCommand { get; set; }
        public ICommand OpenEquipmentsCommand { get; set; }
        public ICommand OpenWarehousesCommand { get; set; }
        public ICommand OpenLessorsCommand { get; set; }
        public ICommand OpenTransportProgramCommand { get; set; }
        public ICommand OpenUsersCommand { get; set; }
        public ICommand OpenUserSettingsCommand { get; set; } // ДОБАВЛЕНО
        public ICommand OpenTransportReportCommand { get; set; }
        public ICommand OpenShiftReportCommand { get; set; }
        public ICommand RequestDoubleClickCommand { get; set; }
        public ICommand AddDayShiftCommand { get; set; }
        public ICommand AddNightShiftCommand { get; set; }
        public ICommand ClearLessorCommand { get; set; } // ДОБАВЛЕНО

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор MainWindowViewModel
        /// </summary>
        public MainWindowViewModel(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IDatabaseService databaseService,
            IShiftRequestService shiftRequestService,
            IEquipmentService equipmentService,
            IDbContextFactory contextFactory,
            IThemeService themeService,
            IUserSettingsService userSettingsService) // ДОБАВЛЕНО
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _shiftRequestService = shiftRequestService ?? throw new ArgumentNullException(nameof(shiftRequestService));
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService)); // ДОБАВЛЕНО

            InitializeCommands();
            SubscribeToEvents();

            _selectedDate = DateTime.UtcNow.Date;
            _leftPanelWidth = 0;
            _isLeftPanelVisible = false;

            _themeService = themeService;
            _isDarkTheme = _themeService.IsDarkTheme;

            _themeService.ThemeChanged += (s, isDark) =>
            {
                IsDarkTheme = isDark;
            };
        }

        #endregion

        #region Инициализация

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitializeCommands()
        {
            ToggleLeftPanelCommand = new RelayCommand(ToggleLeftPanel);
            AddRequestCommand = new RelayCommand(AddNewRequest, CanAddRequest);
            EditRequestCommand = new RelayCommand<ShiftRequestViewModel>(StartEditRequest, CanEditRequest);
            SaveRequestCommand = new RelayCommand(SaveRequest, CanSaveRequest);
            CancelEditCommand = new RelayCommand(CancelEdit);
            DeleteRequestCommand = new RelayCommand<ShiftRequestViewModel>(DeleteRequest, CanDeleteRequest);
            ExportToExcelCommand = new RelayCommand(ExportToExcel, CanExportToExcel);
            ToggleFavoriteCommand = new RelayCommand<EquipmentItemViewModel>(ToggleFavorite);
            PreviousDayCommand = new RelayCommand(PreviousDay);
            NextDayCommand = new RelayCommand(NextDay);
            RequestDoubleClickCommand = new RelayCommand<ShiftRequestViewModel>(StartEditRequest, CanEditRequest);
            OpenConnectionSettingsCommand = new RelayCommand(OpenConnectionSettings, CanOpenConnectionSettings);
            OpenDepartmentsCommand = new RelayCommand(() => OpenReference("Departments"), () => _authorizationService.CanReadTable("Departments"));
            OpenEquipmentsCommand = new RelayCommand(() => OpenReference("Equipments"), () => _authorizationService.CanReadTable("Equipments"));
            OpenWarehousesCommand = new RelayCommand(() => OpenReference("Warehouses"), () => _authorizationService.CanReadTable("Warehouses"));
            OpenLessorsCommand = new RelayCommand(() => OpenReference("LessorOrganizations"), () => _authorizationService.CanReadTable("LessorOrganizations"));
            OpenTransportProgramCommand = new RelayCommand(() => OpenReference("TransportProgram"), () => _authorizationService.CanReadTable("TransportProgram"));
            OpenUsersCommand = new RelayCommand(() => OpenReference("Users"), () => _authorizationService.HasSpecialPermission("ManageUsers") || _authorizationService.IsSystemAdmin);
            OpenUserSettingsCommand = new RelayCommand(() => OpenReference("UserSettings"), () => _authorizationService.HasSpecialPermission("ManageUsers") || _authorizationService.IsSystemAdmin); // ДОБАВЛЕНО
            OpenTransportReportCommand = new RelayCommand(() => OpenReport("Transport"), () => _authorizationService.HasSpecialPermission("ViewReports"));
            OpenShiftReportCommand = new RelayCommand(() => OpenReport("Shift"), () => _authorizationService.HasSpecialPermission("ViewReports"));
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            AddDayShiftCommand = new RelayCommand<EquipmentItemViewModel>(AddDayShiftRequest);
            AddNightShiftCommand = new RelayCommand<EquipmentItemViewModel>(AddNightShiftRequest);
            ClearLessorCommand = new RelayCommand(ClearLessor, (param) => EditingRequest != null);
        }

        /// <summary>
        /// Подписка на события
        /// </summary>
        private void SubscribeToEvents()
        {
            _databaseService.ConnectionStateChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsDatabaseConnected));
                if (e)
                {
                    _ = LoadDataAsync();
                }
            };

            _authenticationService.UserChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(WindowTitle));
                if (e != null)
                {
                    _ = LoadDataAsync();
                    _ = LoadUserPreferencesAsync(); // ДОБАВЛЕНО
                }
            };
        }

        #endregion

        #region Методы инициализации

        /// <summary>
        /// Асинхронная инициализация ViewModel
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_databaseService.IsConnected && _authenticationService.IsAuthenticated)
            {
                await LoadUserPreferencesAsync(); // ДОБАВЛЕНО
                await LoadAccessibleDepartmentsAsync();
                await LoadComboBoxDataAsync();
                await LoadDataAsync();
            }
        }

        /// <summary>
        /// Загрузка предпочтений пользователя из настроек
        /// </summary>
        private async Task LoadUserPreferencesAsync() // ДОБАВЛЕНО
        {
            try
            {
                var userId = _authenticationService.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId)) return;

                // Загружаем настройки пользователя
                IsOnlyFavorites = await _userSettingsService.GetSettingAsync(userId, "OnlyFavorites", false);

                // Загружаем последний выбранный отдел
                var lastDepartmentId = await _userSettingsService.GetSettingAsync<string>(userId, "LastDepartment", null);
                if (!string.IsNullOrEmpty(lastDepartmentId) && AccessibleDepartments != null)
                {
                    SelectedDepartment = AccessibleDepartments.FirstOrDefault(d => d.Id == lastDepartmentId);
                }

                // Загружаем последнюю выбранную дату
                var lastDateStr = await _userSettingsService.GetSettingAsync<string>(userId, "LastDate", null);
                if (!string.IsNullOrEmpty(lastDateStr) && DateTime.TryParse(lastDateStr, out DateTime lastDate))
                {
                    SelectedDate = lastDate;
                }

                // Загружаем тему
                var darkTheme = await _userSettingsService.GetSettingAsync(userId, "DarkTheme", _themeService.IsDarkTheme);
                if (darkTheme != _themeService.IsDarkTheme)
                {
                    _themeService.ApplyTheme(darkTheme);
                }

                System.Diagnostics.Debug.WriteLine("Предпочтения пользователя загружены");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки предпочтений: {ex.Message}");
            }
        }

        /// <summary>
        /// Сохранение предпочтения пользователя
        /// </summary>
        private async Task SaveUserPreferenceAsync(string key, object value) // ДОБАВЛЕНО
        {
            try
            {
                var userId = _authenticationService.CurrentUser?.Id;
                if (!string.IsNullOrEmpty(userId))
                {
                    await _userSettingsService.SaveSettingAsync(userId, key, value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения предпочтения {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка доступных отделов
        /// </summary>
        private async Task LoadAccessibleDepartmentsAsync()
        {
            try
            {
                var departments = await _authorizationService.GetAccessibleDepartmentsAsync();
                AccessibleDepartments = departments;
                if (departments.Any() && SelectedDepartment == null)
                {
                    SelectedDepartment = departments.First();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки отделов: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка данных
        /// </summary>
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                StatusMessage = "Загрузка данных...";

                await LoadMonthlyHoursLeftAsync();
                await LoadEquipmentAsync();
                await LoadShiftRequestsAsync();

                StatusMessage = "Готов";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки";
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Загрузка данных для выпадающих списков
        /// </summary>
        public async Task LoadComboBoxDataAsync()
        {
            try
            {
                if (!_databaseService.IsConnected) return;

                System.Diagnostics.Debug.WriteLine("Загрузка данных для выпадающих списков...");

                var departments = await _authorizationService.GetAccessibleDepartmentsAsync();
                AccessibleDepartments = departments;

                var warehouses = await _authorizationService.GetAccessibleWarehousesAsync();
                AccessibleWarehouses = warehouses;

                using (var context = _contextFactory.CreateDbContext())
                {
                    AllEquipments = await context.Equipments
                        .Where(e => e.IsActive)
                        .OrderBy(e => e.Name)
                        .ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"Загружено техники: {AllEquipments.Count}");
                    OnPropertyChanged(nameof(AvailableEquipmentsForDepartment));
                }

                using (var context = _contextFactory.CreateDbContext())
                {
                    AllLicensePlates = await context.LicensePlates
                        .Include(lp => lp.Equipment)
                        .Include(lp => lp.LessorOrganization)
                        .Where(lp => lp.IsActive)
                        .ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"Загружено госномеров: {AllLicensePlates.Count}");
                }

                using (var context = _contextFactory.CreateDbContext())
                {
                    AllLessorOrganizations = await context.LessorOrganizations
                        .Where(lo => lo.IsActive)
                        .OrderBy(lo => lo.Name)
                        .ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"Загружено арендодателей: {AllLessorOrganizations.Count}");
                }

                System.Diagnostics.Debug.WriteLine("Данные для выпадающих списков загружены");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных для combo: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка оставшихся часов по транспортной программе
        /// </summary>
        private async Task LoadMonthlyHoursLeftAsync()
        {
            try
            {
                if (SelectedDepartment == null || !_databaseService.IsConnected)
                {
                    _monthlyHoursLeft.Clear();
                    return;
                }

                using var context = _contextFactory.CreateDbContext();
                var currentMonth = _selectedDate.Month;
                var currentYear = _selectedDate.Year;

                var transportPrograms = await context.TransportProgram
                    .Where(tp => tp.DepartmentId == SelectedDepartment.Id && tp.Year == currentYear)
                    .ToListAsync();

                var workedHours = await context.ShiftRequests
                    .Where(sr => sr.DepartmentId == SelectedDepartment.Id &&
                                 sr.Date.Month == currentMonth &&
                                 sr.Date.Year == currentYear &&
                                 sr.IsWorked == true)
                    .GroupBy(sr => sr.EquipmentId)
                    .Select(g => new { EquipmentId = g.Key, TotalHours = g.Sum(sr => sr.WorkedHours ?? 0) })
                    .ToDictionaryAsync(x => x.EquipmentId, x => x.TotalHours);

                _monthlyHoursLeft.Clear();
                foreach (var tp in transportPrograms)
                {
                    decimal planHours = tp.GetHoursByMonth(currentMonth);
                    decimal worked = workedHours.GetValueOrDefault(tp.EquipmentId);
                    decimal left = planHours - worked;
                    _monthlyHoursLeft[tp.EquipmentId] = left;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки оставшихся часов: {ex.Message}");
                _monthlyHoursLeft.Clear();
            }
        }

        /// <summary>
        /// Загрузка техники
        /// </summary>
        private async Task LoadEquipmentAsync()
        {
            if (!await _equipmentSemaphore.WaitAsync(0))
            {
                System.Diagnostics.Debug.WriteLine("LoadEquipmentAsync: Уже выполняется, пропускаем");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("LoadEquipmentAsync: Начало загрузки");

                if (!_authenticationService.IsAuthenticated || !_databaseService.IsConnected)
                    return;

                var allEquipments = await _equipmentService.GetEquipmentsWithFavoritesAsync(
                    _authenticationService.CurrentUser.Id,
                    _isOnlyFavorites);

                List<Equipment> equipmentsToShow;
                if (SelectedDepartment != null)
                {
                    using var context = _contextFactory.CreateDbContext();
                    var tpEquipmentIdsList = await context.TransportProgram
                        .Where(tp => tp.DepartmentId == SelectedDepartment.Id && tp.Year == _selectedDate.Year)
                        .Select(tp => tp.EquipmentId)
                        .ToListAsync();
                    var tpEquipmentIds = new HashSet<string>(tpEquipmentIdsList);
                    equipmentsToShow = allEquipments.Where(e => tpEquipmentIds.Contains(e.Id)).ToList();
                }
                else
                {
                    equipmentsToShow = allEquipments;
                }

                System.Diagnostics.Debug.WriteLine($"LoadEquipmentAsync: Найдено {equipmentsToShow.Count} единиц техники");

                var (nightDate, dayDate) = GetDisplayDates();
                var nightRequests = await _shiftRequestService.GetByDateAndShiftAsync(nightDate, 1);
                var dayRequests = await _shiftRequestService.GetByDateAndShiftAsync(dayDate, 0);

                var userId = _authenticationService.CurrentUser.Id;
                var favoriteEquipmentIds = new HashSet<string>();
                using (var context = _contextFactory.CreateDbContext())
                {
                    var favorites = await context.UserFavorites
                        .Where(uf => uf.UserId == userId)
                        .Select(uf => uf.EquipmentId)
                        .ToListAsync();
                    favoriteEquipmentIds = new HashSet<string>(favorites);
                }

                var equipmentItems = new ObservableCollection<EquipmentItemViewModel>();
                foreach (var e in equipmentsToShow)
                {
                    bool isFavorite = favoriteEquipmentIds.Contains(e.Id);
                    decimal hoursLeft = _monthlyHoursLeft.GetValueOrDefault(e.Id);

                    equipmentItems.Add(new EquipmentItemViewModel
                    {
                        Equipment = e,
                        IsFavorite = isFavorite,
                        NightCount = nightRequests.Count(r => r.EquipmentId == e.Id &&
                            (SelectedDepartment == null || r.DepartmentId == SelectedDepartment.Id)),
                        DayCount = dayRequests.Count(r => r.EquipmentId == e.Id &&
                            (SelectedDepartment == null || r.DepartmentId == SelectedDepartment.Id)),
                        MonthlyHoursLeft = hoursLeft
                    });
                }

                System.Diagnostics.Debug.WriteLine("LoadEquipmentAsync: Присвоение EquipmentItems");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        EquipmentItems = equipmentItems;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке техники: {ex.Message}");
                    }
                });

                System.Diagnostics.Debug.WriteLine("LoadEquipmentAsync: Завершено");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке техники: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            finally
            {
                _equipmentSemaphore.Release();
            }
        }

        /// <summary>
        /// Обновление отображаемых смен
        /// </summary>
        public async Task UpdateDisplayedShiftsAsync()
        {
            await LoadShiftRequestsAsync();
            await LoadEquipmentAsync();
        }

        /// <summary>
        /// Получение дат для отображения в зависимости от видимости левой панели
        /// </summary>
        private (DateTime nightDate, DateTime dayDate) GetDisplayDates()
        {
            if (_isLeftPanelVisible)
            {
                return (_selectedDate, _selectedDate.AddDays(1));
            }
            else
            {
                return (_selectedDate.AddDays(-1), _selectedDate);
            }
        }

        /// <summary>
        /// Загрузка заявок
        /// </summary>
        private async Task LoadShiftRequestsAsync()
        {
            if (!_authenticationService.IsAuthenticated || !_databaseService.IsConnected)
                return;

            try
            {
                var (nightDate, dayDate) = GetDisplayDates();

                var nightTask = _shiftRequestService.GetByDateAndShiftAsync(nightDate, 1);
                var dayTask = _shiftRequestService.GetByDateAndShiftAsync(dayDate, 0);

                await Task.WhenAll(nightTask, dayTask);

                var allRequests = nightTask.Result
                    .Concat(dayTask.Result)
                    .Where(r => SelectedDepartment == null || r.DepartmentId == SelectedDepartment.Id)
                    .Select(r => new ShiftRequestViewModel(r, _authorizationService, _databaseService, _contextFactory, this))
                    .OrderBy(r => r.Date)
                    .ThenBy(r => r.Shift)
                    .ThenBy(r => r.DepartmentName)
                    .ThenBy(r => r.WarehouseName);

                ShiftRequests = new ObservableCollection<ShiftRequestViewModel>(allRequests);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке заявок: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновление времени в зависимости от выбранной смены
        /// </summary>
        private void UpdateTimeBasedOnShift()
        {
            if (EditingRequest == null) return;

            if (EditingRequest.WorkedHours.HasValue && EditingRequest.WorkedHours.Value > 0)
                return;

            if (EditingRequest.Shift == 0) // Дневная смена
            {
                StartTime = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30));
                EndTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(30));
            }
            else // Ночная смена
            {
                StartTime = TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30));
                EndTime = TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(30)).Add(TimeSpan.FromDays(1));
            }
            HasLunchBreak = true;
        }

        /// <summary>
        /// Обновление отработанных часов при изменении времени
        /// </summary>
        private void UpdateWorkedHours()
        {
            if (EditingRequest == null) return;

            if (StartTime.HasValue && EndTime.HasValue)
            {
                double hours;

                if (EndTime > StartTime)
                {
                    hours = (EndTime.Value - StartTime.Value).TotalHours;
                }
                else
                {
                    hours = (EndTime.Value.Add(TimeSpan.FromDays(1)) - StartTime.Value).TotalHours;
                }

                if (HasLunchBreak && hours > 5)
                {
                    hours -= 1;
                }
                EditingRequest.WorkedHours = (decimal)Math.Round(hours, 1);
            }
        }

        /// <summary>
        /// Уведомление об изменении свойства
        /// </summary>
        public void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        #endregion

        #region Команды левой панели

        /// <summary>
        /// Переключение видимости левой панели
        /// </summary>
        private void ToggleLeftPanel(object parameter)
        {
            IsLeftPanelVisible = !IsLeftPanelVisible;

            if (IsLeftPanelVisible)
            {
                LeftPanelWidth = 250;
            }
            else
            {
                LeftPanelWidth = 0;
            }

            _ = UpdateDisplayedShiftsAsync();
            _ = SaveUserPreferenceAsync("LeftPanelVisible", IsLeftPanelVisible); // ДОБАВЛЕНО
        }

        /// <summary>
        /// Переключение избранного
        /// </summary>
        private async void ToggleFavorite(EquipmentItemViewModel equipmentItem)
        {
            if (equipmentItem == null) return;

            try
            {
                if (equipmentItem.IsFavorite)
                {
                    await _equipmentService.RemoveFromFavoritesAsync(
                        _authenticationService.CurrentUser.Id,
                        equipmentItem.Equipment.Id);
                    equipmentItem.IsFavorite = false;
                }
                else
                {
                    await _equipmentService.AddToFavoritesAsync(
                        _authenticationService.CurrentUser.Id,
                        equipmentItem.Equipment.Id);
                    equipmentItem.IsFavorite = true;
                }

                if (_isOnlyFavorites)
                {
                    await LoadEquipmentAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении избранного: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавление заявки на дневную смену
        /// </summary>
        private void AddDayShiftRequest(EquipmentItemViewModel equipment)
        {
            if (equipment == null) return;

            var (nightDate, dayDate) = GetDisplayDates();

            var newRequest = new ShiftRequest
            {
                Date = _isLeftPanelVisible ? nightDate : dayDate,
                Shift = 0,
                EquipmentId = equipment.Equipment.Id,
                RequestedCount = 1,
                CreatedByUserId = _authenticationService.CurrentUser.Id,
                CreatedAt = DateTime.UtcNow,
                DepartmentId = SelectedDepartment?.Id
            };

            EditingRequest = new ShiftRequestViewModel(newRequest, _authorizationService, _databaseService, _contextFactory, this);
            EditingRequest.IsNew = true;

            StartTime = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30));
            EndTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(30));
            HasLunchBreak = true;

            SetDefaultWarehouseAndArea();

            OnPropertyChanged(nameof(AvailableEquipmentsForDepartment));

            IsEditMode = true;
            IsPopupOpen = true;
        }

        /// <summary>
        /// Добавление заявки на ночную смену
        /// </summary>
        private void AddNightShiftRequest(EquipmentItemViewModel equipment)
        {
            if (equipment == null) return;

            var (nightDate, dayDate) = GetDisplayDates();

            var newRequest = new ShiftRequest
            {
                Date = _isLeftPanelVisible ? nightDate : dayDate,
                Shift = 1,
                EquipmentId = equipment.Equipment.Id,
                RequestedCount = 1,
                CreatedByUserId = _authenticationService.CurrentUser.Id,
                CreatedAt = DateTime.UtcNow,
                DepartmentId = SelectedDepartment?.Id
            };

            EditingRequest = new ShiftRequestViewModel(newRequest, _authorizationService, _databaseService, _contextFactory, this);
            EditingRequest.IsNew = true;

            StartTime = TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30));
            EndTime = TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(30)).Add(TimeSpan.FromDays(1));
            HasLunchBreak = true;

            SetDefaultWarehouseAndArea();

            IsEditMode = true;
            IsPopupOpen = true;

            OnPropertyChanged(nameof(AvailableEquipmentsForDepartment));
        }

        /// <summary>
        /// Установка склада и территории по умолчанию
        /// </summary>
        private void SetDefaultWarehouseAndArea()
        {
            if (EditingRequest == null) return;

            if (EditingRequest.AvailableWarehouses != null && EditingRequest.AvailableWarehouses.Any())
            {
                EditingRequest.Warehouse = EditingRequest.AvailableWarehouses.First();

                if (EditingRequest.AvailableAreas != null && EditingRequest.AvailableAreas.Any())
                {
                    EditingRequest.Area = EditingRequest.AvailableAreas.First();
                }
            }
        }

        #endregion

        #region Команды заявок

        /// <summary>
        /// Проверка возможности добавления заявки
        /// </summary>
        private bool CanAddRequest(object parameter)
        {
            return _authorizationService.CanWriteTable("ShiftRequests") &&
                   _databaseService.IsConnected &&
                   SelectedDepartment != null;
        }

        /// <summary>
        /// Добавление новой заявки
        /// </summary>
        private void AddNewRequest(object parameter)
        {
            var (nightDate, dayDate) = GetDisplayDates();
            var newRequest = new ShiftRequest
            {
                Date = _isLeftPanelVisible ? nightDate : dayDate,
                Shift = _isLeftPanelVisible ? 1 : 0,
                RequestedCount = 1,
                CreatedByUserId = _authenticationService.CurrentUser.Id,
                CreatedAt = DateTime.UtcNow,
                DepartmentId = SelectedDepartment?.Id
            };

            EditingRequest = new ShiftRequestViewModel(newRequest, _authorizationService, _databaseService, _contextFactory, this);
            EditingRequest.IsNew = true;

            UpdateTimeBasedOnShift();

            SetDefaultWarehouseAndArea();

            OnPropertyChanged(nameof(AvailableEquipmentsForDepartment));

            IsEditMode = true;
            IsPopupOpen = true;
        }

        /// <summary>
        /// Проверка возможности редактирования заявки
        /// </summary>
        private bool CanEditRequest(ShiftRequestViewModel request)
        {
            if (request == null || !_authorizationService.CanWriteTable("ShiftRequests"))
                return false;
            if (request.IsBlocked && request.LockedByUserId != _authenticationService.CurrentUser.Id)
                return false;
            return true;
        }

        /// <summary>
        /// Начало редактирования заявки
        /// </summary>
        private async void StartEditRequest(ShiftRequestViewModel request)
        {
            if (request == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"StartEditRequest: Редактирование заявки {request.Key}");

                await _shiftRequestService.LockRequestAsync(request.Key, _authenticationService.CurrentUser.Id);

                using var context = _contextFactory.CreateDbContext();
                var fullRequest = await context.ShiftRequests
                    .Include(sr => sr.Equipment)
                    .Include(sr => sr.Warehouse)
                    .Include(sr => sr.Area)
                    .Include(sr => sr.LicensePlate)
                    .Include(sr => sr.LessorOrganization)
                    .Include(sr => sr.Department)
                    .Include(sr => sr.CreatedByUser)
                    .FirstOrDefaultAsync(sr => sr.Key == request.Key);

                if (fullRequest == null)
                {
                    System.Diagnostics.Debug.WriteLine("StartEditRequest: Заявка не найдена");
                    return;
                }

                EditingRequest = new ShiftRequestViewModel(fullRequest, _authorizationService, _databaseService, _contextFactory, this);
                EditingRequest.IsEdit = true;

                EditingRequest.Department = fullRequest.Department;
                EditingRequest.Warehouse = fullRequest.Warehouse;
                EditingRequest.Area = fullRequest.Area;
                EditingRequest.Equipment = fullRequest.Equipment;
                EditingRequest.LessorOrganization = fullRequest.LessorOrganization;
                EditingRequest.LicensePlate = fullRequest.LicensePlate;

                if (fullRequest.Department != null)
                {
                    SelectedDepartment = fullRequest.Department;
                }

                if (fullRequest.WorkedHours.HasValue && fullRequest.WorkedHours.Value > 0)
                {
                    double hours = (double)fullRequest.WorkedHours.Value;

                    if (fullRequest.Shift == 0)
                    {
                        StartTime = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30));
                        EndTime = StartTime.Value.Add(TimeSpan.FromHours(hours));

                        if (EndTime > TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(30)))
                        {
                            EndTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(30));
                        }
                    }
                    else
                    {
                        StartTime = TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30));
                        EndTime = StartTime.Value.Add(TimeSpan.FromHours(hours));

                        if (EndTime > TimeSpan.FromHours(30).Add(TimeSpan.FromMinutes(30)))
                        {
                            EndTime = TimeSpan.FromHours(30).Add(TimeSpan.FromMinutes(30));
                        }
                    }

                    HasLunchBreak = hours > 5;
                }
                else
                {
                    if (fullRequest.Shift == 0)
                    {
                        StartTime = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30));
                        EndTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(30));
                    }
                    else
                    {
                        StartTime = TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30));
                        EndTime = TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(30)).Add(TimeSpan.FromDays(1));
                    }
                    HasLunchBreak = true;
                }

                OnPropertyChanged(nameof(FilteredLicensePlates));

                IsEditMode = true;
                IsPopupOpen = true;

                System.Diagnostics.Debug.WriteLine("StartEditRequest: Завершено");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при открытии заявки: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверка возможности сохранения заявки
        /// </summary>
        private bool CanSaveRequest(object parameter)
        {
            return EditingRequest != null &&
                   EditingRequest.IsValid &&
                   _authorizationService.CanWriteTable("ShiftRequests") &&
                   _databaseService.IsConnected;
        }

        /// <summary>
        /// Сохранение заявки
        /// </summary>
        private async void SaveRequest(object parameter)
        {
            try
            {
                if (EditingRequest.IsNew && !string.IsNullOrEmpty(EditingRequest.EquipmentId))
                {
                    await AddDependentEquipmentAsync(EditingRequest);
                }

                var request = EditingRequest.OriginalRequest;

                if (EditingRequest.IsNew)
                {
                    await _shiftRequestService.AddAsync(request);
                }
                else
                {
                    await _shiftRequestService.UpdateAsync(request);
                    await _shiftRequestService.UnlockRequestAsync(request.Key);
                }

                CancelEdit(null);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заявки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавление зависимой техники
        /// </summary>
        private async Task AddDependentEquipmentAsync(ShiftRequestViewModel mainRequest)
        {
            try
            {
                using var context = _databaseService.CreateDbContext();
                var dependencies = await context.EquipmentDependencies
                    .Include(ed => ed.DependentEquipment)
                    .Where(ed => ed.MainEquipmentId == mainRequest.EquipmentId)
                    .ToListAsync();

                foreach (var dep in dependencies)
                {
                    var dependentRequest = new ShiftRequest
                    {
                        Date = mainRequest.Date,
                        Shift = mainRequest.Shift,
                        EquipmentId = dep.DependentEquipmentId,
                        WarehouseId = mainRequest.WarehouseId,
                        AreaId = mainRequest.AreaId,
                        LessorOrganizationId = mainRequest.LessorOrganizationId,
                        RequestedCount = dep.RequiredCount,
                        CreatedByUserId = _authenticationService.CurrentUser.Id,
                        CreatedAt = DateTime.UtcNow,
                        DepartmentId = mainRequest.DepartmentId,
                        Comment = $"Зависимость от {mainRequest.EquipmentName}. Требуется: {dep.RequiredCount} ед."
                    };

                    await _shiftRequestService.AddAsync(dependentRequest);

                    System.Diagnostics.Debug.WriteLine($"Добавлена зависимость: {dep.DependentEquipment?.Name} x{dep.RequiredCount}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении зависимой техники: {ex.Message}");
            }
        }

        /// <summary>
        /// Отмена редактирования
        /// </summary>
        private async void CancelEdit(object parameter)
        {
            if (EditingRequest != null && !EditingRequest.IsNew)
            {
                await _shiftRequestService.UnlockRequestAsync(EditingRequest.Key);
                EditingRequest.Cleanup();
                EditingRequest.Dispose();
            }

            EditingRequest = null;
            IsEditMode = false;
            IsPopupOpen = false;
        }

        /// <summary>
        /// Очистка арендодателя
        /// </summary>
        private void ClearLessor(object parameter) // ДОБАВЛЕНО
        {
            if (EditingRequest != null)
            {
                EditingRequest.LessorOrganization = null;
                EditingRequest.LicensePlate = null;
                OnPropertyChanged(nameof(FilteredLicensePlates));
            }
        }

        /// <summary>
        /// Проверка возможности удаления заявки
        /// </summary>
        private bool CanDeleteRequest(ShiftRequestViewModel request)
        {
            return request != null &&
                   _authorizationService.CanWriteTable("ShiftRequests") &&
                   _databaseService.IsConnected;
        }

        /// <summary>
        /// Удаление заявки
        /// </summary>
        private async void DeleteRequest(ShiftRequestViewModel request)
        {
            if (request == null) return;

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить заявку от {request.Date:dd.MM.yyyy}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _shiftRequestService.DeleteAsync(request.Key);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении заявки: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Создание новой заявки из техники
        /// </summary>
        private void CreateNewRequestFromEquipment(EquipmentItemViewModel equipment)
        {
            var (nightDate, dayDate) = GetDisplayDates();
            var newRequest = new ShiftRequest
            {
                Date = _isLeftPanelVisible ? nightDate : dayDate,
                Shift = _isLeftPanelVisible ? 1 : 0,
                EquipmentId = equipment.Equipment.Id,
                RequestedCount = 1,
                CreatedByUserId = _authenticationService.CurrentUser.Id,
                CreatedAt = DateTime.UtcNow,
                DepartmentId = SelectedDepartment?.Id
            };

            EditingRequest = new ShiftRequestViewModel(newRequest, _authorizationService, _databaseService, _contextFactory, this);
            EditingRequest.IsNew = true;

            if (_isLeftPanelVisible)
            {
                StartTime = TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30));
                EndTime = TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(30)).Add(TimeSpan.FromDays(1));
            }
            else
            {
                StartTime = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30));
                EndTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(30));
            }
            HasLunchBreak = true;

            SetDefaultWarehouseAndArea();

            IsEditMode = true;
            IsPopupOpen = true;
        }

        /// <summary>
        /// Проверка возможности экспорта в Excel
        /// </summary>
        private bool CanExportToExcel(object parameter)
        {
            return _authorizationService.HasSpecialPermission("ExportData") &&
                   ShiftRequests != null &&
                   ShiftRequests.Any() &&
                   _databaseService.IsConnected;
        }

        /// <summary>
        /// Экспорт в Excel
        /// </summary>
        private void ExportToExcel(object parameter)
        {
            var requests = ShiftRequests.Select(vm => vm.OriginalRequest).ToList();
            ExcelExporter.ExportShiftRequests(requests);
        }

        #endregion

        #region Навигация по датам

        /// <summary>
        /// Предыдущий день
        /// </summary>
        private void PreviousDay(object parameter)
        {
            SelectedDate = SelectedDate.AddDays(-1);
        }

        /// <summary>
        /// Следующий день
        /// </summary>
        private void NextDay(object parameter)
        {
            SelectedDate = SelectedDate.AddDays(1);
        }

        #endregion

        #region Настройки подключения

        /// <summary>
        /// Проверка возможности открытия настроек подключения
        /// </summary>
        private bool CanOpenConnectionSettings(object parameter)
        {
            return _authorizationService.HasSpecialPermission("ConfigureConnection") ||
                   _authorizationService.IsSystemAdmin;
        }

        /// <summary>
        /// Открытие настроек подключения
        /// </summary>
        private void OpenConnectionSettings(object parameter)
        {
            var settingsWindow = new Views.ConnectionSettingsWindow();
            settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();
        }

        #endregion

        #region Открытие справочников и отчетов

        /// <summary>
        /// Открытие справочника
        /// </summary>
        private void OpenReference(string referenceName)
        {
            Window window = null;
            switch (referenceName)
            {
                case "Departments":
                    window = new Views.DepartmentsView();
                    break;
                case "Equipments":
                    window = new Views.EquipmentsView();
                    break;
                case "Warehouses":
                    window = new Views.WarehousesAndAreasView();
                    break;
                case "LessorOrganizations":
                    window = new Views.LessorsAndPlatesView();
                    break;
                case "TransportProgram":
                    window = new Views.TransportProgramView();
                    break;
                case "Users":
                    window = new Views.UsersAndRolesView();
                    break;
                case "UserSettings": // ДОБАВЛЕНО
                    window = new Views.UserSettingsView();
                    break;
            }

            if (window != null)
            {
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
                _ = LoadDataAsync();
            }
        }

        /// <summary>
        /// Открытие отчета
        /// </summary>
        private void OpenReport(string reportName)
        {
            Window window = null;
            switch (reportName)
            {
                case "Transport":
                    window = new Views.TransportProgramReportView();
                    break;
                case "Shift":
                    window = new Views.ShiftRequestsReportView();
                    break;
            }

            if (window != null)
            {
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        /// <summary>
        /// Переключение темы
        /// </summary>
        private void ToggleTheme(object parameter)
        {
            _themeService.ToggleTheme();
            IsDarkTheme = _themeService.IsDarkTheme;
            _ = SaveUserPreferenceAsync("DarkTheme", IsDarkTheme); // ДОБАВЛЕНО
        }

        #endregion
    }
}