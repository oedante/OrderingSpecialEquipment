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
    /// ViewModel для редактирования пользователей
    /// </summary>
    public class EditUsersViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IDepartmentRepository _departmentRepository;

        private ObservableCollection<User> _users;
        private User? _selectedUser;
        private bool _isEditMode;
        private string _searchText;
        private ObservableCollection<Role> _roles;
        private ObservableCollection<Department> _departments;

        public EditUsersViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
            _roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();
            _departmentRepository = serviceProvider.GetRequiredService<IDepartmentRepository>();

            Users = new ObservableCollection<User>();
            Roles = new ObservableCollection<Role>();
            Departments = new ObservableCollection<Department>();
            SearchText = string.Empty;

            // Команды
            LoadUsersCommand = new RelayCommand(LoadUsers);
            AddUserCommand = new RelayCommand(AddUser);
            EditUserCommand = new RelayCommand(EditUser, CanEditUser);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanDeleteUser);
            SaveUserCommand = new RelayCommand(SaveUser, CanSaveUser);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SearchCommand = new RelayCommand(SearchUsers);

            // Загрузка данных
            _ = LoadDataAsync();
        }

        // ========== Свойства ==========

        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ObservableCollection<Role> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        // ========== Команды ==========

        public RelayCommand LoadUsersCommand { get; }
        public RelayCommand AddUserCommand { get; }
        public RelayCommand EditUserCommand { get; }
        public RelayCommand DeleteUserCommand { get; }
        public RelayCommand SaveUserCommand { get; }
        public RelayCommand CancelEditCommand { get; }
        public RelayCommand SearchCommand { get; }

        // ========== Методы ==========

        /// <summary>
        /// Загружает все данные (пользователи, роли, отделы)
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                await LoadUsersAsync();
                await LoadRolesAsync();
                await LoadDepartmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает пользователей
        /// </summary>
        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                Users = new ObservableCollection<User>(users);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUsers()
        {
            _ = LoadUsersAsync();
        }

        /// <summary>
        /// Загружает роли
        /// </summary>
        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _roleRepository.GetAllAsync();
                Roles = new ObservableCollection<Role>(roles);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает отделы
        /// </summary>
        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _departmentRepository.GetAllAsync();
                Departments = new ObservableCollection<Department>(departments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавляет нового пользователя
        /// </summary>
        private void AddUser()
        {
            SelectedUser = new User
            {
                Id = GenerateNewId("US"),
                IsActive = true,
                HasAllDepartments = false,
                CreatedAt = DateTime.UtcNow
            };
            IsEditMode = true;
        }

        /// <summary>
        /// Редактирует выбранного пользователя
        /// </summary>
        private void EditUser()
        {
            if (SelectedUser != null)
            {
                IsEditMode = true;
            }
        }

        private bool CanEditUser()
        {
            return SelectedUser != null && !IsEditMode;
        }

        /// <summary>
        /// Удаляет выбранного пользователя
        /// </summary>
        private async void DeleteUser()
        {
            if (SelectedUser == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пользователя '{SelectedUser.FullName}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _userRepository.RemoveAsync(SelectedUser);
                    await LoadUsersAsync();
                    SelectedUser = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteUser()
        {
            return SelectedUser != null && !IsEditMode;
        }

        /// <summary>
        /// Сохраняет пользователя
        /// </summary>
        private async void SaveUser()
        {
            if (SelectedUser == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedUser.WindowsLogin))
            {
                MessageBox.Show("Введите логин Windows", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedUser.FullName))
            {
                MessageBox.Show("Введите ФИО", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (Users.Any(u => u.Id != SelectedUser.Id && u.WindowsLogin == SelectedUser.WindowsLogin))
                {
                    MessageBox.Show("Пользователь с таким логином Windows уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedUser.Key == 0)
                {
                    // Новый пользователь
                    await _userRepository.AddAsync(SelectedUser);
                }
                else
                {
                    // Обновление существующего
                    _userRepository.Update(SelectedUser);
                }

                await LoadUsersAsync();
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveUser()
        {
            return SelectedUser != null && IsEditMode;
        }

        /// <summary>
        /// Отменяет редактирование
        /// </summary>
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedUser = null;
        }

        /// <summary>
        /// Поиск пользователей
        /// </summary>
        private async void SearchUsers()
        {
            try
            {
                var allUsers = await _userRepository.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var filtered = allUsers.Where(u =>
                        u.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        u.WindowsLogin.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        u.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

                    Users = new ObservableCollection<User>(filtered);
                }
                else
                {
                    Users = new ObservableCollection<User>(allUsers);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Генерирует новый идентификатор в формате префикс + номер
        /// </summary>
        private string GenerateNewId(string prefix)
        {
            try
            {
                var existingIds = Users.Select(u => u.Id).Where(id => id.StartsWith(prefix));

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