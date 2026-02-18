using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Threading;

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
        private bool _isLoadingEquipment = false;
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
        #endregion

        #region Свойства

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

        public bool IsLeftPanelVisible
        {
            get => _isLeftPanelVisible;
            set
            {
                if (SetProperty(ref _isLeftPanelVisible, value))
                {
                    // При изменении видимости панели обновляем отображаемые смены
                    _ = UpdateDisplayedShiftsAsync();
                }
            }
        }

        public string LeftPanelButtonText => IsLeftPanelVisible ? "◀" : "▶";
        public string LeftPanelButtonToolTip => IsLeftPanelVisible ? "Скрыть панель техники" : "Показать панель техники";

        public bool IsOnlyFavorites
        {
            get => _isOnlyFavorites;
            set
            {
                if (SetProperty(ref _isOnlyFavorites, value))
                {
                    _ = LoadEquipmentAsync();
                }
            }
        }

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
                }
            }
        }

        public string DisplayDate => _selectedDate.ToString("dd.MM.yyyy");

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

        public ObservableCollection<EquipmentItemViewModel> EquipmentItems
        {
            get => _equipmentItems;
            set => SetProperty(ref _equipmentItems, value);
        }

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

        public ShiftRequestViewModel SelectedRequest
        {
            get => _selectedRequest;
            set => SetProperty(ref _selectedRequest, value);
        }

        public EquipmentItemViewModel SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                if (SetProperty(ref _selectedEquipment, value) && value != null)
                {
                    CreateNewRequestFromEquipment(value);
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public ShiftRequestViewModel EditingRequest
        {
            get => _editingRequest;
            set
            {
                if (SetProperty(ref _editingRequest, value))
                {
                    if (value != null)
                    {
                        if (value.WorkedHours.HasValue)
                        {
                            double hours = (double)value.WorkedHours.Value;
                            StartTime = TimeSpan.FromHours(8);
                            EndTime = StartTime.Value.Add(TimeSpan.FromHours(hours));
                            HasLunchBreak = hours > 5;
                        }
                        else
                        {
                            StartTime = TimeSpan.FromHours(8);
                            EndTime = TimeSpan.FromHours(17);
                            HasLunchBreak = true;
                        }
                    }
                    OnPropertyChanged(nameof(FilteredLicensePlates));
                }
            }
        }

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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public User CurrentUser => _authenticationService.CurrentUser;
        public bool IsDatabaseConnected => _databaseService.IsConnected;
        public string WindowTitle => $"Управление заявками на спецтехнику - {CurrentUser?.FullName ?? "Не авторизован"}";

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
                }
            }
        }

        public List<Department> AccessibleDepartments
        {
            get => _accessibleDepartments;
            set => SetProperty(ref _accessibleDepartments, value);
        }

        public List<Warehouse> AccessibleWarehouses
        {
            get => _accessibleWarehouses;
            set => SetProperty(ref _accessibleWarehouses, value);
        }

        public List<Equipment> AllEquipments
        {
            get => _allEquipments;
            set => SetProperty(ref _allEquipments, value);
        }

        public List<LicensePlate> AllLicensePlates
        {
            get => _allLicensePlates;
            set => SetProperty(ref _allLicensePlates, value);
        }

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

        public List<LessorOrganization> AllLessorOrganizations
        {
            get => _allLessorOrganizations;
            set => SetProperty(ref _allLessorOrganizations, value);
        }

        public List<Equipment> AvailableEquipmentsForDepartment
        {
            get
            {
                if (SelectedDepartment == null || AllEquipments == null)
                    return AllEquipments ?? new List<Equipment>();

                try
                {
                    using var context = _databaseService.CreateDbContext();
                    var tpEquipmentIds = context.TransportProgram
                        .Where(tp => tp.DepartmentId == SelectedDepartment.Id && tp.Year == _selectedDate.Year)
                        .Select(tp => tp.EquipmentId)
                        .ToHashSet();

                    return AllEquipments
                        .Where(e => tpEquipmentIds.Contains(e.Id))
                        .ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки доступной техники: {ex.Message}");
                    return AllEquipments ?? new List<Equipment>();
                }
            }
        }

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
        public ICommand OpenTransportReportCommand { get; set; }
        public ICommand OpenShiftReportCommand { get; set; }
        public ICommand RequestDoubleClickCommand { get; set; }
        #endregion

        #region Конструктор
        public MainWindowViewModel(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IDatabaseService databaseService,
            IShiftRequestService shiftRequestService,
            IEquipmentService equipmentService,
            IDbContextFactory contextFactory)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _shiftRequestService = shiftRequestService ?? throw new ArgumentNullException(nameof(shiftRequestService));
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            InitializeCommands();
            SubscribeToEvents();

            _selectedDate = DateTime.UtcNow.Date;
            _leftPanelWidth = 0; // Начальная ширина
            _isLeftPanelVisible = false; // Панель не видима
        }
        #endregion

        #region Инициализация
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
            OpenTransportReportCommand = new RelayCommand(() => OpenReport("Transport"), () => _authorizationService.HasSpecialPermission("ViewReports"));
            OpenShiftReportCommand = new RelayCommand(() => OpenReport("Shift"), () => _authorizationService.HasSpecialPermission("ViewReports"));
        }

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
                }
            };
        }
        #endregion

        #region Методы инициализации
        public async Task InitializeAsync()
        {
            if (_databaseService.IsConnected && _authenticationService.IsAuthenticated)
            {
                await LoadAccessibleDepartmentsAsync();
                await LoadComboBoxDataAsync();
                await LoadDataAsync();
            }
        }

        private async Task LoadAccessibleDepartmentsAsync()
        {
            try
            {
                var departments = await _authorizationService.GetAccessibleDepartmentsAsync();
                AccessibleDepartments = departments;
                if (departments.Any())
                {
                    SelectedDepartment = departments.First();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки отделов: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                StatusMessage = "Загрузка данных...";

                // Загружаем последовательно для избежания конфликтов
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

                // Загружаем отделы
                var departments = await _authorizationService.GetAccessibleDepartmentsAsync();
                AccessibleDepartments = departments;

                // Загружаем склады
                var warehouses = await _authorizationService.GetAccessibleWarehousesAsync();
                AccessibleWarehouses = warehouses;

                // Загружаем технику
                using (var context = _contextFactory.CreateDbContext())
                {
                    AllEquipments = await context.Equipments
                        .Where(e => e.IsActive)
                        .OrderBy(e => e.Name)
                        .ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"Загружено техники: {AllEquipments.Count}");
                }

                // Загружаем госномера
                using (var context = _contextFactory.CreateDbContext())
                {
                    AllLicensePlates = await context.LicensePlates
                        .Include(lp => lp.Equipment)
                        .Include(lp => lp.LessorOrganization)
                        .Where(lp => lp.IsActive)
                        .ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"Загружено госномеров: {AllLicensePlates.Count}");
                }

                // Загружаем арендодателей
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

        public async Task UpdateDisplayedShiftsAsync()
        {
            await LoadShiftRequestsAsync();
            await LoadEquipmentAsync();
        }

        private (DateTime nightDate, DateTime dayDate) GetDisplayDates()
        {
            if (_isLeftPanelVisible)
            {
                // Панель ОТКРЫТА: ночь ТЕКУЩЕГО дня, день СЛЕДУЮЩЕГО дня
                return (_selectedDate, _selectedDate.AddDays(1));
            }
            else
            {
                // Панель СКРЫТА: ночь ПРЕДЫДУЩЕГО дня, день ТЕКУЩЕГО дня
                return (_selectedDate.AddDays(-1), _selectedDate);
            }
        }

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

        private void UpdateWorkedHours()
        {
            if (EditingRequest == null) return;

            if (StartTime.HasValue && EndTime.HasValue && EndTime > StartTime)
            {
                double hours = (EndTime.Value - StartTime.Value).TotalHours;
                if (HasLunchBreak && hours > 5)
                {
                    hours -= 1;
                }
                EditingRequest.WorkedHours = (decimal)Math.Round(hours, 1);
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
        #endregion

        #region Команды левой панели
        private void ToggleLeftPanel(object parameter)
        {
            IsLeftPanelVisible = !IsLeftPanelVisible;

            // Анимируем изменение ширины
            if (IsLeftPanelVisible)
            {
                LeftPanelWidth = 250;
            }
            else
            {
                LeftPanelWidth = 0;
            }

            // Обновляем отображение смен
            _ = UpdateDisplayedShiftsAsync();
        }

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
        #endregion

        #region Команды заявок
        private bool CanAddRequest(object parameter)
        {
            return _authorizationService.CanWriteTable("ShiftRequests") &&
                   _databaseService.IsConnected &&
                   SelectedDepartment != null;
        }

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

            StartTime = TimeSpan.FromHours(8);
            EndTime = TimeSpan.FromHours(17);
            HasLunchBreak = true;

            IsEditMode = true;
            IsPopupOpen = true;
        }

        private bool CanEditRequest(ShiftRequestViewModel request)
        {
            if (request == null || !_authorizationService.CanWriteTable("ShiftRequests"))
                return false;
            if (request.IsBlocked && request.LockedByUserId != _authenticationService.CurrentUser.Id)
                return false;
            return true;
        }

        private async void StartEditRequest(ShiftRequestViewModel request)
        {
            if (request == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"StartEditRequest: Редактирование заявки {request.Key}");

                await _shiftRequestService.LockRequestAsync(request.Key, _authenticationService.CurrentUser.Id);

                // Создаем копию для редактирования с полной загрузкой связанных данных
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

                // Явно устанавливаем навигационные свойства
                EditingRequest.Department = fullRequest.Department;
                EditingRequest.Warehouse = fullRequest.Warehouse;
                EditingRequest.Area = fullRequest.Area;
                EditingRequest.Equipment = fullRequest.Equipment;
                EditingRequest.LessorOrganization = fullRequest.LessorOrganization;
                EditingRequest.LicensePlate = fullRequest.LicensePlate;

                // Устанавливаем выбранный отдел в родительской ViewModel для фильтрации
                if (fullRequest.Department != null)
                {
                    SelectedDepartment = fullRequest.Department;
                }

                // Инициализация времени
                if (fullRequest.WorkedHours.HasValue)
                {
                    double hours = (double)fullRequest.WorkedHours.Value;
                    StartTime = TimeSpan.FromHours(8);
                    EndTime = StartTime.Value.Add(TimeSpan.FromHours(hours));
                    HasLunchBreak = hours > 5;
                }
                else
                {
                    StartTime = TimeSpan.FromHours(8);
                    EndTime = TimeSpan.FromHours(17);
                    HasLunchBreak = true;
                }

                // Обновляем фильтрацию госномеров
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

        private bool CanSaveRequest(object parameter)
        {
            return EditingRequest != null &&
                   EditingRequest.IsValid &&
                   _authorizationService.CanWriteTable("ShiftRequests") &&
                   _databaseService.IsConnected;
        }

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
                    for (int i = 0; i < dep.RequiredCount; i++)
                    {
                        var dependentRequest = new ShiftRequest
                        {
                            Date = mainRequest.Date,
                            Shift = mainRequest.Shift,
                            EquipmentId = dep.DependentEquipmentId,
                            WarehouseId = mainRequest.WarehouseId,
                            AreaId = mainRequest.AreaId,
                            LessorOrganizationId = mainRequest.LessorOrganizationId,
                            RequestedCount = 1,
                            CreatedByUserId = _authenticationService.CurrentUser.Id,
                            CreatedAt = DateTime.UtcNow,
                            DepartmentId = mainRequest.DepartmentId,
                            Comment = $"Зависимость от {mainRequest.EquipmentName}"
                        };
                        await _shiftRequestService.AddAsync(dependentRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении зависимой техники: {ex.Message}");
            }
        }

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

        private bool CanDeleteRequest(ShiftRequestViewModel request)
        {
            return request != null &&
                   _authorizationService.CanWriteTable("ShiftRequests") &&
                   _databaseService.IsConnected;
        }

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

            StartTime = TimeSpan.FromHours(8);
            EndTime = TimeSpan.FromHours(17);
            HasLunchBreak = true;

            IsEditMode = true;
            IsPopupOpen = true;
        }

        private bool CanExportToExcel(object parameter)
        {
            return _authorizationService.HasSpecialPermission("ExportData") &&
                   ShiftRequests != null &&
                   ShiftRequests.Any() &&
                   _databaseService.IsConnected;
        }

        private void ExportToExcel(object parameter)
        {
            var requests = ShiftRequests.Select(vm => vm.OriginalRequest).ToList();
            ExcelExporter.ExportShiftRequests(requests);
        }
        #endregion

        #region Навигация по датам
        private void PreviousDay(object parameter)
        {
            SelectedDate = SelectedDate.AddDays(-1);
        }

        private void NextDay(object parameter)
        {
            SelectedDate = SelectedDate.AddDays(1);
        }
        #endregion

        #region Настройки подключения
        private bool CanOpenConnectionSettings(object parameter)
        {
            return _authorizationService.HasSpecialPermission("ConfigureConnection") ||
                   _authorizationService.IsSystemAdmin;
        }

        private void OpenConnectionSettings(object parameter)
        {
            var settingsWindow = new Views.ConnectionSettingsWindow();
            settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();
        }
        #endregion

        #region Открытие справочников и отчетов
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
            }

            if (window != null)
            {
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
                _ = LoadDataAsync();
            }
        }

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
        #endregion
    }
}