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
    /// ViewModel для редактирования ролей
    /// </summary>
    public class EditRolesViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRoleRepository _roleRepository;

        private ObservableCollection<Role> _roles;
        private Role? _selectedRole;
        private bool _isEditMode;
        private string _searchText;

        public EditRolesViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();

            Roles = new ObservableCollection<Role>();
            SearchText = string.Empty;

            // Команды
            LoadRolesCommand = new RelayCommand(LoadRoles);
            AddRoleCommand = new RelayCommand(AddRole);
            EditRoleCommand = new RelayCommand(EditRole, CanEditRole);
            DeleteRoleCommand = new RelayCommand(DeleteRole, CanDeleteRole);
            SaveRoleCommand = new RelayCommand(SaveRole, CanSaveRole);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SearchCommand = new RelayCommand(SearchRoles);

            // Загрузка данных
            _ = LoadRolesAsync();
        }

        // ========== Свойства ==========

        public ObservableCollection<Role> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public Role? SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
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

        // ========== Команды ==========

        public RelayCommand LoadRolesCommand { get; }
        public RelayCommand AddRoleCommand { get; }
        public RelayCommand EditRoleCommand { get; }
        public RelayCommand DeleteRoleCommand { get; }
        public RelayCommand SaveRoleCommand { get; }
        public RelayCommand CancelEditCommand { get; }
        public RelayCommand SearchCommand { get; }

        // ========== Методы ==========

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

        private void LoadRoles()
        {
            _ = LoadRolesAsync();
        }

        /// <summary>
        /// Добавляет новую роль
        /// </summary>
        private void AddRole()
        {
            SelectedRole = new Role
            {
                Id = GenerateNewId("RL"),
                IsActive = true,
                IsSystem = false,
                CreatedAt = DateTime.UtcNow
            };
            IsEditMode = true;
        }

        /// <summary>
        /// Редактирует выбранную роль
        /// </summary>
        private void EditRole()
        {
            if (SelectedRole != null)
            {
                IsEditMode = true;
            }
        }

        private bool CanEditRole()
        {
            return SelectedRole != null && !IsEditMode;
        }

        /// <summary>
        /// Удаляет выбранную роль
        /// </summary>
        private async void DeleteRole()
        {
            if (SelectedRole == null)
                return;

            if (SelectedRole.IsSystem)
            {
                MessageBox.Show("Системную роль нельзя удалить", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить роль '{SelectedRole.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _roleRepository.RemoveAsync(SelectedRole);
                    await LoadRolesAsync();
                    SelectedRole = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления роли: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteRole()
        {
            return SelectedRole != null && !IsEditMode && !SelectedRole.IsSystem;
        }

        /// <summary>
        /// Сохраняет роль
        /// </summary>
        private async void SaveRole()
        {
            if (SelectedRole == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedRole.Name))
            {
                MessageBox.Show("Введите наименование роли", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedRole.Code))
            {
                MessageBox.Show("Введите код роли", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (Roles.Any(r => r.Id != SelectedRole.Id && r.Name == SelectedRole.Name))
                {
                    MessageBox.Show("Роль с таким наименованием уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Roles.Any(r => r.Id != SelectedRole.Id && r.Code == SelectedRole.Code))
                {
                    MessageBox.Show("Роль с таким кодом уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedRole.Key == 0)
                {
                    // Новая роль
                    await _roleRepository.AddAsync(SelectedRole);
                }
                else
                {
                    // Обновление существующей
                    _roleRepository.Update(SelectedRole);
                }

                await LoadRolesAsync();
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения роли: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveRole()
        {
            return SelectedRole != null && IsEditMode;
        }

        /// <summary>
        /// Отменяет редактирование
        /// </summary>
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedRole = null;
        }

        /// <summary>
        /// Поиск ролей
        /// </summary>
        private async void SearchRoles()
        {
            try
            {
                var allRoles = await _roleRepository.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var filtered = allRoles.Where(r =>
                        r.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        r.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        r.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

                    Roles = new ObservableCollection<Role>(filtered);
                }
                else
                {
                    Roles = new ObservableCollection<Role>(allRoles);
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
                var existingIds = Roles.Select(r => r.Id).Where(id => id.StartsWith(prefix));

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