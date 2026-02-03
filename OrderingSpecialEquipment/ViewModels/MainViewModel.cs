using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// Главная ViewModel основного окна приложения
    /// Управляет заявками, техникой, избранным и вкладками
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IShiftRequestService _shiftRequestService;
        private readonly IFavoriteEquipmentService _favoriteEquipmentService;
        private readonly IThemeService _themeService;

        // ========== Поля ==========

        private User? _currentUser;
        private DateTime _selectedDate;
        private bool _isEquipmentPanelVisible;
        private ObservableCollection<ShiftRequest> _shiftRequestsToShow;
        private ObservableCollection<Equipment> _availableEquipment;
        private ObservableCollection<Equipment> _favoriteEquipment;
        private string _statusMessage;
        private Equipment? _selectedEquipment;
        private Equipment? _selectedFavoriteEquipment;
        private string _headerText;

        // ========== Свойства ==========

        /// <summary>
        /// Текущий авторизованный пользователь
        /// </summary>
        public User? CurrentUser
        {
            get => _currentUser;
            private set => SetProperty(ref _currentUser, value);
        }

        /// <summary>
        /// Выбранная дата в календаре
        /// </summary>
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    // При изменении даты обновляем заявки
                    _ = LoadShiftRequestsAsync();
                }
            }
        }

        /// <summary>
        /// Видимость панели техники
        /// </summary>
        public bool IsEquipmentPanelVisible
        {
            get => _isEquipmentPanelVisible;
            set
            {
                if (SetProperty(ref _isEquipmentPanelVisible, value))
                {
                    // При открытии панели техники переключаем дату на следующий день
                    if (value)
                    {
                        SelectedDate = SelectedDate.AddDays(1);
                    }
                    // При закрытии панели техники показываем ночь предыдущего и день текущего
                    else
                    {
                        _ = LoadShiftRequestsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Заявки для отображения в таблице
        /// </summary>
        public ObservableCollection<ShiftRequest> ShiftRequestsToShow
        {
            get => _shiftRequestsToShow;
            set => SetProperty(ref _shiftRequestsToShow, value);
        }

        /// <summary>
        /// Доступная для заказа техника
        /// </summary>
        public ObservableCollection<Equipment> AvailableEquipment
        {
            get => _availableEquipment;
            set => SetProperty(ref _availableEquipment, value);
        }

        /// <summary>
        /// Избранная техника пользователя
        /// </summary>
        public ObservableCollection<Equipment> FavoriteEquipment
        {
            get => _favoriteEquipment;
            set => SetProperty(ref _favoriteEquipment, value);
        }

        /// <summary>
        /// Статусное сообщение для отображения внизу окна
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Выбранная техника в списке доступной
        /// </summary>
        public Equipment? SelectedEquipment
        {
            get => _selectedEquipment;
            set => SetProperty(ref _selectedEquipment, value);
        }

        /// <summary>
        /// Выбранная техника в списке избранного
        /// </summary>
        public Equipment? SelectedFavoriteEquipment
        {
            get => _selectedFavoriteEquipment;
            set => SetProperty(ref _selectedFavoriteEquipment, value);
        }

        /// <summary>
        /// Заголовок таблицы заявок (динамически меняется в зависимости от режима)
        /// </summary>
        public string HeaderText
        {
            get => _headerText;
            set => SetProperty(ref _headerText, value);
        }

        // ========== Команды ==========

        public ICommand ToggleEquipmentPanelCommand { get; }
        public ICommand RefreshAvailableEquipmentCommand { get; }
        public ICommand RefreshShiftRequestsCommand { get; }
        public ICommand SetTodayDateCommand { get; }
        public ICommand DeleteRequestCommand { get; }
        public ICommand AddRequestFromEquipmentCommand { get; }
        public ICommand RefreshFavoriteEquipmentCommand { get; }
        public ICommand SelectFavoriteEquipmentCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand OpenEditDepartmentsCommand { get; }
        public ICommand OpenEditEquipmentsCommand { get; }
        public ICommand OpenEditWarehousesCommand { get; }
        public ICommand OpenEditAccessRightsCommand { get; }
        public ICommand OpenEditUsersCommand { get; }
        public ICommand OpenEditRolesCommand { get; }
        public ICommand OpenEditLessorOrgsCommand { get; }
        public ICommand OpenEditLicensePlatesCommand { get; }
        public ICommand OpenEditTransportProgramCommand { get; }
        public ICommand OpenEditDependenciesCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand OpenThemeSettingsCommand { get; }
        public ICommand OpenReportsCommand { get; }

        // ========== Конструктор ==========

        /// <summary>
        /// Конструктор главной ViewModel
        /// </summary>
        public MainViewModel(
            IServiceProvider serviceProvider,
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IShiftRequestService shiftRequestService,
            IFavoriteEquipmentService favoriteEquipmentService,
            IThemeService themeService)
        {
            _serviceProvider = serviceProvider;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _shiftRequestService = shiftRequestService;
            _favoriteEquipmentService = favoriteEquipmentService;
            _themeService = themeService;

            // Инициализация коллекций
            ShiftRequestsToShow = new ObservableCollection<ShiftRequest>();
            AvailableEquipment = new ObservableCollection<Equipment>();
            FavoriteEquipment = new ObservableCollection<Equipment>();
            StatusMessage = "Загрузка приложения...";
            HeaderText = "Загрузка данных...";

            // Инициализация команд
            ToggleEquipmentPanelCommand = new RelayCommand(ToggleEquipmentPanel);
            RefreshAvailableEquipmentCommand = new RelayCommand(RefreshAvailableEquipment);
            RefreshShiftRequestsCommand = new RelayCommand(RefreshShiftRequests);
            SetTodayDateCommand = new RelayCommand(SetTodayDate);
            DeleteRequestCommand = new RelayCommand<ShiftRequest>(DeleteRequest, CanDeleteRequest);
            AddRequestFromEquipmentCommand = new RelayCommand<Equipment>(AddRequestFromEquipment, CanAddRequestFromEquipment);
            RefreshFavoriteEquipmentCommand = new RelayCommand(RefreshFavoriteEquipment);
            SelectFavoriteEquipmentCommand = new RelayCommand<Equipment>(SelectFavoriteEquipment);
            ToggleFavoriteCommand = new RelayCommand<Equipment>(ToggleFavorite);
            OpenEditDepartmentsCommand = new RelayCommand(OpenEditDepartments);
            OpenEditEquipmentsCommand = new RelayCommand(OpenEditEquipments);
            OpenEditWarehousesCommand = new RelayCommand(OpenEditWarehouses);
            OpenEditAccessRightsCommand = new RelayCommand(OpenEditAccessRights);
            OpenEditUsersCommand = new RelayCommand(OpenEditUsers);
            OpenEditRolesCommand = new RelayCommand(OpenEditRoles);
            OpenEditLessorOrgsCommand = new RelayCommand(OpenEditLessorOrganizations);
            OpenEditLicensePlatesCommand = new RelayCommand(OpenEditLicensePlates);
            OpenEditTransportProgramCommand = new RelayCommand(OpenEditTransportProgram);
            OpenEditDependenciesCommand = new RelayCommand(OpenEditDependencies);
            ExportToExcelCommand = new RelayCommand(ExportToExcel, CanExportToExcel);
            OpenThemeSettingsCommand = new RelayCommand(OpenThemeSettings);
            OpenReportsCommand = new RelayCommand(OpenReports);

            // Установка текущей даты по умолчанию
            SelectedDate = DateTime.Today;
            HeaderText = $"Ночь {SelectedDate.AddDays(-1):dd.MM.yyyy} + День {SelectedDate:dd.MM.yyyy}";

            // Аутентификация пользователя и загрузка данных
            _ = InitializeAsync();
        }

        // ========== Методы инициализации ==========

        /// <summary>
        /// Инициализация при запуске (аутентификация и загрузка данных)
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                StatusMessage = "Аутентификация пользователя...";

                // Аутентификация текущего пользователя
                CurrentUser = await _authenticationService.AuthenticateCurrentUserAsync();

                if (CurrentUser == null)
                {
                    MessageBox.Show(
                        "Пользователь не найден в системе. Обратитесь к администратору.",
                        "Ошибка авторизации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }

                StatusMessage = $"Добро пожаловать, {CurrentUser.FullName}! Загрузка данных...";

                // Загрузка начальных данных параллельно
                var loadTasks = new Task[]
                {
                    LoadAvailableEquipmentAsync(),
                    LoadFavoriteEquipmentAsync(),
                    LoadShiftRequestsAsync()
                };

                await Task.WhenAll(loadTasks);

                StatusMessage = "Готово к работе";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show(
                    $"Ошибка инициализации: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ========== Методы загрузки данных ==========

        /// <summary>
        /// Загружает доступную технику
        /// </summary>
        private async Task LoadAvailableEquipmentAsync()
        {
            try
            {
                if (CurrentUser == null)
                    return;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var equipmentRepo = scope.ServiceProvider.GetRequiredService<IEquipmentRepository>();
                    var equipmentList = await equipmentRepo.GetActiveEquipmentAsync();

                    // Фильтруем по правам доступа пользователя (если необходимо)
                    AvailableEquipment = new ObservableCollection<Equipment>(equipmentList);
                    StatusMessage = $"Загружено {equipmentList.Count()} единиц техники";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки техники: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки техники: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает избранную технику пользователя
        /// </summary>
        private async Task LoadFavoriteEquipmentAsync()
        {
            try
            {
                if (CurrentUser == null)
                    return;

                var favorites = await _favoriteEquipmentService.GetFavoriteEquipmentAsync(CurrentUser.Id);
                FavoriteEquipment = new ObservableCollection<Equipment>(favorites);
                StatusMessage = $"Загружено {favorites.Count()} единиц избранной техники";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки избранного: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки избранного: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает заявки для отображения
        /// Если панель техники скрыта - показывает ночь предыдущего и день текущего
        /// Если панель техники открыта - показывает заявки на выбранную дату
        /// </summary>
        private async Task LoadShiftRequestsAsync()
        {
            try
            {
                if (CurrentUser == null)
                    return;

                IEnumerable<ShiftRequest> requests;

                if (!IsEquipmentPanelVisible)
                {
                    // Обычный режим: ночь предыдущего дня + день текущего дня
                    DateTime nightDate = SelectedDate.AddDays(-1);
                    DateTime dayDate = SelectedDate;

                    requests = await _shiftRequestService.FindRequestsForTwoShiftsAsync(
                        nightDate, 0,  // Ночь предыдущего дня
                        dayDate, 1     // День текущего дня
                    );
                }
                else
                {
                    // Панель техники открыта: показываем обе смены выбранного дня
                    requests = await _shiftRequestService.FindRequestsForTwoShiftsAsync(
                        SelectedDate, 0,  // Ночь выбранного дня
                        SelectedDate, 1   // День выбранного дня
                    );
                }

                // Фильтруем по правам доступа пользователя
                var filteredRequests = new List<ShiftRequest>();
                foreach (var request in requests)
                {
                    if (await _authorizationService.HasAccessToDepartmentAsync(CurrentUser, request.Warehouse?.DepartmentId ?? ""))
                    {
                        filteredRequests.Add(request);
                    }
                }

                ShiftRequestsToShow = new ObservableCollection<ShiftRequest>(filteredRequests);
                StatusMessage = $"Загружено {filteredRequests.Count} заявок";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки заявок: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== Обработчики команд ==========

        /// <summary>
        /// Переключает видимость панели техники
        /// </summary>
        private void ToggleEquipmentPanel()
        {
            IsEquipmentPanelVisible = !IsEquipmentPanelVisible;
        }

        /// <summary>
        /// Обновляет список доступной техники
        /// </summary>
        private async void RefreshAvailableEquipment()
        {
            StatusMessage = "Обновление списка техники...";
            await LoadAvailableEquipmentAsync();
        }

        /// <summary>
        /// Обновляет список заявок
        /// </summary>
        private async void RefreshShiftRequests()
        {
            StatusMessage = "Обновление списка заявок...";
            await LoadShiftRequestsAsync();
        }

        /// <summary>
        /// Устанавливает текущую дату
        /// </summary>
        private void SetTodayDate()
        {
            SelectedDate = DateTime.Today;
            if (!IsEquipmentPanelVisible)
            {
                HeaderText = $"Ночь {SelectedDate.AddDays(-1):dd.MM.yyyy} + День {SelectedDate:dd.MM.yyyy}";
            }
            else
            {
                HeaderText = $"Заявки на {SelectedDate:dd.MM.yyyy} (ночь и день)";
            }
        }

        /// <summary>
        /// Удаляет заявку
        /// </summary>
        private async void DeleteRequest(ShiftRequest request)
        {
            try
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить заявку на {request.Equipment?.Name}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes && CurrentUser != null)
                {
                    await _shiftRequestService.DeleteRequestAsync(request.Key, CurrentUser);
                    await LoadShiftRequestsAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка удаления заявки: {ex.Message}";
                MessageBox.Show($"Ошибка удаления заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Проверяет возможность удаления заявки
        /// </summary>
        private bool CanDeleteRequest(ShiftRequest request)
        {
            if (CurrentUser == null || request.IsBlocked)
                return false;

            return _authorizationService.CanWriteTable(CurrentUser, "ShiftRequests");
        }

        /// <summary>
        /// Добавляет заявку из техники (кнопка в панели техники)
        /// </summary>
        private async void AddRequestFromEquipment(Equipment equipment)
        {
            try
            {
                if (CurrentUser == null)
                    return;

                // Создаем новую заявку
                var newRequest = new ShiftRequest
                {
                    Date = SelectedDate,
                    Shift = IsEquipmentPanelVisible ? 1 : 0, // День если панель открыта, иначе ночь
                    EquipmentId = equipment.Id,
                    WarehouseId = CurrentUser.DefaultDepartmentId ?? "WH000001", // Берем первый склад или по умолчанию
                    RequestedCount = equipment.CanOrderMultiple ? 1 : 1,
                    CreatedByUserId = CurrentUser.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _shiftRequestService.CreateRequestAsync(newRequest, CurrentUser);
                await LoadShiftRequestsAsync();
                await LoadFavoriteEquipmentAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка создания заявки: {ex.Message}";
                MessageBox.Show($"Ошибка создания заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Проверяет возможность добавления заявки из техники
        /// </summary>
        private bool CanAddRequestFromEquipment(Equipment equipment)
        {
            if (CurrentUser == null)
                return false;

            return _authorizationService.CanWriteTable(CurrentUser, "ShiftRequests");
        }

        /// <summary>
        /// Обновляет список избранной техники
        /// </summary>
        private async void RefreshFavoriteEquipment()
        {
            StatusMessage = "Обновление избранного...";
            await LoadFavoriteEquipmentAsync();
        }

        /// <summary>
        /// Выбирает технику из избранного (добавляет заявку)
        /// </summary>
        private async void SelectFavoriteEquipment(Equipment equipment)
        {
            await AddRequestFromEquipment(equipment);
        }

        /// <summary>
        /// Переключает технику в избранное/из избранного
        /// </summary>
        private async void ToggleFavorite(Equipment equipment)
        {
            try
            {
                if (CurrentUser == null)
                    return;

                bool isFavorite = await _favoriteEquipmentService.IsFavoriteAsync(CurrentUser.Id, equipment.Id);

                if (isFavorite)
                {
                    await _favoriteEquipmentService.RemoveFromFavoriteAsync(CurrentUser.Id, equipment.Id);
                }
                else
                {
                    await _favoriteEquipmentService.AddToFavoriteAsync(CurrentUser.Id, equipment.Id);
                }

                await LoadFavoriteEquipmentAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка обновления избранного: {ex.Message}";
                MessageBox.Show($"Ошибка обновления избранного: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Открывает вкладку редактирования отделов
        /// </summary>
        private void OpenEditDepartments()
        {
            // Реализация открытия вкладки
            var editWindow = new Views.EditDepartmentsView();
            editWindow.DataContext = new EditDepartmentsViewModel(_serviceProvider);
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Открывает вкладку редактирования техники
        /// </summary>
        private void OpenEditEquipments()
        {
            var editWindow = new Views.EditEquipmentsView();
            editWindow.DataContext = new EditEquipmentsViewModel(_serviceProvider);
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Открывает вкладку редактирования складов и территорий
        /// </summary>
        private void OpenEditWarehouses()
        {
            var editWindow = new Views.EditWarehousesView();
            editWindow.DataContext = new EditWarehousesViewModel(_serviceProvider);
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Открывает вкладку редактирования прав доступа
        /// </summary>
        private void OpenEditAccessRights()
        {
            var editWindow = new Views.EditAccessRightsView();
            editWindow.DataContext = new EditAccessRightsViewModel(_serviceProvider);
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Открывает вкладку редактирования пользователей
        /// </summary>
        private void OpenEditUsers()
        {
            var editWindow = new Views.EditUsersView();
            editWindow.DataContext = new EditUsersViewModel(_serviceProvider);
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Открывает вкладку редактирования ролей
        /// </summary>
        private void OpenEditRoles()
        {
            var editWindow = new Views.EditRolesView();
            editWindow.DataContext = new EditRolesViewModel(_serviceProvider);
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Открывает вкладку редактирования организаций-арендодателей
        /// </summary>
        private void OpenEditLessorOrganizations()
        {
            var editWindow = new Views.EditLessorOrganizationsView();
            editWindow.DataContext = new EditLessorOrganizationsViewModel(_serviceProvider);
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Открывает вкладку редактирования госномеров
        /// </summary>
        private void OpenEditLicensePlates()
        {
            var editWindow = new Views.EditLicensePlatesView();
            editWindow.DataContext = new EditLicensePlatesViewModel(_serviceProvider);
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Открывает вкладку редактирования транспортной программы
        /// </summary>
        private void OpenEditTransportProgram()
        {
            var editWindow = new Views.EditTransportProgramView();
            editWindow.DataContext = new EditTransportProgramViewModel(_serviceProvider);
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Открывает вкладку редактирования зависимостей техники
        /// </summary>
        private void OpenEditDependencies()
        {
            var editWindow = new Views.EditEquipmentDependenciesView();
            editWindow.DataContext = new EditEquipmentDependenciesViewModel(_serviceProvider);
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Открывает окно предпросмотра отчетов
        /// </summary>
        private void OpenReports()
        {
            var reportWindow = new Views.Reports.ReportPreviewWindow(
                new ViewModels.Reports.ReportPreviewViewModel(
                    _serviceProvider.GetRequiredService<Services.Reports.IReportService>(),
                    _serviceProvider));
            reportWindow.ShowDialog();
        }

        /// <summary>
        /// Экспортирует данные в Excel
        /// </summary>
        private void ExportToExcel()
        {
            StatusMessage = "Экспорт в Excel начат...";
            // Реализация экспорта через EPPlus будет добавлена позже
            MessageBox.Show("Экспорт в Excel будет реализован в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = "Готово к работе";
        }

        /// <summary>
        /// Проверяет возможность экспорта в Excel
        /// </summary>
        private bool CanExportToExcel()
        {
            return CurrentUser != null && _authorizationService.HasSpecialPermission(CurrentUser, "ExportData");
        }

        /// <summary>
        /// Открывает настройки темы
        /// </summary>
        private void OpenThemeSettings()
        {
            var themeWindow = new Views.ThemeSettingsWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = new ThemeSettingsViewModel(_serviceProvider.GetRequiredService<IThemeService>())
            };
            themeWindow.ShowDialog();
        }
    }
}