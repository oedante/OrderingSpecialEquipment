using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IShiftRequestService _shiftRequestService;
        private readonly IEquipmentService _equipmentService;

        private bool _isLeftPanelVisible = true;
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
        private string _statusMessage = "Готов";

        // Свойства для выпадающих списков
        private List<Department> _accessibleDepartments = new List<Department>();
        private List<Warehouse> _accessibleWarehouses = new List<Warehouse>();
        private List<Equipment> _equipments = new List<Equipment>();
        private List<LicensePlate> _licensePlates = new List<LicensePlate>();
        private List<LessorOrganization> _lessorOrganizations = new List<LessorOrganization>();

        #endregion

        #region Свойства

        /// <summary>
        /// Видимость левой панели
        /// </summary>
        public bool IsLeftPanelVisible
        {
            get => _isLeftPanelVisible;
            set => SetProperty(ref _isLeftPanelVisible, value);
        }

        /// <summary>
        /// Фильтр "Только избранное"
        /// </summary>
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
                    _ = LoadShiftRequestsAsync();
                }
            }
        }

        /// <summary>
        /// Список техники в левой панели
        /// </summary>
        public ObservableCollection<EquipmentItemViewModel> EquipmentItems
        {
            get => _equipmentItems;
            set => SetProperty(ref _equipmentItems, value);
        }

        /// <summary>
        /// Список заявок
        /// </summary>
        public ObservableCollection<ShiftRequestViewModel> ShiftRequests
        {
            get => _shiftRequests;
            set => SetProperty(ref _shiftRequests, value);
        }

        /// <summary>
        /// Выбранная заявка
        /// </summary>
        public ShiftRequestViewModel SelectedRequest
        {
            get => _selectedRequest;
            set
            {
                if (SetProperty(ref _selectedRequest, value) && value != null && !_isEditMode)
                {
                    StartEditRequest(value);
                }
            }
        }

        /// <summary>
        /// Выбранная техника в левой панели
        /// </summary>
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

        /// <summary>
        /// Режим редактирования
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
            set => SetProperty(ref _editingRequest, value);
        }

        /// <summary>
        /// Открыто ли всплывающее окно
        /// </summary>
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set => SetProperty(ref _isPopupOpen, value);
        }

        /// <summary>
        /// Флаг загрузки данных
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
        /// Статус подключения к БД
        /// </summary>
        public bool IsDatabaseConnected => _databaseService.IsConnected;

        /// <summary>
        /// Заголовок окна
        /// </summary>
        public string WindowTitle => $"Управление заявками на спецтехнику - {CurrentUser?.FullName ?? "Не авторизован"}";

        // Свойства для выпадающих списков
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

        public List<Equipment> EquipmentsList
        {
            get => _equipments;
            set => SetProperty(ref _equipments, value);
        }

        public List<LicensePlate> LicensePlates
        {
            get => _licensePlates;
            set => SetProperty(ref _licensePlates, value);
        }

        public List<LessorOrganization> LessorOrganizations
        {
            get => _lessorOrganizations;
            set => SetProperty(ref _lessorOrganizations, value);
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

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор ViewModel главного окна
        /// </summary>
        public MainWindowViewModel(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IDatabaseService databaseService,
            IShiftRequestService shiftRequestService,
            IEquipmentService equipmentService)
        {
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _databaseService = databaseService;
            _shiftRequestService = shiftRequestService;
            _equipmentService = equipmentService;

            // Инициализация команд
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
            OpenConnectionSettingsCommand = new RelayCommand(OpenConnectionSettings, CanOpenConnectionSettings);

            OpenDepartmentsCommand = new RelayCommand(() => OpenReference("Departments"), () => _authorizationService.CanReadTable("Departments"));
            OpenEquipmentsCommand = new RelayCommand(() => OpenReference("Equipments"), () => _authorizationService.CanReadTable("Equipments"));
            OpenWarehousesCommand = new RelayCommand(() => OpenReference("Warehouses"), () => _authorizationService.CanReadTable("Warehouses"));
            OpenLessorsCommand = new RelayCommand(() => OpenReference("LessorOrganizations"), () => _authorizationService.CanReadTable("LessorOrganizations"));
            OpenTransportProgramCommand = new RelayCommand(() => OpenReference("TransportProgram"), () => _authorizationService.CanReadTable("TransportProgram"));
            OpenUsersCommand = new RelayCommand(() => OpenReference("Users"), () => _authorizationService.HasSpecialPermission("ManageUsers") || _authorizationService.IsSystemAdmin);

            OpenTransportReportCommand = new RelayCommand(() => OpenReport("Transport"), () => _authorizationService.HasSpecialPermission("ViewReports"));
            OpenShiftReportCommand = new RelayCommand(() => OpenReport("Shift"), () => _authorizationService.HasSpecialPermission("ViewReports"));

            // Подписка на события
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

            // Установка начальной даты
            _selectedDate = DateTime.UtcNow.Date;
        }

        #endregion

        #region Методы инициализации

        /// <summary>
        /// Инициализация ViewModel
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_databaseService.IsConnected && _authenticationService.IsAuthenticated)
            {
                await LoadDataAsync();
                await LoadComboBoxDataAsync();
            }
        }

        /// <summary>
        /// Загрузка всех данных
        /// </summary>
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                StatusMessage = "Загрузка данных...";
                await Task.WhenAll(
                    LoadEquipmentAsync(),
                    LoadShiftRequestsAsync()
                );
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

                var departmentsTask = _authorizationService.GetAccessibleDepartmentsAsync();
                var warehousesTask = _authorizationService.GetAccessibleWarehousesAsync();
                var equipmentsTask = _databaseService.Context.Equipments.ToListAsync();
                var platesTask = _databaseService.Context.LicensePlates.ToListAsync();
                var lessorsTask = _databaseService.Context.LessorOrganizations.ToListAsync();

                await Task.WhenAll(departmentsTask, warehousesTask, equipmentsTask, platesTask, lessorsTask);

                AccessibleDepartments = await departmentsTask;
                AccessibleWarehouses = await warehousesTask;
                EquipmentsList = await equipmentsTask;
                LicensePlates = await platesTask;
                LessorOrganizations = await lessorsTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных для combo: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка списка техники
        /// </summary>
        private async Task LoadEquipmentAsync()
        {
            if (!_authenticationService.IsAuthenticated || !_databaseService.IsConnected)
                return;

            try
            {
                var equipments = await _equipmentService.GetEquipmentsWithFavoritesAsync(
                    _authenticationService.CurrentUser.Id,
                    _isOnlyFavorites);

                // Получаем доступные отделы для расчета количества заявок
                var departments = await _authorizationService.GetAccessibleDepartmentsAsync();
                var departmentIds = departments.Select(d => d.Id).ToList();

                // Получаем текущие заявки для расчета количества
                var today = DateTime.UtcNow.Date;
                var nightShiftDate = GetNightShiftDate();

                var nightRequests = await _shiftRequestService.GetByDateAndShiftAsync(
                    nightShiftDate, 1); // Ночная смена

                var dayRequests = await _shiftRequestService.GetByDateAndShiftAsync(
                    _isLeftPanelVisible ? today.AddDays(1) : today, 0); // Дневная смена

                var equipmentItems = new ObservableCollection<EquipmentItemViewModel>();

                foreach (var e in equipments)
                {
                    var isFavorite = await _equipmentService.IsFavoriteAsync(_authenticationService.CurrentUser.Id, e.Id);
                    equipmentItems.Add(new EquipmentItemViewModel
                    {
                        Equipment = e,
                        IsFavorite = isFavorite,
                        NightCount = nightRequests.Count(r => r.EquipmentId == e.Id && departmentIds.Contains(r.DepartmentId)),
                        DayCount = dayRequests.Count(r => r.EquipmentId == e.Id && departmentIds.Contains(r.DepartmentId))
                    });
                }

                EquipmentItems = equipmentItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке техники: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка списка заявок
        /// </summary>
        private async Task LoadShiftRequestsAsync()
        {
            if (!_authenticationService.IsAuthenticated || !_databaseService.IsConnected)
                return;

            try
            {
                // Определяем, какую смену показывать
                DateTime targetDate;
                int targetShift;

                if (_isLeftPanelVisible)
                {
                    // Панель открыта: ночная смена ТЕКУЩЕГО дня, дневная смена СЛЕДУЮЩЕГО
                    targetDate = _selectedDate;
                    targetShift = 1; // Ночная
                }
                else
                {
                    // Панель скрыта: ночная смена ПРЕДЫДУЩЕГО дня, дневная смена ТЕКУЩЕГО
                    targetDate = _selectedDate.AddDays(-1);
                    targetShift = 1; // Ночная
                }

                // Загружаем обе смены параллельно
                var nightTask = _shiftRequestService.GetByDateAndShiftAsync(targetDate, targetShift);
                var dayTask = _shiftRequestService.GetByDateAndShiftAsync(
                    _isLeftPanelVisible ? _selectedDate.AddDays(1) : _selectedDate, 0);

                await Task.WhenAll(nightTask, dayTask);

                // Объединяем и преобразуем
                var allRequests = nightTask.Result.Concat(dayTask.Result)
                    .Select(r => new ShiftRequestViewModel(r, _authorizationService))
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
        /// Получение даты для ночной смены в зависимости от состояния панели
        /// </summary>
        private DateTime GetNightShiftDate()
        {
            if (_isLeftPanelVisible)
                return _selectedDate; // Ночь текущего дня
            else
                return _selectedDate.AddDays(-1); // Ночь предыдущего дня
        }

        #endregion

        #region Команды левой панели

        private void ToggleLeftPanel(object parameter)
        {
            IsLeftPanelVisible = !IsLeftPanelVisible;
            _ = LoadShiftRequestsAsync();
            _ = LoadEquipmentAsync();
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
                System.Windows.MessageBox.Show($"Ошибка при изменении избранного: {ex.Message}",
                    "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region Команды заявок

        private bool CanAddRequest(object parameter)
        {
            return _authorizationService.CanWriteTable("ShiftRequests") &&
                   _databaseService.IsConnected;
        }

        private void AddNewRequest(object parameter)
        {
            var newRequest = new ShiftRequest
            {
                Date = _selectedDate,
                Shift = _isLeftPanelVisible ? 1 : 0,
                RequestedCount = 1,
                CreatedByUserId = _authenticationService.CurrentUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            EditingRequest = new ShiftRequestViewModel(newRequest, _authorizationService);
            EditingRequest.IsNew = true;
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

            await _shiftRequestService.LockRequestAsync(request.Key, _authenticationService.CurrentUser.Id);
            EditingRequest = new ShiftRequestViewModel(request.OriginalRequest, _authorizationService);
            EditingRequest.IsEdit = true;
            IsEditMode = true;
            IsPopupOpen = true;
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
                System.Windows.MessageBox.Show($"Ошибка при сохранении заявки: {ex.Message}",
                    "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void CancelEdit(object parameter)
        {
            if (EditingRequest != null && !EditingRequest.IsNew)
            {
                await _shiftRequestService.UnlockRequestAsync(EditingRequest.Key);
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

            var result = System.Windows.MessageBox.Show(
                $"Вы действительно хотите удалить заявку от {request.Date:dd.MM.yyyy}?",
                "Подтверждение удаления",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    await _shiftRequestService.DeleteAsync(request.Key);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при удалении заявки: {ex.Message}",
                        "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void CreateNewRequestFromEquipment(EquipmentItemViewModel equipment)
        {
            var newRequest = new ShiftRequest
            {
                Date = _selectedDate,
                Shift = _isLeftPanelVisible ? 1 : 0,
                EquipmentId = equipment.Equipment.Id,
                RequestedCount = 1,
                CreatedByUserId = _authenticationService.CurrentUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            EditingRequest = new ShiftRequestViewModel(newRequest, _authorizationService);
            EditingRequest.IsNew = true;
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
            settingsWindow.Owner = System.Windows.Application.Current.MainWindow;
            settingsWindow.ShowDialog();
        }

        #endregion

        #region Открытие справочников и отчетов

        private void OpenReference(string referenceName)
        {
            System.Windows.Window window = null;

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
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
                _ = LoadDataAsync();
            }
        }

        private void OpenReport(string reportName)
        {
            System.Windows.Window window = null;

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
                window.Owner = System.Windows.Application.Current.MainWindow;
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

        public string DisplayName => Equipment?.Name ?? "";
        public string DisplayCounts => $"Н:{NightCount} Д:{DayCount}";
    }

    /// <summary>
    /// ViewModel для заявки
    /// </summary>
    public class ShiftRequestViewModel : ViewModelBase
    {
        private readonly ShiftRequest _request;
        private readonly IAuthorizationService _authorizationService;
        private bool _isNew;
        private bool _isEdit;
        private bool _isExpanded;

        public ShiftRequestViewModel(ShiftRequest request, IAuthorizationService authorizationService)
        {
            _request = request;
            _authorizationService = authorizationService;
        }

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
            }
        }

        public string EquipmentName => _request.Equipment?.Name ?? "";

        public string LicensePlateId
        {
            get => _request.LicensePlateId;
            set
            {
                _request.LicensePlateId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlateNumber));
            }
        }

        public string PlateNumber => _request.LicensePlate?.PlateNumber ?? "";

        public string WarehouseId
        {
            get => _request.WarehouseId;
            set
            {
                _request.WarehouseId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WarehouseName));
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
            }
        }

        public string AreaName => _request.Area?.Name ?? "";

        public string LessorOrganizationId
        {
            get => _request.LessorOrganizationId;
            set
            {
                _request.LessorOrganizationId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LessorName));
            }
        }

        public string LessorName => _request.LessorOrganization?.Name ?? "";

        public int RequestedCount
        {
            get => _request.RequestedCount;
            set
            {
                _request.RequestedCount = value;
                OnPropertyChanged();
            }
        }

        public decimal? WorkedHours
        {
            get => _request.WorkedHours;
            set
            {
                _request.WorkedHours = value;
                OnPropertyChanged();
                if (value.HasValue && value > 0 && _request.HourlyCost.HasValue)
                {
                    ActualCost = value * _request.HourlyCost;
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
            }
        }

        public bool IsNotProvided
        {
            get => _request.IsNotProvided;
            set
            {
                _request.IsNotProvided = value;
                OnPropertyChanged();
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

        public string DepartmentId
        {
            get => _request.DepartmentId;
            set
            {
                _request.DepartmentId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DepartmentName));
            }
        }

        public string DepartmentName => _request.Department?.Name ?? "";

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
    }
}