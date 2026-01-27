using OrderingSpecialEquipment.Commands;
using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// ViewModel для окна редактирования прав доступа пользователей к отделам и складам.
    /// </summary>
    public class EditUserAccessViewModel : ViewModelBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IUserDepartmentAccessRepository _userDeptAccessRepository;
        private readonly IUserWarehouseAccessRepository _userWarehouseAccessRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        // Для выбора пользователей
        private ObservableCollection<User> _allUsers;
        private User? _selectedUser;

        // Для отображения и редактирования доступа к отделам
        private ObservableCollection<UserDepartmentAccess> _userDepartmentAccesses;
        private UserDepartmentAccess? _selectedUserDeptAccess;
        private bool _editHasAllWarehouses = false;

        // Для отображения и редактирования доступа к складам (для выбранного отдела)
        private ObservableCollection<Warehouse> _availableWarehousesForSelection; // Все склады выбранного отдела
        private ObservableCollection<Warehouse> _userAllowedWarehouses; // Склады, к которым разрешен доступ
        private Warehouse? _selectedAvailableWarehouse; // Для добавления
        private Warehouse? _selectedAllowedWarehouse; // Для удаления


        public EditUserAccessViewModel(
            IUserRepository userRepository,
            IDepartmentRepository departmentRepository,
            IWarehouseRepository warehouseRepository,
            IUserDepartmentAccessRepository userDeptAccessRepository,
            IUserWarehouseAccessRepository userWarehouseAccessRepository,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
            _warehouseRepository = warehouseRepository;
            _userDeptAccessRepository = userDeptAccessRepository;
            _userWarehouseAccessRepository = userWarehouseAccessRepository;
            _messageService = messageService;
            _authorizationService = authorizationService;

            _allUsers = new ObservableCollection<User>();
            _userDepartmentAccesses = new ObservableCollection<UserDepartmentAccess>();
            _availableWarehousesForSelection = new ObservableCollection<Warehouse>();
            _userAllowedWarehouses = new ObservableCollection<Warehouse>();

            LoadUsersCommand = new RelayCommand(async _ => await LoadUsersAsync(), _ => _authorizationService.HasSpecialPermission("SPEC_ManageUsers"));
            LoadUserAccessCommand = new RelayCommand(async _ => await LoadUserAccessAsync(), _ => SelectedUser != null);
            AddDepartmentAccessCommand = new RelayCommand(async _ => await AddDepartmentAccessAsync(), _ => CanAddDepartmentAccess());
            RemoveDepartmentAccessCommand = new RelayCommand(async _ => await RemoveDepartmentAccessAsync(), _ => SelectedUserDeptAccess != null);
            ToggleAllWarehousesCommand = new RelayCommand(async _ => await ToggleAllWarehousesAsync(), _ => SelectedUserDeptAccess != null);
            LoadAvailableWarehousesCommand = new RelayCommand(async _ => await LoadAvailableWarehousesForDepartmentAsync(), _ => SelectedUserDeptAccess != null);
            AddWarehouseAccessCommand = new RelayCommand(async _ => await AddWarehouseAccessAsync(), _ => CanAddWarehouseAccess());
            RemoveWarehouseAccessCommand = new RelayCommand(async _ => await RemoveWarehouseAccessAsync(), _ => SelectedAllowedWarehouse != null);

            // Загрузка всех пользователей при инициализации
            Task.Run(async () => await LoadUsersAsync());
        }

        // --- Свойства для выбора пользователей ---
        public ObservableCollection<User> AllUsers
        {
            get => _allUsers;
            set => SetProperty(ref _allUsers, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    if (value != null)
                    {
                        // Загрузить доступы для выбранного пользователя
                        Task.Run(async () => await LoadUserAccessAsync());
                    }
                    else
                    {
                        UserDepartmentAccesses.Clear();
                        AvailableWarehousesForSelection.Clear();
                        UserAllowedWarehouses.Clear();
                        SelectedUserDeptAccess = null;
                        SelectedAvailableWarehouse = null;
                        SelectedAllowedWarehouse = null;
                    }
                    ((RelayCommand)LoadUserAccessCommand).RaiseCanExecuteChanged();
                }
            }
        }

        // --- Свойства для доступа к отделам ---
        public ObservableCollection<UserDepartmentAccess> UserDepartmentAccesses
        {
            get => _userDepartmentAccesses;
            set => SetProperty(ref _userDepartmentAccesses, value);
        }

        public UserDepartmentAccess? SelectedUserDeptAccess
        {
            get => _selectedUserDeptAccess;
            set
            {
                if (SetProperty(ref _selectedUserDeptAccess, value))
                {
                    if (value != null)
                    {
                        _editHasAllWarehouses = value.HasAllWarehouses;
                        // Загрузить склады для выбранного доступа к отделу
                        Task.Run(async () => await LoadUserAllowedWarehousesForAccessAsync(value));
                    }
                    else
                    {
                        _editHasAllWarehouses = false;
                        AvailableWarehousesForSelection.Clear();
                        UserAllowedWarehouses.Clear();
                        SelectedAvailableWarehouse = null;
                        SelectedAllowedWarehouse = null;
                    }
                    OnPropertyChanged(nameof(EditHasAllWarehouses));
                    ((RelayCommand)ToggleAllWarehousesCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)LoadAvailableWarehousesCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool EditHasAllWarehouses
        {
            get => _editHasAllWarehouses;
            set
            {
                if (SetProperty(ref _editHasAllWarehouses, value))
                {
                    // Обновить логическое состояние в модели
                    if (SelectedUserDeptAccess != null)
                    {
                        SelectedUserDeptAccess.HasAllWarehouses = value;
                        // Если включено "Все склады", очистить список конкретных складов
                        if (value)
                        {
                            UserAllowedWarehouses.Clear();
                        }
                    }
                }
            }
        }

        // --- Свойства для доступа к складам ---
        public ObservableCollection<Warehouse> AvailableWarehousesForSelection
        {
            get => _availableWarehousesForSelection;
            set => SetProperty(ref _availableWarehousesForSelection, value);
        }

        public ObservableCollection<Warehouse> UserAllowedWarehouses
        {
            get => _userAllowedWarehouses;
            set => SetProperty(ref _userAllowedWarehouses, value);
        }

        public Warehouse? SelectedAvailableWarehouse
        {
            get => _selectedAvailableWarehouse;
            set => SetProperty(ref _selectedAvailableWarehouse, value);
        }

        public Warehouse? SelectedAllowedWarehouse
        {
            get => _selectedAllowedWarehouse;
            set => SetProperty(ref _selectedAllowedWarehouse, value);
        }

        // --- Команды ---
        public ICommand LoadUsersCommand { get; }
        public ICommand LoadUserAccessCommand { get; }
        public ICommand AddDepartmentAccessCommand { get; }
        public ICommand RemoveDepartmentAccessCommand { get; }
        public ICommand ToggleAllWarehousesCommand { get; }
        public ICommand LoadAvailableWarehousesCommand { get; }
        public ICommand AddWarehouseAccessCommand { get; }
        public ICommand RemoveWarehouseAccessCommand { get; }

        // --- Методы ---
        private async Task LoadUsersAsync()
        {
            try
            {
                var dbUsers = await _userRepository.GetAllAsync(); // Или GetActiveAsync()
                AllUsers.Clear();
                foreach (var user in dbUsers)
                {
                    // Можно фильтровать по ролям или другим критериям, если нужно
                    AllUsers.Add(user);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка");
            }
        }

        private async Task LoadUserAccessAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                var accesses = await _userDeptAccessRepository.GetByUserIdAsync(SelectedUser.Id);
                UserDepartmentAccesses.Clear();
                foreach (var acc in accesses)
                {
                    UserDepartmentAccesses.Add(acc);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки доступа к отделам: {ex.Message}", "Ошибка");
            }
        }

        private bool CanAddDepartmentAccess()
        {
            // Проверка прав на редактирование доступа и выбор отдела
            return _authorizationService.HasSpecialPermission("SPEC_ManageUsers") && SelectedUser != null;
            // Также нужно выбрать отдел, но это обычно делается в отдельном диалоге или через ComboBox, который не привязан напрямую к этой VM.
            // Для простоты, предположим, что отдел выбирается в процессе выполнения команды AddDepartmentAccessAsync.
        }

        private async Task AddDepartmentAccessAsync()
        {
            // Реализация добавления доступа к отделу.
            // Требуется выбор отдела (например, через диалог или отдельный ComboBox).
            // Предположим, что отдел выбран в переменной selectedDepartmentId.
            // string selectedDepartmentId = ... ; // Необходимо реализовать выбор отдела

            // Пример (временно):
            if (SelectedUser == null) return;
            string selectedDepartmentId = ""; // <-- Заменить на реальный выбор отдела, например, через диалог
            if (string.IsNullOrEmpty(selectedDepartmentId)) return; // Проверить выбор

            try
            {
                // Проверить, не существует ли уже доступа к этому отделу
                var existingAccess = UserDepartmentAccesses.FirstOrDefault(a => a.DepartmentId == selectedDepartmentId);
                if (existingAccess != null)
                {
                    _messageService.ShowWarningMessage("Доступ к этому отделу уже предоставлен.");
                    return;
                }

                var newAccess = new UserDepartmentAccess
                {
                    UserId = SelectedUser.Id,
                    DepartmentId = selectedDepartmentId,
                    HasAllWarehouses = false // По умолчанию - нет доступа ко всем складам
                };

                await _userDeptAccessRepository.AddAsync(newAccess);
                await _userDeptAccessRepository.SaveChangesAsync();

                UserDepartmentAccesses.Add(newAccess);
                _messageService.ShowInfoMessage("Доступ к отделу добавлен.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка добавления доступа к отделу: {ex.Message}", "Ошибка");
            }
        }

        private async Task RemoveDepartmentAccessAsync()
        {
            if (SelectedUserDeptAccess == null || SelectedUser == null) return;

            if (!_messageService.ShowConfirmationDialog($"Вы действительно хотите удалить доступ пользователя '{SelectedUser.FullName}' к отделу?")) return;

            try
            {
                // Удалить связанные доступы к складам
                var warehouseAccessesToRemove = await _userWarehouseAccessRepository.GetByUserDepartmentAccessKeyAsync(SelectedUserDeptAccess.Key);
                foreach (var wa in warehouseAccessesToRemove)
                {
                    _userWarehouseAccessRepository.Delete(wa);
                }
                await _userWarehouseAccessRepository.SaveChangesAsync();

                // Удалить доступ к отделу
                _userDeptAccessRepository.Delete(SelectedUserDeptAccess);
                await _userDeptAccessRepository.SaveChangesAsync();

                UserDepartmentAccesses.Remove(SelectedUserDeptAccess);
                _messageService.ShowInfoMessage("Доступ к отделу удален.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка удаления доступа к отделу: {ex.Message}", "Ошибка");
            }
        }

        private async Task ToggleAllWarehousesAsync()
        {
            if (SelectedUserDeptAccess == null) return;

            try
            {
                // Переключить флаг в модели
                SelectedUserDeptAccess.HasAllWarehouses = !SelectedUserDeptAccess.HasAllWarehouses;
                _editHasAllWarehouses = SelectedUserDeptAccess.HasAllWarehouses; // Обновить свойство VM
                OnPropertyChanged(nameof(EditHasAllWarehouses));

                // Удалить или оставить доступы к конкретным складам
                if (SelectedUserDeptAccess.HasAllWarehouses)
                {
                    // Удалить все конкретные доступы к складам для этого доступа к отделу
                    var warehouseAccessesToRemove = await _userWarehouseAccessRepository.GetByUserDepartmentAccessKeyAsync(SelectedUserDeptAccess.Key);
                    foreach (var wa in warehouseAccessesToRemove)
                    {
                        _userWarehouseAccessRepository.Delete(wa);
                    }
                    UserAllowedWarehouses.Clear(); // Очистить список в UI
                }
                // Если сняли галочку, доступы к складам остаются или добавляются вручную

                // Сохранить изменения
                _userDeptAccessRepository.Update(SelectedUserDeptAccess);
                await _userDeptAccessRepository.SaveChangesAsync();

                _messageService.ShowInfoMessage(SelectedUserDeptAccess.HasAllWarehouses ? "Теперь доступ ко всем складам отдела." : "Доступ ко всем складам снят. Можно настроить доступ к отдельным складам.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка изменения доступа ко складам: {ex.Message}", "Ошибка");
            }
        }

        private async Task LoadAvailableWarehousesForDepartmentAsync()
        {
            if (SelectedUserDeptAccess == null) return;

            try
            {
                var allWhForDept = await _warehouseRepository.GetByDepartmentAsync(SelectedUserDeptAccess.DepartmentId);
                AvailableWarehousesForSelection.Clear();
                foreach (var wh in allWhForDept)
                {
                    AvailableWarehousesForSelection.Add(wh);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки складов отдела: {ex.Message}", "Ошибка");
            }
        }

        private async Task LoadUserAllowedWarehousesForAccessAsync(UserDepartmentAccess access)
        {
            try
            {
                if (access.HasAllWarehouses)
                {
                    // Если доступ ко всем, показываем все склады отдела
                    var allWhForDept = await _warehouseRepository.GetByDepartmentAsync(access.DepartmentId);
                    UserAllowedWarehouses.Clear();
                    foreach (var wh in allWhForDept)
                    {
                        UserAllowedWarehouses.Add(wh);
                    }
                }
                else
                {
                    // Иначе загружаем только те, что указаны в UserWarehouseAccess
                    var warehouseAccessKeys = new List<int> { access.Key };
                    var allowedWarehouseIds = await _userWarehouseAccessRepository.GetWarehouseIdsByUserDeptAccessKeyAsync(access.Key);
                    if (allowedWarehouseIds != null && allowedWarehouseIds.Any())
                    {
                        UserAllowedWarehouses.Clear();
                        foreach (var id in allowedWarehouseIds)
                        {
                            var wh = await _warehouseRepository.GetByIdStringAsync(id);
                            if (wh != null) UserAllowedWarehouses.Add(wh);
                        }
                    }
                    else
                    {
                        UserAllowedWarehouses.Clear(); // Нет разрешенных складов
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки разрешенных складов: {ex.Message}", "Ошибка");
            }
        }

        private bool CanAddWarehouseAccess()
        {
            return SelectedUserDeptAccess != null && SelectedAvailableWarehouse != null && !SelectedUserDeptAccess.HasAllWarehouses;
        }

        private async Task AddWarehouseAccessAsync()
        {
            if (SelectedUserDeptAccess == null || SelectedAvailableWarehouse == null) return;

            try
            {
                // Проверить, не существует ли уже доступа к этому складу через этот доступ к отделу
                var existingAccess = await _userWarehouseAccessRepository.GetWarehouseIdsByUserDeptAccessKeyAsync(SelectedUserDeptAccess.Key);
                if (existingAccess.Contains(SelectedAvailableWarehouse.Id))
                {
                    _messageService.ShowWarningMessage("Доступ к этому складу уже предоставлен через этот доступ к отделу.");
                    return;
                }

                var newWarehouseAccess = new UserWarehouseAccess
                {
                    UserDepartmentAccessKey = SelectedUserDeptAccess.Key,
                    WarehouseId = SelectedAvailableWarehouse.Id
                };

                await _userWarehouseAccessRepository.AddAsync(newWarehouseAccess);
                await _userWarehouseAccessRepository.SaveChangesAsync();

                UserAllowedWarehouses.Add(SelectedAvailableWarehouse); // Обновить UI
                _messageService.ShowInfoMessage("Доступ к складу добавлен.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка добавления доступа к складу: {ex.Message}", "Ошибка");
            }
        }

        private async Task RemoveWarehouseAccessAsync()
        {
            if (SelectedUserDeptAccess == null || SelectedAllowedWarehouse == null) return;

            if (!_messageService.ShowConfirmationDialog($"Вы действительно хотите удалить доступ к складу '{SelectedAllowedWarehouse.Name}'?")) return;

            try
            {
                // Найти и удалить конкретный доступ к складу
                var warehouseAccesses = await _userWarehouseAccessRepository.GetByUserDepartmentAccessKeyAsync(SelectedUserDeptAccess.Key);
                var accessToDelete = warehouseAccesses.FirstOrDefault(wa => wa.WarehouseId == SelectedAllowedWarehouse.Id);

                if (accessToDelete != null)
                {
                    _userWarehouseAccessRepository.Delete(accessToDelete);
                    await _userWarehouseAccessRepository.SaveChangesAsync();

                    UserAllowedWarehouses.Remove(SelectedAllowedWarehouse); // Обновить UI
                    _messageService.ShowInfoMessage("Доступ к складу удален.", "Успех");
                }
                else
                {
                    _messageService.ShowWarningMessage("Доступ к складу не найден для удаления.");
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка удаления доступа к складу: {ex.Message}", "Ошибка");
            }
        }
    }
}