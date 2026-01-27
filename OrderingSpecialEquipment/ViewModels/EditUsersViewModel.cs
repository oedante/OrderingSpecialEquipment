using OrderingSpecialEquipment.Commands;
using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// ViewModel для окна редактирования пользователей.
    /// </summary>
    public class EditUsersViewModel : ViewModelBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        private ObservableCollection<User> _users;
        private User? _selectedUser;
        private bool _isEditing = false;
        private string _editId = string.Empty;
        private string _editWindowsLogin = string.Empty;
        private string _editFullName = string.Empty;
        private string? _editEmail;
        private string? _editPhone;
        private string _editRoleId = string.Empty; // Привязка к роли
        private string? _editDefaultDepartmentId; // Привязка к отделу по умолчанию
        private bool _editHasAllDepartments = false;
        private bool _editIsActive = true;

        // Для выбора в ComboBox
        private ObservableCollection<Role> _rolesForSelection;
        private ObservableCollection<Department> _departmentsForSelection;

        public EditUsersViewModel(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IDepartmentRepository departmentRepository,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _departmentRepository = departmentRepository;
            _messageService = messageService;
            _authorizationService = authorizationService;

            _users = new ObservableCollection<User>();
            _rolesForSelection = new ObservableCollection<Role>();
            _departmentsForSelection = new ObservableCollection<Department>();

            LoadUsersCommand = new RelayCommand(async _ => await LoadUsersAsync(), _ => _authorizationService.CanReadTable("Users"));
            SaveUserCommand = new RelayCommand(async _ => await SaveUserAsync(), _ => CanSaveUser());
            DeleteUserCommand = new RelayCommand(async _ => await DeleteUserAsync(), _ => CanDeleteUser());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());

            // Загрузка зависимых данных
            Task.Run(async () =>
            {
                await LoadRolesForSelectionAsync();
                await LoadDepartmentsForSelectionAsync();
            });

            Task.Run(async () => await LoadUsersAsync());
        }

        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
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
                        _editId = value.Id;
                        _editWindowsLogin = value.WindowsLogin;
                        _editFullName = value.FullName;
                        _editEmail = value.Email;
                        _editPhone = value.Phone;
                        _editRoleId = value.RoleId;
                        _editDefaultDepartmentId = value.DefaultDepartmentId;
                        _editHasAllDepartments = value.HasAllDepartments;
                        _editIsActive = value.IsActive;
                        _isEditing = true;
                    }
                    else
                    {
                        ResetEditFields();
                    }
                    OnPropertyChanged(nameof(EditId));
                    OnPropertyChanged(nameof(EditWindowsLogin));
                    OnPropertyChanged(nameof(EditFullName));
                    OnPropertyChanged(nameof(EditEmail));
                    OnPropertyChanged(nameof(EditPhone));
                    OnPropertyChanged(nameof(EditRoleId));
                    OnPropertyChanged(nameof(EditDefaultDepartmentId));
                    OnPropertyChanged(nameof(EditHasAllDepartments));
                    OnPropertyChanged(nameof(EditIsActive));
                    ((RelayCommand)SaveUserCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditId
        {
            get => _editId;
            set
            {
                if (SetProperty(ref _editId, value))
                {
                    ((RelayCommand)SaveUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditWindowsLogin
        {
            get => _editWindowsLogin;
            set => SetProperty(ref _editWindowsLogin, value);
        }

        public string EditFullName
        {
            get => _editFullName;
            set => SetProperty(ref _editFullName, value);
        }

        public string? EditEmail
        {
            get => _editEmail;
            set => SetProperty(ref _editEmail, value);
        }

        public string? EditPhone
        {
            get => _editPhone;
            set => SetProperty(ref _editPhone, value);
        }

        public string EditRoleId
        {
            get => _editRoleId;
            set => SetProperty(ref _editRoleId, value);
        }

        public string? EditDefaultDepartmentId
        {
            get => _editDefaultDepartmentId;
            set => SetProperty(ref _editDefaultDepartmentId, value);
        }

        public bool EditHasAllDepartments
        {
            get => _editHasAllDepartments;
            set => SetProperty(ref _editHasAllDepartments, value);
        }

        public bool EditIsActive
        {
            get => _editIsActive;
            set => SetProperty(ref _editIsActive, value);
        }

        // --- Свойства для выбора ---
        public ObservableCollection<Role> RolesForSelection
        {
            get => _rolesForSelection;
            set => SetProperty(ref _rolesForSelection, value);
        }

        public ObservableCollection<Department> DepartmentsForSelection
        {
            get => _departmentsForSelection;
            set => SetProperty(ref _departmentsForSelection, value);
        }

        public ICommand LoadUsersCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand CancelEditCommand { get; }

        private async Task LoadUsersAsync()
        {
            try
            {
                var dbUsers = await _userRepository.GetAllAsync();
                Users.Clear();
                foreach (var user in dbUsers)
                {
                    // Фильтруем неактивные при отображении в основном списке, если нужно
                    // В данном случае, оставим все, чтобы пользователь мог редактировать IsActive
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка");
            }
        }

        private bool CanSaveUser()
        {
            return _authorizationService.CanWriteTable("Users") &&
                   !string.IsNullOrWhiteSpace(EditId) &&
                   !string.IsNullOrWhiteSpace(EditWindowsLogin) &&
                   !string.IsNullOrWhiteSpace(EditFullName) &&
                   !string.IsNullOrWhiteSpace(EditRoleId) &&
                   (!_isEditing || SelectedUser?.Id == EditId);
        }

        private async Task SaveUserAsync()
        {
            if (!CanSaveUser()) return;

            try
            {
                User user;
                bool isNew = !_isEditing;

                if (isNew)
                {
                    if (await _userRepository.ExistsAsync(EditId))
                    {
                        _messageService.ShowErrorMessage($"Пользователь с ID '{EditId}' уже существует.", "Ошибка");
                        return;
                    }
                    user = new User
                    {
                        Id = EditId,
                        WindowsLogin = EditWindowsLogin,
                        FullName = EditFullName,
                        Email = EditEmail,
                        Phone = EditPhone,
                        RoleId = EditRoleId,
                        DefaultDepartmentId = EditDefaultDepartmentId,
                        HasAllDepartments = EditHasAllDepartments,
                        IsActive = EditIsActive
                    };
                    await _userRepository.AddAsync(user);
                }
                else
                {
                    user = SelectedUser!;
                    user.WindowsLogin = EditWindowsLogin;
                    user.FullName = EditFullName;
                    user.Email = EditEmail;
                    user.Phone = EditPhone;
                    user.RoleId = EditRoleId;
                    user.DefaultDepartmentId = EditDefaultDepartmentId;
                    user.HasAllDepartments = EditHasAllDepartments;
                    user.IsActive = EditIsActive;
                    _userRepository.Update(user);
                }

                await _userRepository.SaveChangesAsync();

                if (isNew)
                {
                    // Добавляем в список, даже если IsActive = false, для видимости
                    Users.Add(user);
                }
                else
                {
                    await LoadUsersAsync(); // Обновление списка
                }

                ResetEditFields();
                _messageService.ShowInfoMessage(isNew ? "Пользователь добавлен." : "Пользователь обновлен.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка сохранения пользователя: {ex.Message}", "Ошибка");
            }
        }

        private bool CanDeleteUser()
        {
            return _authorizationService.CanWriteTable("Users") &&
                   SelectedUser != null && SelectedUser.Key != 0;
        }

        private async Task DeleteUserAsync()
        {
            if (!CanDeleteUser() || SelectedUser == null) return;

            if (!_messageService.ShowConfirmationDialog($"Вы действительно хотите удалить пользователя '{SelectedUser.FullName}'?")) return;

            try
            {
                _userRepository.Delete(SelectedUser);
                await _userRepository.SaveChangesAsync();
                Users.Remove(SelectedUser);
                ResetEditFields();
                _messageService.ShowInfoMessage("Пользователь удален.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка удаления пользователя: {ex.Message}", "Ошибка");
            }
        }

        private void CancelEdit()
        {
            ResetEditFields();
        }

        private void ResetEditFields()
        {
            _isEditing = false;
            _editId = string.Empty;
            _editWindowsLogin = string.Empty;
            _editFullName = string.Empty;
            _editEmail = null;
            _editPhone = null;
            _editRoleId = string.Empty; // Сбросить на значение по умолчанию или оставить как есть?
            _editDefaultDepartmentId = null; // Сбросить на значение по умолчанию или оставить как есть?
            _editHasAllDepartments = false;
            _editIsActive = true;
            SelectedUser = null;
            OnPropertyChanged(nameof(EditId));
            OnPropertyChanged(nameof(EditWindowsLogin));
            OnPropertyChanged(nameof(EditFullName));
            OnPropertyChanged(nameof(EditEmail));
            OnPropertyChanged(nameof(EditPhone));
            OnPropertyChanged(nameof(EditRoleId));
            OnPropertyChanged(nameof(EditDefaultDepartmentId));
            OnPropertyChanged(nameof(EditHasAllDepartments));
            OnPropertyChanged(nameof(EditIsActive));
            ((RelayCommand)SaveUserCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteUserCommand).RaiseCanExecuteChanged();
        }

        // --- Вспомогательные методы с фильтрацией по правам ---
        private async Task LoadRolesForSelectionAsync()
        {
            try
            {
                var dbRoles = await _roleRepository.GetAllAsync();
                RolesForSelection.Clear();
                foreach (var role in dbRoles)
                {
                    // Фильтрация ролей: показываем только активные
                    // Также можно исключить системные роли, если текущий пользователь не админ
                    if (role.IsActive && (!_authorizationService.IsCurrentUserSystemAdmin() || !role.IsSystem))
                    {
                        RolesForSelection.Add(role);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки ролей для выбора: {ex.Message}", "Ошибка");
            }
        }

        private async Task LoadDepartmentsForSelectionAsync()
        {
            try
            {
                var dbDepts = await _departmentRepository.GetAllAsync();
                DepartmentsForSelection.Clear();
                foreach (var dept in dbDepts)
                {
                    // Фильтрация отделов: показываем только активные и доступные пользователю
                    if (dept.IsActive && _authorizationService.CanAccessDepartment(dept.Id))
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