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
    /// ViewModel для редактирования прав доступа пользователей к отделам и складам
    /// </summary>
    public class EditAccessRightsViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IGenericRepository<UserDepartmentAccess> _userDeptAccessRepository;
        private readonly IGenericRepository<UserWarehouseAccess> _userWarehouseAccessRepository;

        private ObservableCollection<User> _users;
        private ObservableCollection<Department> _departments;
        private ObservableCollection<Warehouse> _warehouses;
        private User? _selectedUser;
        private Department? _selectedDepartment;
        private Warehouse? _selectedWarehouse;
        private bool _userHasAllDepartments;
        private bool _deptHasAllWarehouses;

        public EditAccessRightsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
            _departmentRepository = serviceProvider.GetRequiredService<IDepartmentRepository>();
            _warehouseRepository = serviceProvider.GetRequiredService<IGenericRepository<Warehouse>>();
            _userDeptAccessRepository = serviceProvider.GetRequiredService<IGenericRepository<UserDepartmentAccess>>();
            _userWarehouseAccessRepository = serviceProvider.GetRequiredService<IGenericRepository<UserWarehouseAccess>>();

            Users = new ObservableCollection<User>();
            Departments = new ObservableCollection<Department>();
            Warehouses = new ObservableCollection<Warehouse>();

            // Команды
            LoadDataCommand = new RelayCommand(LoadData);
            UserSelectionChangedCommand = new RelayCommand(UserSelectionChanged);
            DepartmentSelectionChangedCommand = new RelayCommand(DepartmentSelectionChanged);
            ToggleUserAllDepartmentsCommand = new RelayCommand(ToggleUserAllDepartments);
            ToggleDeptAllWarehousesCommand = new RelayCommand(ToggleDeptAllWarehouses);
            AddDepartmentAccessCommand = new RelayCommand(AddDepartmentAccess, CanAddDepartmentAccess);
            RemoveDepartmentAccessCommand = new RelayCommand(RemoveDepartmentAccess, CanRemoveDepartmentAccess);
            AddWarehouseAccessCommand = new RelayCommand(AddWarehouseAccess, CanAddWarehouseAccess);
            RemoveWarehouseAccessCommand = new RelayCommand(RemoveWarehouseAccess, CanRemoveWarehouseAccess);

            // Загрузка данных
            _ = LoadDataAsync();
        }

        // ========== Свойства ==========

        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
        }

        public Warehouse? SelectedWarehouse
        {
            get => _selectedWarehouse;
            set => SetProperty(ref _selectedWarehouse, value);
        }

        public bool UserHasAllDepartments
        {
            get => _userHasAllDepartments;
            set => SetProperty(ref _userHasAllDepartments, value);
        }

        public bool DeptHasAllWarehouses
        {
            get => _deptHasAllWarehouses;
            set => SetProperty(ref _deptHasAllWarehouses, value);
        }

        // ========== Команды ==========

        public ICommand LoadDataCommand { get; }
        public ICommand UserSelectionChangedCommand { get; }
        public ICommand DepartmentSelectionChangedCommand { get; }
        public ICommand ToggleUserAllDepartmentsCommand { get; }
        public ICommand ToggleDeptAllWarehousesCommand { get; }
        public ICommand AddDepartmentAccessCommand { get; }
        public ICommand RemoveDepartmentAccessCommand { get; }
        public ICommand AddWarehouseAccessCommand { get; }
        public ICommand RemoveWarehouseAccessCommand { get; }

        // ========== Методы ==========

        /// <summary>
        /// Загружает все данные
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                await LoadUsersAsync();
                await LoadDepartmentsAsync();
                await LoadWarehousesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            _ = LoadDataAsync();
        }

        /// <summary>
        /// Загружает пользователей
        /// </summary>
        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetActiveUsersAsync();
                Users = new ObservableCollection<User>(users);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает отделы
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
        /// Загружает склады
        /// </summary>
        private async Task LoadWarehousesAsync()
        {
            try
            {
                var warehouses = await _warehouseRepository.FindAsync(w => w.IsActive);
                Warehouses = new ObservableCollection<Warehouse>(warehouses);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки складов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик изменения выбранного пользователя
        /// </summary>
        private async void UserSelectionChanged()
        {
            if (SelectedUser == null)
                return;

            try
            {
                // Загружаем доступы пользователя к отделам
                var deptAccess = await _userDeptAccessRepository.FindAsync(a => a.UserId == SelectedUser.Id);

                // Обновляем информацию о доступах
                SelectedUser.HasAllDepartments = deptAccess.Any(a => a.HasAllWarehouses); // Упрощенно
                UserHasAllDepartments = SelectedUser.HasAllDepartments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки доступов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик изменения выбранного отдела
        /// </summary>
        private async void DepartmentSelectionChanged()
        {
            if (SelectedUser == null || SelectedDepartment == null)
                return;

            try
            {
                // Проверяем, есть ли у пользователя доступ к этому отделу
                var deptAccess = await _userDeptAccessRepository.FindAsync(a =>
                    a.UserId == SelectedUser.Id && a.DepartmentId == SelectedDepartment.Id);

                if (deptAccess.Any())
                {
                    DeptHasAllWarehouses = deptAccess.First().HasAllWarehouses;
                }
                else
                {
                    DeptHasAllWarehouses = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки доступов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Переключает доступ пользователя ко всем отделам
        /// </summary>
        private async void ToggleUserAllDepartments()
        {
            if (SelectedUser == null)
                return;

            try
            {
                SelectedUser.HasAllDepartments = UserHasAllDepartments;
                _userRepository.Update(SelectedUser);

                MessageBox.Show("Доступ ко всем отделам обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления доступа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Переключает доступ к всем складам отдела
        /// </summary>
        private async void ToggleDeptAllWarehouses()
        {
            if (SelectedUser == null || SelectedDepartment == null)
                return;

            try
            {
                // Находим или создаем запись доступа
                var deptAccess = await _userDeptAccessRepository.FindAsync(a =>
                    a.UserId == SelectedUser.Id && a.DepartmentId == SelectedDepartment.Id);

                if (deptAccess.Any())
                {
                    var access = deptAccess.First();
                    access.HasAllWarehouses = DeptHasAllWarehouses;
                    _userDeptAccessRepository.Update(access);
                }
                else
                {
                    var newAccess = new UserDepartmentAccess
                    {
                        UserId = SelectedUser.Id,
                        DepartmentId = SelectedDepartment.Id,
                        HasAllWarehouses = DeptHasAllWarehouses,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _userDeptAccessRepository.AddAsync(newAccess);
                }

                MessageBox.Show("Доступ к складам обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления доступа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавляет доступ пользователя к отделу
        /// </summary>
        private async void AddDepartmentAccess()
        {
            if (SelectedUser == null || SelectedDepartment == null)
                return;

            try
            {
                // Проверяем, не существует ли уже такой доступ
                var existing = await _userDeptAccessRepository.FindAsync(a =>
                    a.UserId == SelectedUser.Id && a.DepartmentId == SelectedDepartment.Id);

                if (existing.Any())
                {
                    MessageBox.Show("Доступ к этому отделу уже существует", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var newAccess = new UserDepartmentAccess
                {
                    UserId = SelectedUser.Id,
                    DepartmentId = SelectedDepartment.Id,
                    HasAllWarehouses = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _userDeptAccessRepository.AddAsync(newAccess);
                MessageBox.Show("Доступ к отделу добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления доступа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddDepartmentAccess()
        {
            return SelectedUser != null && SelectedDepartment != null;
        }

        /// <summary>
        /// Удаляет доступ пользователя к отделу
        /// </summary>
        private async void RemoveDepartmentAccess()
        {
            if (SelectedUser == null || SelectedDepartment == null)
                return;

            try
            {
                var deptAccess = await _userDeptAccessRepository.FindAsync(a =>
                    a.UserId == SelectedUser.Id && a.DepartmentId == SelectedDepartment.Id);

                if (!deptAccess.Any())
                {
                    MessageBox.Show("Доступ к этому отделу не найден", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить доступ пользователя '{SelectedUser.FullName}' к отделу '{SelectedDepartment.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _userDeptAccessRepository.RemoveAsync(deptAccess.First());
                    MessageBox.Show("Доступ к отделу удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления доступа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanRemoveDepartmentAccess()
        {
            return SelectedUser != null && SelectedDepartment != null;
        }

        /// <summary>
        /// Добавляет доступ пользователя к складу
        /// </summary>
        private async void AddWarehouseAccess()
        {
            if (SelectedUser == null || SelectedDepartment == null || SelectedWarehouse == null)
                return;

            try
            {
                // Проверяем, есть ли доступ к отделу
                var deptAccess = await _userDeptAccessRepository.FindAsync(a =>
                    a.UserId == SelectedUser.Id && a.DepartmentId == SelectedDepartment.Id);

                if (!deptAccess.Any())
                {
                    MessageBox.Show("Сначала добавьте доступ пользователя к отделу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, не существует ли уже такой доступ
                var existing = await _userWarehouseAccessRepository.FindAsync(a =>
                    a.UserDepartmentAccessKey == deptAccess.First().Key &&
                    a.WarehouseId == SelectedWarehouse.Id);

                if (existing.Any())
                {
                    MessageBox.Show("Доступ к этому складу уже существует", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var newAccess = new UserWarehouseAccess
                {
                    UserDepartmentAccessKey = deptAccess.First().Key,
                    WarehouseId = SelectedWarehouse.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _userWarehouseAccessRepository.AddAsync(newAccess);
                MessageBox.Show("Доступ к складу добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления доступа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddWarehouseAccess()
        {
            return SelectedUser != null && SelectedDepartment != null && SelectedWarehouse != null;
        }

        /// <summary>
        /// Удаляет доступ пользователя к складу
        /// </summary>
        private async void RemoveWarehouseAccess()
        {
            if (SelectedUser == null || SelectedDepartment == null || SelectedWarehouse == null)
                return;

            try
            {
                var deptAccess = await _userDeptAccessRepository.FindAsync(a =>
                    a.UserId == SelectedUser.Id && a.DepartmentId == SelectedDepartment.Id);

                if (!deptAccess.Any())
                {
                    MessageBox.Show("Доступ к отделу не найден", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var warehouseAccess = await _userWarehouseAccessRepository.FindAsync(a =>
                    a.UserDepartmentAccessKey == deptAccess.First().Key &&
                    a.WarehouseId == SelectedWarehouse.Id);

                if (!warehouseAccess.Any())
                {
                    MessageBox.Show("Доступ к этому складу не найден", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить доступ пользователя '{SelectedUser.FullName}' к складу '{SelectedWarehouse.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _userWarehouseAccessRepository.RemoveAsync(warehouseAccess.First());
                    MessageBox.Show("Доступ к складу удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления доступа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanRemoveWarehouseAccess()
        {
            return SelectedUser != null && SelectedDepartment != null && SelectedWarehouse != null;
        }
    }
}