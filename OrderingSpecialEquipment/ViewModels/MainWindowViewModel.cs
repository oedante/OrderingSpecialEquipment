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
        #endregion

        #region Свойства
        public bool IsLeftPanelVisible
        {
            get => _isLeftPanelVisible;
            set
            {
                if (SetProperty(ref _isLeftPanelVisible, value))
                {
                    OnPropertyChanged(nameof(LeftPanelButtonText));
                    OnPropertyChanged(nameof(LeftPanelButtonToolTip));
                    _ = UpdateDisplayedShiftsAsync();
                }
            }
        }

        public string LeftPanelButtonText => _isLeftPanelVisible ? "◀" : "▶";
        public string LeftPanelButtonToolTip => _isLeftPanelVisible ? "Скрыть панель техники" : "Показать панель техники";

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
                using var context = _databaseService.CreateDbContext();
                var tpEquipmentIds = context.TransportProgram
                    .Where(tp => tp.DepartmentId == SelectedDepartment.Id && tp.Year == _selectedDate.Year)
                    .Select(tp => tp.EquipmentId)
                    .ToHashSet();
                return AllEquipments
                    .Where(e => tpEquipmentIds.Contains(e.Id))
                    .ToList();
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
        public ICommand ToggleLeftPanelCommand { get; }
        public ICommand AddRequestCommand { get; }
        public ICommand EditRequestCommand { get; }
        public ICommand SaveRequestCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteRequestCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand PreviousDayCommand { get; }
        public ICommand NextDayCommand { get; }
        public ICommand OpenConnectionSettingsCommand { get; }
        public ICommand OpenDepartmentsCommand { get; }
        public ICommand OpenEquipmentsCommand { get; }
        public ICommand OpenWarehousesCommand { get; }
        public ICommand OpenLessorsCommand { get; }
        public ICommand OpenTransportProgramCommand { get; }
        public ICommand OpenUsersCommand { get; }
        public ICommand OpenTransportReportCommand { get; }
        public ICommand OpenShiftReportCommand { get; }
        public ICommand RequestDoubleClickCommand { get; }
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

            _selectedDate = DateTime.UtcNow.Date;
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
                var equipmentTask = LoadEquipmentAsync();
                var shiftRequestsTask = LoadShiftRequestsAsync();
                var monthlyHoursTask = LoadMonthlyHoursLeftAsync();
                await Task.WhenAll(equipmentTask, shiftRequestsTask, monthlyHoursTask);
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
        private async Task LoadComboBoxDataAsync()
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
                    var item = new EquipmentItemViewModel
                    {
                        Equipment = e,
                        IsFavorite = isFavorite,
                        NightCount = nightRequests.Count(r => r.EquipmentId == e.Id &&
                            (SelectedDepartment == null || r.DepartmentId == SelectedDepartment.Id)),
                        DayCount = dayRequests.Count(r => r.EquipmentId == e.Id &&
                            (SelectedDepartment == null || r.DepartmentId == SelectedDepartment.Id)),
                        MonthlyHoursLeft = hoursLeft
                    };
                    equipmentItems.Add(item);
                }

                System.Diagnostics.Debug.WriteLine("LoadEquipmentAsync: Присвоение EquipmentItems");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try { EquipmentItems = equipmentItems; }
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

        private async Task UpdateDisplayedShiftsAsync()
        {
            await Task.WhenAll(LoadShiftRequestsAsync(), LoadEquipmentAsync());
        }

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
                _groupedShiftRequests = null;
                OnPropertyChanged(nameof(GroupedShiftRequests));
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

    /// <summary>
    /// ViewModel для элемента техники в левой панели
    /// </summary>
    public class EquipmentItemViewModel : ViewModelBase
    {
        private Equipment _equipment;
        private bool _isFavorite;
        private int _nightCount;
        private int _dayCount;
        private decimal _monthlyHoursLeft;

        public Equipment Equipment
        {
            get => _equipment;
            set => SetProperty(ref _equipment, value);
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set => SetProperty(ref _isFavorite, value);
        }

        public int NightCount
        {
            get => _nightCount;
            set => SetProperty(ref _nightCount, value);
        }

        public int DayCount
        {
            get => _dayCount;
            set => SetProperty(ref _dayCount, value);
        }

        public decimal MonthlyHoursLeft
        {
            get => _monthlyHoursLeft;
            set
            {
                if (SetProperty(ref _monthlyHoursLeft, value))
                {
                    OnPropertyChanged(nameof(HoursLeftDisplay));
                    OnPropertyChanged(nameof(IsHoursLeftCritical));
                    OnPropertyChanged(nameof(IsHoursLeftWarning));
                    OnPropertyChanged(nameof(HoursLeftColor));
                }
            }
        }

        public string DisplayName => Equipment?.Name ?? "";
        public string DisplayCounts => $"Н:{NightCount} Д:{DayCount}";
        public string HoursLeftDisplay => $"{MonthlyHoursLeft:F1} ч";
        public bool IsHoursLeftCritical => MonthlyHoursLeft <= 0;
        public bool IsHoursLeftWarning => MonthlyHoursLeft > 0 && MonthlyHoursLeft < 10;

        public string HoursLeftColor
        {
            get
            {
                if (MonthlyHoursLeft <= 0) return "#FFFFE0E0";
                if (MonthlyHoursLeft < 10) return "#FFFFF0E0";
                return "Transparent";
            }
        }
    }

    /// <summary>
    /// ViewModel для заявки
    /// </summary>
    public class ShiftRequestViewModel : ViewModelBase, IDisposable
    {
        private readonly ShiftRequest _request;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDatabaseService _databaseService;
        private readonly IDbContextFactory _contextFactory;
        private readonly MainWindowViewModel _parent;
        private bool _isNew;
        private bool _isEdit;
        private bool _isExpanded;
        private bool _isUpdatingRelatedProperties = false;
        private bool _disposed = false;

        public ShiftRequestViewModel(ShiftRequest request,
            IAuthorizationService authorizationService,
            IDatabaseService databaseService,
            IDbContextFactory contextFactory,
            MainWindowViewModel parent)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public List<LicensePlate> FilteredLicensePlates => _parent?.FilteredLicensePlates ?? new List<LicensePlate>();
        public ShiftRequest OriginalRequest => _request;
        public int Key => _request.Key;

        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        public bool IsEdit
        {
            get => _isEdit;
            set => SetProperty(ref _isEdit, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public DateTime Date
        {
            get => _request.Date;
            set
            {
                _request.Date = value.ToUniversalTime();
                OnPropertyChanged();
                OnPropertyChanged(nameof(GroupDisplayString));
            }
        }

        public int Shift
        {
            get => _request.Shift;
            set
            {
                _request.Shift = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShiftName));
                OnPropertyChanged(nameof(GroupDisplayString));
            }
        }

        public string ShiftName => _request.Shift == 0 ? "Дневная" : "Ночная";

        public string EquipmentId
        {
            get => _request.EquipmentId;
            set
            {
                _request.EquipmentId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EquipmentName));
                OnPropertyChanged(nameof(CanOrderMultiple));
                OnPropertyChanged(nameof(GroupDisplayString));

                if (!CanOrderMultiple && RequestedCount > 1)
                {
                    RequestedCount = 1;
                }
            }
        }

        public string EquipmentName => _request.Equipment?.Name ?? "";

        public string LicensePlateId
        {
            get => _request.LicensePlateId;
            set
            {
                if (_isUpdatingRelatedProperties) return;
                try
                {
                    _isUpdatingRelatedProperties = true;
                    _request.LicensePlateId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PlateNumber));
                    OnPropertyChanged(nameof(PlateDisplay));
                    OnPropertyChanged(nameof(GroupDisplayString));

                    if (!string.IsNullOrEmpty(value))
                    {
                        var plate = _parent.AllLicensePlates?.FirstOrDefault(lp => lp.Id == value);
                        if (plate != null && !string.IsNullOrEmpty(plate.LessorOrganizationId))
                        {
                            LessorOrganizationId = plate.LessorOrganizationId;
                        }
                    }
                }
                finally
                {
                    _isUpdatingRelatedProperties = false;
                }
            }
        }

        public string PlateNumber => _request.LicensePlate?.PlateNumber ?? "";

        public string PlateDisplay
        {
            get
            {
                if (_request.LicensePlate != null)
                {
                    return $"{_request.LicensePlate.PlateNumber} - {_request.LicensePlate.Brand}";
                }
                return "";
            }
        }

        public string WarehouseId
        {
            get => _request.WarehouseId;
            set
            {
                _request.WarehouseId = value;
                _availableAreas = null; // Сбрасываем кэш территорий
                OnPropertyChanged();
                OnPropertyChanged(nameof(WarehouseName));
                OnPropertyChanged(nameof(AvailableAreas));

                // Сбрасываем территорию при смене склада
                AreaId = null;
            }
        }

        public string WarehouseName => _request.Warehouse?.Name ?? "";

        public string AreaId
        {
            get => _request.AreaId;
            set
            {
                _request.AreaId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AreaName));
                OnPropertyChanged(nameof(GroupDisplayString));
            }
        }

        public string AreaName => _request.Area?.Name ?? "";

        public string LessorOrganizationId
        {
            get => _request.LessorOrganizationId;
            set
            {
                if (_isUpdatingRelatedProperties) return;
                try
                {
                    _isUpdatingRelatedProperties = true;
                    _request.LessorOrganizationId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LessorName));
                    OnPropertyChanged(nameof(GroupDisplayString));
                    _parent?.NotifyPropertyChanged(nameof(MainWindowViewModel.FilteredLicensePlates));
                    OnPropertyChanged(nameof(FilteredLicensePlates));

                    if (!string.IsNullOrEmpty(LicensePlateId))
                    {
                        var plate = _parent.AllLicensePlates?.FirstOrDefault(lp => lp.Id == LicensePlateId);
                        if (plate != null && plate.LessorOrganizationId != value)
                        {
                            LicensePlateId = null;
                        }
                    }
                }
                finally
                {
                    _isUpdatingRelatedProperties = false;
                }
            }
        }

        public string LessorName => _request.LessorOrganization?.Name ?? "";

        public int RequestedCount
        {
            get => _request.RequestedCount;
            set
            {
                if (value >= 1)
                {
                    _request.RequestedCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(GroupDisplayString));
                }
            }
        }

        public decimal? WorkedHours
        {
            get => _request.WorkedHours;
            set
            {
                _request.WorkedHours = value;
                OnPropertyChanged();
                if (value.HasValue && _request.HourlyCost.HasValue)
                {
                    ActualCost = Math.Round(value.Value * _request.HourlyCost.Value, 2);
                }
            }
        }

        public decimal? ActualCost
        {
            get => _request.ActualCost;
            set
            {
                _request.ActualCost = value;
                OnPropertyChanged();
            }
        }

        public bool IsWorked
        {
            get => _request.IsWorked;
            set
            {
                _request.IsWorked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RowBackgroundColor));
            }
        }

        public bool IsNotProvided
        {
            get => _request.IsNotProvided;
            set
            {
                _request.IsNotProvided = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RowBackgroundColor));
                if (value)
                {
                    WorkedHours = 0;
                    IsWeatherCancellation = false;
                }
            }
        }

        public bool IsWeatherCancellation
        {
            get => _request.IsWeatherCancellation;
            set
            {
                _request.IsWeatherCancellation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RowBackgroundColor));
                if (value)
                {
                    WorkedHours = 0;
                    IsNotProvided = false;
                }
            }
        }

        public string CancellationReason
        {
            get => _request.CancellationReason;
            set
            {
                _request.CancellationReason = value;
                OnPropertyChanged();
            }
        }

        public bool IsBlocked => _request.IsBlocked;
        public string LockedByUserId => _request.LockedByUserId;
        public DateTime? LockedAt => _request.LockedAt;

        public string Comment
        {
            get => _request.Comment;
            set
            {
                _request.Comment = value;
                OnPropertyChanged();
            }
        }

        public string CreatedByUserId => _request.CreatedByUserId;
        public string CreatedByUser => _request.CreatedByUser?.FullName ?? "";
        public DateTime CreatedAt => _request.CreatedAt;

        // В сеттерах сбрасываем кэш
        public string DepartmentId
        {
            get => _request.DepartmentId;
            set
            {
                _request.DepartmentId = value;
                _availableWarehouses = null; // Сбрасываем кэш складов
                OnPropertyChanged();
                OnPropertyChanged(nameof(DepartmentName));
                OnPropertyChanged(nameof(AvailableWarehouses));
            }
        }

        public string DepartmentName => _request.Department?.Name ?? "";

        // Группировка в одну строку
        public string GroupDisplayString
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(DepartmentName)) parts.Add(DepartmentName);
                if (Date != default) parts.Add(Date.ToString("dd.MM.yyyy"));
                if (!string.IsNullOrEmpty(ShiftName)) parts.Add(ShiftName);
                if (!string.IsNullOrEmpty(WarehouseName)) parts.Add(WarehouseName);
                return string.Join(" | ", parts);
            }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(EquipmentId) &&
                       !string.IsNullOrEmpty(WarehouseId) &&
                       RequestedCount > 0;
            }
        }

        public bool CanEditEquipment => _authorizationService.CanWriteTable("Equipments");
        public bool CanEditWarehouse => _authorizationService.CanWriteTable("Warehouses");
        public bool CanEditLessor => _authorizationService.CanWriteTable("LessorOrganizations");
        public bool CanOrderMultiple => _request.Equipment?.CanOrderMultiple ?? false;

        // Доступные территории для выбранного склада
        private List<WarehouseArea> _availableAreas;
        public List<WarehouseArea> AvailableAreas
        {
            get
            {
                if (string.IsNullOrEmpty(WarehouseId))
                    return new List<WarehouseArea>();

                if (_availableAreas == null)
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        _availableAreas = context.WarehouseAreas
                            .Where(wa => wa.WarehouseId == WarehouseId && wa.IsActive)
                            .OrderBy(wa => wa.Name)
                            .ToList();
                        System.Diagnostics.Debug.WriteLine($"AvailableAreas: загружено {_availableAreas.Count} территорий");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading AvailableAreas: {ex.Message}");
                        _availableAreas = new List<WarehouseArea>();
                    }
                }
                return _availableAreas;
            }
        }


        // Доступные склады для выбранного отдела
        private List<Warehouse> _availableWarehouses;
        public List<Warehouse> AvailableWarehouses
        {
            get
            {
                if (string.IsNullOrEmpty(DepartmentId))
                    return new List<Warehouse>();

                if (_availableWarehouses == null)
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        _availableWarehouses = context.Warehouses
                            .Where(w => w.DepartmentId == DepartmentId && w.IsActive)
                            .OrderBy(w => w.Name)
                            .ToList();
                        System.Diagnostics.Debug.WriteLine($"AvailableWarehouses: загружено {_availableWarehouses.Count} складов");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading AvailableWarehouses: {ex.Message}");
                        _availableWarehouses = new List<Warehouse>();
                    }
                }
                return _availableWarehouses;
            }
        }

        public string RowBackgroundColor
        {
            get
            {
                if (IsBlocked) return "#FFF0F0F0";
                if (IsWorked) return "#FFE0FFE0";
                if (IsNotProvided) return "#FFFFE0E0";
                if (IsWeatherCancellation) return "#FFE0F0FF";
                return "White";
            }
        }

        public void Cleanup()
        {
            // Очистка ресурсов при необходимости
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Cleanup();
                }
                _disposed = true;
            }
        }

        ~ShiftRequestViewModel()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Конвертер для группировки по дате (оставлен для совместимости)
    /// </summary>
    public class DateGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.ToString("dd.MM.yyyy (dddd)", new System.Globalization.CultureInfo("ru-RU"));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}