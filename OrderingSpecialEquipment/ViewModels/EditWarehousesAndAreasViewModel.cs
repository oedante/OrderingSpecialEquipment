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
    /// ViewModel для окна редактирования складов и территорий складов.
    /// </summary>
    public class EditWarehousesAndAreasViewModel : ViewModelBase
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IWarehouseAreaRepository _warehouseAreaRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        // Для складов
        private ObservableCollection<Warehouse> _warehouses;
        private Warehouse? _selectedWarehouse;
        private bool _isEditingWarehouse = false;
        private string _editWarehouseId = string.Empty;
        private string _editWarehouseName = string.Empty;
        private string _editWarehouseDepartmentId = string.Empty;
        private string _editWarehouseAddress = string.Empty;
        private bool _editWarehouseIsActive = true;

        // Для территорий
        private ObservableCollection<WarehouseArea> _warehouseAreas;
        private WarehouseArea? _selectedArea;
        private bool _isEditingArea = false;
        private string _editAreaId = string.Empty;
        private string _editAreaName = string.Empty;
        private string _editAreaWarehouseId = string.Empty; // Привязка к складу
        private string? _editAreaType;
        private int? _editAreaMaxCapacity;
        private bool _editAreaIsActive = true;

        // Для выбора в ComboBox
        private ObservableCollection<Department> _departmentsForSelection;

        public EditWarehousesAndAreasViewModel(
            IWarehouseRepository warehouseRepository,
            IWarehouseAreaRepository warehouseAreaRepository,
            IDepartmentRepository departmentRepository,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _warehouseRepository = warehouseRepository;
            _warehouseAreaRepository = warehouseAreaRepository;
            _departmentRepository = departmentRepository;
            _messageService = messageService;
            _authorizationService = authorizationService;

            _warehouses = new ObservableCollection<Warehouse>();
            _warehouseAreas = new ObservableCollection<WarehouseArea>();
            _departmentsForSelection = new ObservableCollection<Department>();

            LoadWarehousesCommand = new RelayCommand(async _ => await LoadWarehousesAsync(), _ => _authorizationService.CanReadTable("Warehouses"));
            SaveWarehouseCommand = new RelayCommand(async _ => await SaveWarehouseAsync(), _ => CanSaveWarehouse());
            DeleteWarehouseCommand = new RelayCommand(async _ => await DeleteWarehouseAsync(), _ => CanDeleteWarehouse());
            CancelEditWarehouseCommand = new RelayCommand(_ => CancelEditWarehouse());

            LoadAreasCommand = new RelayCommand(async _ => await LoadAreasAsync(), _ => _authorizationService.CanReadTable("WarehouseAreas"));
            SaveAreaCommand = new RelayCommand(async _ => await SaveAreaAsync(), _ => CanSaveArea());
            DeleteAreaCommand = new RelayCommand(async _ => await DeleteAreaAsync(), _ => CanDeleteArea());
            CancelEditAreaCommand = new RelayCommand(_ => CancelEditArea());

            // Загрузка зависимых данных
            Task.Run(async () => await LoadDepartmentsForSelectionAsync());

            // Инициализация данных
            Task.Run(async () => await LoadWarehousesAsync());
        }

        // --- Свойства для складов ---
        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
        }

        public Warehouse? SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                if (SetProperty(ref _selectedWarehouse, value))
                {
                    if (value != null)
                    {
                        _editWarehouseId = value.Id;
                        _editWarehouseName = value.Name;
                        _editWarehouseDepartmentId = value.DepartmentId;
                        _editWarehouseAddress = value.Address ?? string.Empty;
                        _editWarehouseIsActive = value.IsActive;
                        _isEditingWarehouse = true;
                        // После выбора склада, загружаем его территории
                        Task.Run(async () => await LoadAreasForWarehouseAsync(value.Id));
                    }
                    else
                    {
                        ResetEditWarehouseFields();
                        WarehouseAreas.Clear(); // Очистить список территорий
                    }
                    OnPropertyChanged(nameof(EditWarehouseId));
                    OnPropertyChanged(nameof(EditWarehouseName));
                    OnPropertyChanged(nameof(EditWarehouseDepartmentId));
                    OnPropertyChanged(nameof(EditWarehouseAddress));
                    OnPropertyChanged(nameof(EditWarehouseIsActive));
                    ((RelayCommand)SaveWarehouseCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteWarehouseCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditWarehouseId
        {
            get => _editWarehouseId;
            set
            {
                if (SetProperty(ref _editWarehouseId, value))
                {
                    ((RelayCommand)SaveWarehouseCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditWarehouseName
        {
            get => _editWarehouseName;
            set => SetProperty(ref _editWarehouseName, value);
        }

        public string EditWarehouseDepartmentId
        {
            get => _editWarehouseDepartmentId;
            set
            {
                if (SetProperty(ref _editWarehouseDepartmentId, value))
                {
                    // При изменении склада, обновляем список территорий
                    if (SelectedWarehouse != null && SelectedWarehouse.Id == _editWarehouseId)
                    {
                        Task.Run(async () => await LoadAreasForWarehouseAsync(value));
                    }
                }
            }
        }

        public string EditWarehouseAddress
        {
            get => _editWarehouseAddress;
            set => SetProperty(ref _editWarehouseAddress, value);
        }

        public bool EditWarehouseIsActive
        {
            get => _editWarehouseIsActive;
            set => SetProperty(ref _editWarehouseIsActive, value);
        }

        // --- Свойства для территорий ---
        public ObservableCollection<WarehouseArea> WarehouseAreas
        {
            get => _warehouseAreas;
            set => SetProperty(ref _warehouseAreas, value);
        }

        public WarehouseArea? SelectedArea
        {
            get => _selectedArea;
            set
            {
                if (SetProperty(ref _selectedArea, value))
                {
                    if (value != null)
                    {
                        _editAreaId = value.Id;
                        _editAreaName = value.Name;
                        _editAreaWarehouseId = value.WarehouseId; // Привязка к складу
                        _editAreaType = value.AreaType;
                        _editAreaMaxCapacity = value.MaxCapacity;
                        _editAreaIsActive = value.IsActive;
                        _isEditingArea = true;
                    }
                    else
                    {
                        ResetEditAreaFields();
                    }
                    OnPropertyChanged(nameof(EditAreaId));
                    OnPropertyChanged(nameof(EditAreaName));
                    OnPropertyChanged(nameof(EditAreaWarehouseId));
                    OnPropertyChanged(nameof(EditAreaType));
                    OnPropertyChanged(nameof(EditAreaMaxCapacity));
                    OnPropertyChanged(nameof(EditAreaIsActive));
                    ((RelayCommand)SaveAreaCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteAreaCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditAreaId
        {
            get => _editAreaId;
            set
            {
                if (SetProperty(ref _editAreaId, value))
                {
                    ((RelayCommand)SaveAreaCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditAreaName
        {
            get => _editAreaName;
            set => SetProperty(ref _editAreaName, value);
        }

        public string EditAreaWarehouseId
        {
            get => _editAreaWarehouseId;
            set => SetProperty(ref _editAreaWarehouseId, value);
        }

        public string? EditAreaType
        {
            get => _editAreaType;
            set => SetProperty(ref _editAreaType, value);
        }

        public int? EditAreaMaxCapacity
        {
            get => _editAreaMaxCapacity;
            set => SetProperty(ref _editAreaMaxCapacity, value);
        }

        public bool EditAreaIsActive
        {
            get => _editAreaIsActive;
            set => SetProperty(ref _editAreaIsActive, value);
        }

        // --- Свойства для выбора ---
        public ObservableCollection<Department> DepartmentsForSelection
        {
            get => _departmentsForSelection;
            set => SetProperty(ref _departmentsForSelection, value);
        }

        // --- Команды для складов ---
        public ICommand LoadWarehousesCommand { get; }
        public ICommand SaveWarehouseCommand { get; }
        public ICommand DeleteWarehouseCommand { get; }
        public ICommand CancelEditWarehouseCommand { get; }

        // --- Команды для territorий ---
        public ICommand LoadAreasCommand { get; }
        public ICommand SaveAreaCommand { get; }
        public ICommand DeleteAreaCommand { get; }
        public ICommand CancelEditAreaCommand { get; }

        // --- Методы для складов ---
        private async Task LoadWarehousesAsync()
        {
            try
            {
                var dbWarehouses = await _warehouseRepository.GetAllAsync();
                Warehouses.Clear();
                foreach (var wh in dbWarehouses)
                {
                    // Фильтрация по IsActive и правам доступа к отделу
                    if (wh.IsActive && _authorizationService.CanAccessDepartment(wh.DepartmentId))
                    {
                        Warehouses.Add(wh);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки складов: {ex.Message}", "Ошибка");
            }
        }

        private bool CanSaveWarehouse()
        {
            return _authorizationService.CanWriteTable("Warehouses") &&
                   !string.IsNullOrWhiteSpace(EditWarehouseId) &&
                   !string.IsNullOrWhiteSpace(EditWarehouseName) &&
                   !string.IsNullOrWhiteSpace(EditWarehouseDepartmentId) &&
                   (!_isEditingWarehouse || SelectedWarehouse?.Id == EditWarehouseId);
        }

        private async Task SaveWarehouseAsync()
        {
            if (!CanSaveWarehouse()) return;

            try
            {
                Warehouse warehouse;
                bool isNew = !_isEditingWarehouse;

                if (isNew)
                {
                    if (await _warehouseRepository.ExistsAsync(EditWarehouseId))
                    {
                        _messageService.ShowErrorMessage($"Склад с ID '{EditWarehouseId}' уже существует.", "Ошибка");
                        return;
                    }
                    warehouse = new Warehouse
                    {
                        Id = EditWarehouseId,
                        Name = EditWarehouseName,
                        DepartmentId = EditWarehouseDepartmentId,
                        Address = string.IsNullOrWhiteSpace(EditWarehouseAddress) ? null : EditWarehouseAddress,
                        IsActive = EditWarehouseIsActive
                    };
                    await _warehouseRepository.AddAsync(warehouse);
                }
                else
                {
                    warehouse = SelectedWarehouse!;
                    warehouse.Name = EditWarehouseName;
                    warehouse.DepartmentId = EditWarehouseDepartmentId;
                    warehouse.Address = string.IsNullOrWhiteSpace(EditWarehouseAddress) ? null : EditWarehouseAddress;
                    warehouse.IsActive = EditWarehouseIsActive;
                    _warehouseRepository.Update(warehouse);
                }

                await _warehouseRepository.SaveChangesAsync();

                if (isNew)
                {
                    // Добавляем в список, если активен и пользователь имеет доступ к отделу
                    if (warehouse.IsActive && _authorizationService.CanAccessDepartment(warehouse.DepartmentId))
                    {
                        Warehouses.Add(warehouse);
                    }
                }
                else
                {
                    await LoadWarehousesAsync(); // Перезагрузка для обновления списка
                }

                ResetEditWarehouseFields();
                _messageService.ShowInfoMessage(isNew ? "Склад добавлен." : "Склад обновлен.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка сохранения склада: {ex.Message}", "Ошибка");
            }
        }

        private bool CanDeleteWarehouse()
        {
            return _authorizationService.CanWriteTable("Warehouses") &&
                   SelectedWarehouse != null && SelectedWarehouse.Key != 0;
        }

        private async Task DeleteWarehouseAsync()
        {
            if (!CanDeleteWarehouse() || SelectedWarehouse == null) return;

            if (!_messageService.ShowConfirmationDialog($"Вы действительно хотите удалить склад '{SelectedWarehouse.Name}'?")) return;

            try
            {
                _warehouseRepository.Delete(SelectedWarehouse);
                await _warehouseRepository.SaveChangesAsync();
                Warehouses.Remove(SelectedWarehouse);
                // SelectedWarehouse = null; // Устанавливается через сеттер
                ResetEditWarehouseFields();
                _messageService.ShowInfoMessage("Склад удален.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка удаления склада: {ex.Message}", "Ошибка");
            }
        }

        private void CancelEditWarehouse()
        {
            ResetEditWarehouseFields();
        }

        private void ResetEditWarehouseFields()
        {
            _isEditingWarehouse = false;
            _editWarehouseId = string.Empty;
            _editWarehouseName = string.Empty;
            _editWarehouseDepartmentId = string.Empty; // Сбросить на значение по умолчанию или оставить как есть?
            _editWarehouseAddress = string.Empty;
            _editWarehouseIsActive = true;
            SelectedWarehouse = null; // Сбросить выбор
            OnPropertyChanged(nameof(EditWarehouseId));
            OnPropertyChanged(nameof(EditWarehouseName));
            OnPropertyChanged(nameof(EditWarehouseDepartmentId));
            OnPropertyChanged(nameof(EditWarehouseAddress));
            OnPropertyChanged(nameof(EditWarehouseIsActive));
            ((RelayCommand)SaveWarehouseCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteWarehouseCommand).RaiseCanExecuteChanged();
        }

        // --- Методы для территорий ---
        private async Task LoadAreasAsync()
        {
            // Этот метод загружает *все* территории или может быть ограничен складом
            // Основной загрузчик - LoadAreasForWarehouseAsync
        }

        private async Task LoadAreasForWarehouseAsync(string warehouseId)
        {
            try
            {
                var dbAreas = await _warehouseAreaRepository.GetByWarehouseAsync(warehouseId);
                WarehouseAreas.Clear();
                foreach (var area in dbAreas)
                {
                    // Фильтрация по IsActive
                    if (area.IsActive) // <-- Добавлено
                    {
                        WarehouseAreas.Add(area);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки территорий для склада: {ex.Message}", "Ошибка");
            }
        }

        private bool CanSaveArea()
        {
            return _authorizationService.CanWriteTable("WarehouseAreas") &&
                   !string.IsNullOrWhiteSpace(EditAreaId) &&
                   !string.IsNullOrWhiteSpace(EditAreaName) &&
                   !string.IsNullOrWhiteSpace(EditAreaWarehouseId) && // Привязка к складу обязательна
                   (!_isEditingArea || SelectedArea?.Id == EditAreaId);
        }

        private async Task SaveAreaAsync()
        {
            if (!CanSaveArea()) return;

            try
            {
                WarehouseArea area;
                bool isNew = !_isEditingArea;

                if (isNew)
                {
                    if (await _warehouseAreaRepository.ExistsAsync(EditAreaId))
                    {
                        _messageService.ShowErrorMessage($"Территория с ID '{EditAreaId}' уже существует.", "Ошибка");
                        return;
                    }
                    area = new WarehouseArea
                    {
                        Id = EditAreaId,
                        Name = EditAreaName,
                        WarehouseId = EditAreaWarehouseId, // Привязка к складу
                        AreaType = EditAreaType,
                        MaxCapacity = EditAreaMaxCapacity,
                        IsActive = EditAreaIsActive
                    };
                    await _warehouseAreaRepository.AddAsync(area);
                }
                else
                {
                    area = SelectedArea!;
                    area.Name = EditAreaName;
                    area.WarehouseId = EditAreaWarehouseId; // Привязка к складу может измениться
                    area.AreaType = EditAreaType;
                    area.MaxCapacity = EditAreaMaxCapacity;
                    area.IsActive = EditAreaIsActive;
                    _warehouseAreaRepository.Update(area);
                }

                await _warehouseAreaRepository.SaveChangesAsync();

                if (isNew)
                {
                    // Добавляем в список, если активна
                    if (area.IsActive) // <-- Добавлено
                    {
                        // Загружаем территории для выбранного склада, если он текущий
                        if (SelectedWarehouse != null && area.WarehouseId == SelectedWarehouse.Id)
                        {
                            await LoadAreasForWarehouseAsync(area.WarehouseId);
                        }
                    }
                }
                else
                {
                    await LoadAreasForWarehouseAsync(area.WarehouseId); // Перезагрузка списка для выбранного склада
                }

                ResetEditAreaFields();
                _messageService.ShowInfoMessage(isNew ? "Территория добавлена." : "Территория обновлена.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка сохранения территории: {ex.Message}", "Ошибка");
            }
        }

        private bool CanDeleteArea()
        {
            return _authorizationService.CanWriteTable("WarehouseAreas") &&
                   SelectedArea != null && SelectedArea.Key != 0;
        }

        private async Task DeleteAreaAsync()
        {
            if (!CanDeleteArea() || SelectedArea == null) return;

            if (!_messageService.ShowConfirmationDialog($"Вы действительно хотите удалить территорию '{SelectedArea.Name}'?")) return;

            try
            {
                _warehouseAreaRepository.Delete(SelectedArea);
                await _warehouseAreaRepository.SaveChangesAsync();
                // Обновляем список территорий для текущего склада
                if (SelectedWarehouse != null && SelectedArea.WarehouseId == SelectedWarehouse.Id)
                {
                    await LoadAreasForWarehouseAsync(SelectedArea.WarehouseId);
                }
                ResetEditAreaFields();
                _messageService.ShowInfoMessage("Территория удалена.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка удаления территории: {ex.Message}", "Ошибка");
            }
        }

        private void CancelEditArea()
        {
            ResetEditAreaFields();
        }

        private void ResetEditAreaFields()
        {
            _isEditingArea = false;
            _editAreaId = string.Empty;
            _editAreaName = string.Empty;
            _editAreaWarehouseId = string.Empty; // Сбросить на значение выбранного склада или оставить пустым?
            _editAreaType = null;
            _editAreaMaxCapacity = null;
            _editAreaIsActive = true;
            SelectedArea = null; // Сбросить выбор
            OnPropertyChanged(nameof(EditAreaId));
            OnPropertyChanged(nameof(EditAreaName));
            OnPropertyChanged(nameof(EditAreaWarehouseId));
            OnPropertyChanged(nameof(EditAreaType));
            OnPropertyChanged(nameof(EditAreaMaxCapacity));
            OnPropertyChanged(nameof(EditAreaIsActive));
            ((RelayCommand)SaveAreaCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteAreaCommand).RaiseCanExecuteChanged();
        }

        // --- Вспомогательные методы ---
        private async Task LoadDepartmentsForSelectionAsync()
        {
            try
            {
                var dbDepts = await _departmentRepository.GetAllAsync();
                DepartmentsForSelection.Clear();
                foreach (var dept in dbDepts)
                {
                    // Фильтрация по IsActive и правам доступа
                    if (dept.IsActive && _authorizationService.CanAccessDepartment(dept.Id)) // <-- Изменено
                    {
                        DepartmentsForSelection.Add(dept);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки отделов для выбора: {ex.Message}", "Ошибка");
            }
        }
    }
}