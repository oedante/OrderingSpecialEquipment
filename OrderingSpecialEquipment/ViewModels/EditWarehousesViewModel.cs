using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Models;
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
    /// ViewModel для редактирования складов и территорий
    /// </summary>
    public class EditWarehousesViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IGenericRepository<WarehouseArea> _warehouseAreaRepository;
        private readonly IDepartmentRepository _departmentRepository;

        private ObservableCollection<Warehouse> _warehouses;
        private ObservableCollection<WarehouseArea> _warehouseAreas;
        private Warehouse? _selectedWarehouse;
        private WarehouseArea? _selectedWarehouseArea;
        private bool _isWarehouseEditMode;
        private bool _isAreaEditMode;
        private string _searchText;
        private ObservableCollection<Department> _departments;

        public EditWarehousesViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _warehouseRepository = serviceProvider.GetRequiredService<IGenericRepository<Warehouse>>();
            _warehouseAreaRepository = serviceProvider.GetRequiredService<IGenericRepository<WarehouseArea>>();
            _departmentRepository = serviceProvider.GetRequiredService<IDepartmentRepository>();

            Warehouses = new ObservableCollection<Warehouse>();
            WarehouseAreas = new ObservableCollection<WarehouseArea>();
            Departments = new ObservableCollection<Department>();
            SearchText = string.Empty;

            // Команды
            LoadWarehousesCommand = new RelayCommand(LoadWarehouses);
            AddWarehouseCommand = new RelayCommand(AddWarehouse);
            EditWarehouseCommand = new RelayCommand(EditWarehouse, CanEditWarehouse);
            DeleteWarehouseCommand = new RelayCommand(DeleteWarehouse, CanDeleteWarehouse);
            SaveWarehouseCommand = new RelayCommand(SaveWarehouse, CanSaveWarehouse);
            CancelWarehouseEditCommand = new RelayCommand(CancelWarehouseEdit);

            LoadAreasCommand = new RelayCommand(LoadAreas);
            AddAreaCommand = new RelayCommand(AddArea);
            EditAreaCommand = new RelayCommand(EditArea, CanEditArea);
            DeleteAreaCommand = new RelayCommand(DeleteArea, CanDeleteArea);
            SaveAreaCommand = new RelayCommand(SaveArea, CanSaveArea);
            CancelAreaEditCommand = new RelayCommand(CancelAreaEdit);

            SearchCommand = new RelayCommand(Search);

            // Загрузка данных
            _ = LoadDataAsync();
        }

        // ========== Свойства ==========

        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
        }

        public ObservableCollection<WarehouseArea> WarehouseAreas
        {
            get => _warehouseAreas;
            set => SetProperty(ref _warehouseAreas, value);
        }

        public Warehouse? SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                if (SetProperty(ref _selectedWarehouse, value))
                {
                    // При выборе склада загружаем его территории
                    if (value != null)
                    {
                        _ = LoadAreasForWarehouseAsync(value.Id);
                    }
                }
            }
        }

        public WarehouseArea? SelectedWarehouseArea
        {
            get => _selectedWarehouseArea;
            set => SetProperty(ref _selectedWarehouseArea, value);
        }

        public bool IsWarehouseEditMode
        {
            get => _isWarehouseEditMode;
            set => SetProperty(ref _isWarehouseEditMode, value);
        }

        public bool IsAreaEditMode
        {
            get => _isAreaEditMode;
            set => SetProperty(ref _isAreaEditMode, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        // ========== Команды ==========

        public ICommand LoadWarehousesCommand { get; }
        public ICommand AddWarehouseCommand { get; }
        public ICommand EditWarehouseCommand { get; }
        public ICommand DeleteWarehouseCommand { get; }
        public ICommand SaveWarehouseCommand { get; }
        public ICommand CancelWarehouseEditCommand { get; }

        public ICommand LoadAreasCommand { get; }
        public ICommand AddAreaCommand { get; }
        public ICommand EditAreaCommand { get; }
        public ICommand DeleteAreaCommand { get; }
        public ICommand SaveAreaCommand { get; }
        public ICommand CancelAreaEditCommand { get; }

        public ICommand SearchCommand { get; }

        // ========== Методы ==========

        /// <summary>
        /// Загружает все данные (склады, территории, отделы)
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                await LoadWarehousesAsync();
                await LoadDepartmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает все склады
        /// </summary>
        private async Task LoadWarehousesAsync()
        {
            try
            {
                var warehouses = await _warehouseRepository.GetAllAsync();
                Warehouses = new ObservableCollection<Warehouse>(warehouses);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки складов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadWarehouses()
        {
            _ = LoadWarehousesAsync();
        }

        /// <summary>
        /// Загружает территории для выбранного склада
        /// </summary>
        private async Task LoadAreasForWarehouseAsync(string warehouseId)
        {
            try
            {
                var areas = await _warehouseAreaRepository.FindAsync(a => a.WarehouseId == warehouseId);
                WarehouseAreas = new ObservableCollection<WarehouseArea>(areas);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки территорий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadAreas()
        {
            if (SelectedWarehouse != null)
            {
                await LoadAreasForWarehouseAsync(SelectedWarehouse.Id);
            }
        }

        /// <summary>
        /// Загружает все отделы для выпадающего списка
        /// </summary>
        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _departmentRepository.GetActiveDepartmentsAsync();
                Departments = new ObservableCollection<Department>(departments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавляет новый склад
        /// </summary>
        private void AddWarehouse()
        {
            SelectedWarehouse = new Warehouse
            {
                Id = GenerateNewId("WH"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            IsWarehouseEditMode = true;
        }

        /// <summary>
        /// Редактирует выбранный склад
        /// </summary>
        private void EditWarehouse()
        {
            if (SelectedWarehouse != null)
            {
                IsWarehouseEditMode = true;
            }
        }

        private bool CanEditWarehouse()
        {
            return SelectedWarehouse != null && !IsWarehouseEditMode;
        }

        /// <summary>
        /// Удаляет выбранный склад
        /// </summary>
        private async void DeleteWarehouse()
        {
            if (SelectedWarehouse == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить склад '{SelectedWarehouse.Name}' и все его территории?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _warehouseRepository.RemoveAsync(SelectedWarehouse);
                    await LoadWarehousesAsync();
                    SelectedWarehouse = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления склада: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteWarehouse()
        {
            return SelectedWarehouse != null && !IsWarehouseEditMode;
        }

        /// <summary>
        /// Сохраняет склад
        /// </summary>
        private async void SaveWarehouse()
        {
            if (SelectedWarehouse == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedWarehouse.Name))
            {
                MessageBox.Show("Введите наименование склада", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedWarehouse.DepartmentId))
            {
                MessageBox.Show("Выберите отдел", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (Warehouses.Any(w => w.Id != SelectedWarehouse.Id && w.Name == SelectedWarehouse.Name))
                {
                    MessageBox.Show("Склад с таким наименованием уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedWarehouse.Key == 0)
                {
                    // Новый склад
                    await _warehouseRepository.AddAsync(SelectedWarehouse);
                }
                else
                {
                    // Обновление существующего
                    _warehouseRepository.Update(SelectedWarehouse);
                }

                await LoadWarehousesAsync();
                IsWarehouseEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения склада: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveWarehouse()
        {
            return SelectedWarehouse != null && IsWarehouseEditMode;
        }

        /// <summary>
        /// Отменяет редактирование склада
        /// </summary>
        private void CancelWarehouseEdit()
        {
            IsWarehouseEditMode = false;
            SelectedWarehouse = null;
        }

        /// <summary>
        /// Добавляет новую территорию
        /// </summary>
        private void AddArea()
        {
            if (SelectedWarehouse == null)
            {
                MessageBox.Show("Сначала выберите склад", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedWarehouseArea = new WarehouseArea
            {
                Id = GenerateNewId("WA"),
                WarehouseId = SelectedWarehouse.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            IsAreaEditMode = true;
        }

        /// <summary>
        /// Редактирует выбранную территорию
        /// </summary>
        private void EditArea()
        {
            if (SelectedWarehouseArea != null)
            {
                IsAreaEditMode = true;
            }
        }

        private bool CanEditArea()
        {
            return SelectedWarehouseArea != null && !IsAreaEditMode;
        }

        /// <summary>
        /// Удаляет выбранную территорию
        /// </summary>
        private async void DeleteArea()
        {
            if (SelectedWarehouseArea == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить территорию '{SelectedWarehouseArea.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _warehouseAreaRepository.RemoveAsync(SelectedWarehouseArea);
                    await LoadAreasForWarehouseAsync(SelectedWarehouse!.Id);
                    SelectedWarehouseArea = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления территории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteArea()
        {
            return SelectedWarehouseArea != null && !IsAreaEditMode;
        }

        /// <summary>
        /// Сохраняет территорию
        /// </summary>
        private async void SaveArea()
        {
            if (SelectedWarehouseArea == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedWarehouseArea.Name))
            {
                MessageBox.Show("Введите наименование территории", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedWarehouseArea.WarehouseId))
            {
                MessageBox.Show("Выберите склад", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (WarehouseAreas.Any(a => a.Id != SelectedWarehouseArea.Id && a.Name == SelectedWarehouseArea.Name))
                {
                    MessageBox.Show("Территория с таким наименованием уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedWarehouseArea.Key == 0)
                {
                    // Новая территория
                    await _warehouseAreaRepository.AddAsync(SelectedWarehouseArea);
                }
                else
                {
                    // Обновление существующей
                    _warehouseAreaRepository.Update(SelectedWarehouseArea);
                }

                await LoadAreasForWarehouseAsync(SelectedWarehouse!.Id);
                IsAreaEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения территории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveArea()
        {
            return SelectedWarehouseArea != null && IsAreaEditMode;
        }

        /// <summary>
        /// Отменяет редактирование территории
        /// </summary>
        private void CancelAreaEdit()
        {
            IsAreaEditMode = false;
            SelectedWarehouseArea = null;
        }

        /// <summary>
        /// Поиск
        /// </summary>
        private async void Search()
        {
            try
            {
                var allWarehouses = await _warehouseRepository.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var filtered = allWarehouses.Where(w =>
                        w.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        w.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

                    Warehouses = new ObservableCollection<Warehouse>(filtered);
                }
                else
                {
                    await LoadWarehousesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Генерирует новый идентификатор
        /// </summary>
        private string GenerateNewId(string prefix)
        {
            try
            {
                var existingIds = Warehouses.Select(w => w.Id).Where(id => id.StartsWith(prefix));

                if (!existingIds.Any())
                {
                    return $"{prefix}000001";
                }

                var maxNumber = existingIds
                    .Select(id => int.TryParse(id.Substring(prefix.Length), out int num) ? num : 0)
                    .Max();

                return $"{prefix}{(maxNumber + 1):D6}";
            }
            catch
            {
                return $"{prefix}000001";
            }
        }
    }
}