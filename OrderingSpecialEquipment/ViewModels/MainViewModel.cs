using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Commands;
using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; // Для Application.Current.Dispatcher
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly IThemeService _themeService;
        private readonly IFavoriteEquipmentService _favoriteEquipmentService;
        private readonly IShiftRequestService _shiftRequestService;
        private readonly IEquipmentRepositoryBase _equipmentRepository;
        private readonly IEquipmentDependencyRepository _equipmentDependencyRepository;
        private readonly IServiceProvider _serviceProvider;

        private ViewModelBase? _currentView;
        private User? _currentUser;
        private string _currentUserInfo = "Пользователь не аутентифицирован";

        // --- ПОЛЯ ДЛЯ ПАНЕЛИ ИЗБРАННОГО ---
        private bool _isFavoriteEquipmentPanelVisible = false; // Поле для состояния панели
        private ObservableCollection<Equipment> _favoriteEquipment;
        private Equipment? _selectedFavoriteEquipment;

        // --- ПОЛЯ ДЛЯ ОСНОВНОЙ ТАБЛИЦЫ ЗАЯВОК ---
        private ObservableCollection<ShiftRequest> _shiftRequestsToShow;
        private ShiftRequest? _selectedShiftRequest;

        // --- ПОЛЯ ДЛЯ ПАНЕЛИ ТЕХНИКИ (открытая панель) ---
        private bool _isEquipmentPanelVisible = false; // Отдельное поле для панели "доступной техники"
        private ObservableCollection<Equipment> _availableEquipment;
        private Equipment? _selectedAvailableEquipment;

        public MainViewModel(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            IThemeService themeService,
            IFavoriteEquipmentService favoriteEquipmentService,
            IShiftRequestService shiftRequestService,
            IEquipmentRepositoryBase equipmentRepository,
            IEquipmentDependencyRepository equipmentDependencyRepository,
            IServiceProvider serviceProvider)
        {
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _themeService = themeService;
            _favoriteEquipmentService = favoriteEquipmentService;
            _shiftRequestService = shiftRequestService;
            _equipmentRepository = equipmentRepository;
            _equipmentDependencyRepository = equipmentDependencyRepository;
            _serviceProvider = serviceProvider;

            // Инициализируем коллекции
            _favoriteEquipment = new ObservableCollection<Equipment>();
            _shiftRequestsToShow = new ObservableCollection<ShiftRequest>();
            _availableEquipment = new ObservableCollection<Equipment>(); // Добавляем коллекцию доступной техники

            InitializeApplication(themeService);
        }

        // --- СВОЙСТВА ---
        public ViewModelBase? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string CurrentUserInfo
        {
            get => _currentUserInfo;
            set => SetProperty(ref _currentUserInfo, value);
        }

        // --- СВОЙСТВА ДЛЯ ПАНЕЛИ ИЗБРАННОГО ---
        public bool IsFavoriteEquipmentPanelVisible
        {
            get => _isFavoriteEquipmentPanelVisible;
            set => SetProperty(ref _isFavoriteEquipmentPanelVisible, value);
        }

        public ObservableCollection<Equipment> FavoriteEquipment
        {
            get => _favoriteEquipment;
            set => SetProperty(ref _favoriteEquipment, value);
        }

        public Equipment? SelectedFavoriteEquipment
        {
            get => _selectedFavoriteEquipment;
            set => SetProperty(ref _selectedFavoriteEquipment, value);
        }

        // --- СВОЙСТВА ДЛЯ ОСНОВНОЙ ТАБЛИЦЫ ЗАЯВОК ---
        public ObservableCollection<ShiftRequest> ShiftRequestsToShow
        {
            get => _shiftRequestsToShow;
            set => SetProperty(ref _shiftRequestsToShow, value);
        }

        public ShiftRequest? SelectedShiftRequest
        {
            get => _selectedShiftRequest;
            set => SetProperty(ref _selectedShiftRequest, value);
        }

        // --- СВОЙСТВА ДЛЯ ПАНЕЛИ ТЕХНИКИ (открытая панель) ---
        public bool IsEquipmentPanelVisible // Новое свойство
        {
            get => _isEquipmentPanelVisible;
            set
            {
                if (SetProperty(ref _isEquipmentPanelVisible, value))
                {
                    // При изменении видимости панели техники, перезагружаем заявки
                    Task.Run(async () => await LoadShiftRequestsBasedOnPanelStateAsync());
                    // Также, если панель открыта, загружаем доступную технику
                    if (IsEquipmentPanelVisible)
                    {
                        LoadAvailableEquipmentAsync(); // Загружаем в фоне
                    }
                }
            }
        }

        public ObservableCollection<Equipment> AvailableEquipment // Новое свойство
        {
            get => _availableEquipment;
            set => SetProperty(ref _availableEquipment, value);
        }

        public Equipment? SelectedAvailableEquipment // Новое свойство
        {
            get => _selectedAvailableEquipment;
            set => SetProperty(ref _selectedAvailableEquipment, value);
        }

        // --- КОМАНДЫ ---
        public ICommand NavigateToEditDepartmentsCommand { get; private set; } = null!;
        public ICommand NavigateToEditEquipmentCommand { get; private set; } = null!;
        // ... остальные команды навигации ...
        public ICommand ToggleFavoriteEquipmentPanelCommand { get; private set; } = null!; // Новая команда для панели избранного
        public ICommand RefreshFavoriteEquipmentCommand { get; private set; } = null!; // Новая команда
        public ICommand SelectFavoriteEquipmentCommand { get; private set; } = null!; // Новая команда
        public ICommand ToggleEquipmentPanelCommand { get; private set; } = null!; // Новая команда для панели техники
        public ICommand RefreshAvailableEquipmentCommand { get; private set; } = null!; // Новая команда
        public ICommand AddRequestFromEquipmentCommand { get; private set; } = null!; // Новая команда
        public ICommand RefreshShiftRequestsCommand { get; private set; } = null!; // Новая команда

        private void InitializeApplication(IThemeService themeService)
        {
            // --- АУТЕНТИФИКАЦИЯ ---
            _currentUser = _authenticationService.AuthenticateCurrentUser(); // Используем синхронный метод
            if (_currentUser != null)
            {
                _currentUserInfo = $"Пользователь: {_currentUser.FullName} ({_currentUser.WindowsLogin})";
                _authorizationService.InitializeForUser(_currentUser);

                // Загружаем избранное для текущего пользователя
                Task.Run(async () => await LoadFavoriteEquipmentAsync());
            }
            else
            {
                _messageService.ShowErrorMessage("Не удалось аутентифицировать пользователя Windows. Приложение будет закрыто.", "Ошибка аутентификации");
                Application.Current.Shutdown(); // Завершаем приложение, если не удалось аутентифицировать
                return; // Прерываем инициализацию
            }

            // --- НАСТРОЙКА ТЕМЫ ---
            var savedTheme = themeService.LoadThemePreference();
            themeService.ApplyTheme(savedTheme);

            // --- ИНИЦИАЛИЗАЦИЯ КОМАНД ---
            bool isDatabaseConnected = true; // Заглушка - заменить на реальную проверку
            NavigateToEditDepartmentsCommand = new RelayCommand(_ => NavigateToView<EditDepartmentsViewModel>(), _ => isDatabaseConnected && (_authorizationService.CanReadTable("Departments") || _authorizationService.CanWriteTable("Departments")));
            NavigateToEditEquipmentCommand = new RelayCommand(_ => NavigateToView<EditEquipmentViewModel>(), _ => isDatabaseConnected && (_authorizationService.CanReadTable("Equipments") || _authorizationService.CanWriteTable("Equipments")));
            // ... остальные команды навигации ...

            // --- ИНИЦИАЛИЗАЦИЯ НОВЫХ КОМАНД ---
            ToggleFavoriteEquipmentPanelCommand = new RelayCommand(_ => IsFavoriteEquipmentPanelVisible = !IsFavoriteEquipmentPanelVisible, _ => isDatabaseConnected && _currentUser != null);
            RefreshFavoriteEquipmentCommand = new RelayCommand(async _ => await LoadFavoriteEquipmentAsync(), _ => isDatabaseConnected && _currentUser != null);
            SelectFavoriteEquipmentCommand = new RelayCommandOfT<string>(OnSelectFavoriteEquipment, _ => isDatabaseConnected && _currentUser != null);

            ToggleEquipmentPanelCommand = new RelayCommand(_ => IsEquipmentPanelVisible = !IsEquipmentPanelVisible, _ => isDatabaseConnected && _currentUser != null);
            RefreshAvailableEquipmentCommand = new RelayCommand(async _ => await LoadAvailableEquipmentAsync(), _ => isDatabaseConnected);
            AddRequestFromEquipmentCommand = new RelayCommand(async _ => await AddRequestFromSelectedEquipmentAsync(), _ => isDatabaseConnected && SelectedAvailableEquipment != null && IsEquipmentPanelVisible);
            RefreshShiftRequestsCommand = new RelayCommand(async _ => await LoadShiftRequestsBasedOnPanelStateAsync(), _ => isDatabaseConnected);

            // --- ЗАГРУЗКА НАЧАЛЬНЫХ ДАННЫХ ---
            Task.Run(async () => await LoadShiftRequestsBasedOnPanelStateAsync()); // Загружаем заявки в фоне

            // NavigateToView<ReportsViewModel>(); // Или другое начальное представление
        }

        // --- МЕТОДЫ ---
        private async Task LoadFavoriteEquipmentAsync()
        {
            if (_currentUser == null) return;

            try
            {
                var favoriteList = await _favoriteEquipmentService.GetFavoriteEquipmentAsync(_currentUser.Id);
                Application.Current.Dispatcher.Invoke(() => // Обновляем UI в основном потоке
                {
                    FavoriteEquipment.Clear();
                    foreach (var eq in favoriteList.OrderBy(e => e.Name))
                    {
                        FavoriteEquipment.Add(eq);
                    }
                    // Показываем панель, если есть избранное и пользователь аутентифицирован
                    IsFavoriteEquipmentPanelVisible = favoriteList.Any();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки избранного: {ex.Message}");
                // Логировать через ILogger
            }
        }

        private async Task LoadShiftRequestsBasedOnPanelStateAsync()
        {
            if (_currentUser == null) return;

            try
            {
                var now = DateTime.Now;
                var today = now.Date;
                var yesterday = today.AddDays(-1);
                var tomorrow = today.AddDays(1);

                List<ShiftRequest> requestsToLoad = new List<ShiftRequest>();

                if (IsEquipmentPanelVisible)
                {
                    // Открытая панель: текущая ночь (сегодня, смена 0) и завтрашний день (завтра, смена 1)
                    var currentNightRequests = await _shiftRequestService.FindRequestsAsync(date: today, shift: 0);
                    var nextDayRequests = await _shiftRequestService.FindRequestsAsync(date: tomorrow, shift: 1);
                    requestsToLoad.AddRange(currentNightRequests);
                    requestsToLoad.AddRange(nextDayRequests);
                }
                else
                {
                    // Закрытая панель: прошлая ночь (вчера, смена 0) и текущий день (сегодня, смена 1)
                    var previousNightRequests = await _shiftRequestService.FindRequestsAsync(date: yesterday, shift: 0);
                    var currentDayRequests = await _shiftRequestService.FindRequestsAsync(date: today, shift: 1);
                    requestsToLoad.AddRange(previousNightRequests);
                    requestsToLoad.AddRange(currentDayRequests);
                }

                // Сортировка
                requestsToLoad.Sort((x, y) => x.Date.CompareTo(y.Date) != 0 ? x.Date.CompareTo(y.Date) : x.Shift.CompareTo(y.Shift));

                Application.Current.Dispatcher.Invoke(() => // Обновляем UI в основном потоке
                {
                    ShiftRequestsToShow.Clear();
                    foreach (var req in requestsToLoad)
                    {
                        ShiftRequestsToShow.Add(req);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки заявок: {ex.Message}");
                // Логировать через ILogger
            }
        }

        private async Task LoadAvailableEquipmentAsync()
        {
            if (_currentUser == null) return;

            try
            {
                var allEquipment = await _equipmentRepository.GetAllAsync();
                var availableEquipmentList = allEquipment.Where(e => e.IsActive).ToList();

                Application.Current.Dispatcher.Invoke(() => // Обновляем UI в основном потоке
                {
                    AvailableEquipment.Clear();
                    foreach (var eq in availableEquipmentList.OrderBy(e => e.Name))
                    {
                        AvailableEquipment.Add(eq);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки доступной техники: {ex.Message}");
                // Логировать через ILogger
            }
        }

        private async Task AddRequestFromSelectedEquipmentAsync()
        {
            if (SelectedAvailableEquipment == null) return;

            var equipmentToAdd = SelectedAvailableEquipment;

            try
            {
                // Создаём новую заявку
                var newRequest = new ShiftRequest
                {
                    Date = DateTime.Today, // По умолчанию сегодня
                    Shift = IsEquipmentPanelVisible ? 0 : 1, // Если панель открыта, предполагаем ночь для новой заявки
                    EquipmentId = equipmentToAdd.Id,
                    RequestedCount = 1, // По умолчанию 1
                    CreatedByUserId = _currentUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    DepartmentId = _currentUser.DefaultDepartmentId, // Используем отдел по умолчанию
                    IsBlocked = false,
                    IsWorked = false
                };

                // --- ПРОВЕРКА ЗАВИСИМОСТЕЙ ---
                var dependencies = await _equipmentDependencyRepository.GetByMainEquipmentAsync(equipmentToAdd.Id);
                var existingRequestsForDateShift = await _shiftRequestService.FindRequestsAsync(date: newRequest.Date, shift: newRequest.Shift);

                foreach (var dep in dependencies)
                {
                    // Находим существующие заявки на зависимую технику
                    var existingDependentRequests = existingRequestsForDateShift.Where(r => r.EquipmentId == dep.DependentEquipmentId).ToList();
                    int totalRequestedDependent = existingDependentRequests.Sum(r => r.RequestedCount);

                    if (dep.IsMandatory && totalRequestedDependent < dep.RequiredCount)
                    {
                        int countToAdd = dep.RequiredCount - totalRequestedDependent;
                        if (countToAdd > 0)
                        {
                            // Автоматически добавляем зависимую технику
                            var dependentRequest = new ShiftRequest
                            {
                                Date = newRequest.Date,
                                Shift = newRequest.Shift,
                                EquipmentId = dep.DependentEquipmentId,
                                RequestedCount = countToAdd,
                                CreatedByUserId = newRequest.CreatedByUserId,
                                CreatedAt = newRequest.CreatedAt,
                                DepartmentId = newRequest.DepartmentId,
                                IsBlocked = false,
                                IsWorked = false
                            };

                            // Проверим, есть ли уже такая заявка в списке (например, если она была добавлена до этого вручную)
                            if (!existingRequestsForDateShift.Any(r => r.EquipmentId == dep.DependentEquipmentId))
                            {
                                // Создаём зависимую заявку
                                await _shiftRequestService.CreateRequestAsync(dependentRequest, _currentUser.Id);
                                _messageService.ShowInfoMessage($"Добавлена зависимая техника: {dep.DependentEquipmentId} (x{countToAdd})", "Зависимость");
                            }
                            else
                            {
                                _messageService.ShowInfoMessage($"Заявка на зависимую технику {dep.DependentEquipmentId} уже существует. Увеличьте количество вручную, если необходимо.", "Зависимость");
                            }
                        }
                    }
                }

                // Теперь добавляем основную заявку
                await _shiftRequestService.CreateRequestAsync(newRequest, _currentUser.Id);

                // Обновляем список заявок
                await LoadShiftRequestsBasedOnPanelStateAsync();

                _messageService.ShowInfoMessage("Заявка добавлена.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка добавления заявки: {ex.Message}", "Ошибка");
            }
        }

        private void OnSelectFavoriteEquipment(string? equipmentId)
        {
            if (string.IsNullOrEmpty(equipmentId))
            {
                Console.WriteLine("ID оборудования не передан.");
                return;
            }
            _messageService.ShowInfoMessage($"Выбрана избранная техника с ID: {equipmentId}", "Избранное");
        }

        private void NavigateToView<T>() where T : ViewModelBase
        {
            var viewModel = _serviceProvider.GetService(typeof(T)) as T;
            if (viewModel != null)
            {
                CurrentView = viewModel;
            }
            else
            {
                _messageService.ShowErrorMessage($"Не удалось создать ViewModel: {typeof(T).Name}", "Ошибка навигации");
            }
        }
    }
}