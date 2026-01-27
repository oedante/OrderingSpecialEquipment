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
    /// ViewModel для окна редактирования ролей пользователей.
    /// </summary>
    public class EditRolesViewModel : ViewModelBase
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        private ObservableCollection<Role> _roles;
        private Role? _selectedRole;
        private bool _isEditing = false;
        private string _editId = string.Empty;
        private string _editName = string.Empty;
        private string _editCode = string.Empty;
        private string? _editDescription;

        // Права на таблицы
        private short _tab_AuditLogs = 0;
        private short _tab_Departments = 0;
        private short _tab_EquipmentDependencies = 0;
        private short _tab_Equipments = 0;
        private short _tab_LessorOrganizations = 0;
        private short _tab_LicensePlates = 0;
        private short _tab_Roles = 0;
        private short _tab_ShiftRequests = 0;
        private short _tab_TransportProgram = 0;
        private short _tab_UserDepartmentAccess = 0;
        private short _tab_UserFavorites = 0;
        private short _tab_Users = 0;
        private short _tab_UserWarehouseAccess = 0;
        private short _tab_WarehouseAreas = 0;
        private short _tab_Warehouses = 0;

        // Специальные права
        private bool _spec_ExportData = false;
        private bool _spec_ViewReports = false;
        private bool _spec_ManageAllDepartments = false;
        private bool _spec_ManageUsers = false;
        private bool _spec_SystemAdmin = false;
        private bool _editIsActive = true;

        public EditRolesViewModel(
            IRoleRepository roleRepository,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _roleRepository = roleRepository;
            _messageService = messageService;
            _authorizationService = authorizationService;

            _roles = new ObservableCollection<Role>();
            LoadRolesCommand = new RelayCommand(async _ => await LoadRolesAsync(), _ => _authorizationService.CanReadTable("Roles"));
            SaveRoleCommand = new RelayCommand(async _ => await SaveRoleAsync(), _ => CanSaveRole());
            DeleteRoleCommand = new RelayCommand(async _ => await DeleteRoleAsync(), _ => CanDeleteRole());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());

            Task.Run(async () => await LoadRolesAsync());
        }

        public ObservableCollection<Role> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public Role? SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    if (value != null)
                    {
                        _editId = value.Id;
                        _editName = value.Name;
                        _editCode = value.Code;
                        _editDescription = value.Description;
                        _tab_AuditLogs = value.TAB_AuditLogs;
                        _tab_Departments = value.TAB_Departments;
                        _tab_EquipmentDependencies = value.TAB_EquipmentDependencies;
                        _tab_Equipments = value.TAB_Equipments;
                        _tab_LessorOrganizations = value.TAB_LessorOrganizations;
                        _tab_LicensePlates = value.TAB_LicensePlates;
                        _tab_Roles = value.TAB_Roles;
                        _tab_ShiftRequests = value.TAB_ShiftRequests;
                        _tab_TransportProgram = value.TAB_TransportProgram;
                        _tab_UserDepartmentAccess = value.TAB_UserDepartmentAccess;
                        _tab_UserFavorites = value.TAB_UserFavorites;
                        _tab_Users = value.TAB_Users;
                        _tab_UserWarehouseAccess = value.TAB_UserWarehouseAccess;
                        _tab_WarehouseAreas = value.TAB_WarehouseAreas;
                        _tab_Warehouses = value.TAB_Warehouses;
                        _spec_ExportData = value.SPEC_ExportData;
                        _spec_ViewReports = value.SPEC_ViewReports;
                        _spec_ManageAllDepartments = value.SPEC_ManageAllDepartments;
                        _spec_ManageUsers = value.SPEC_ManageUsers;
                        _spec_SystemAdmin = value.SPEC_SystemAdmin;
                        _editIsActive = value.IsActive;
                        _isEditing = true;
                    }
                    else
                    {
                        ResetEditFields();
                    }
                    OnPropertyChanged(nameof(EditId));
                    OnPropertyChanged(nameof(EditName));
                    OnPropertyChanged(nameof(EditCode));
                    OnPropertyChanged(nameof(EditDescription));
                    OnPropertyChanged(nameof(Tab_AuditLogs));
                    OnPropertyChanged(nameof(Tab_Departments));
                    OnPropertyChanged(nameof(Tab_EquipmentDependencies));
                    OnPropertyChanged(nameof(Tab_Equipments));
                    OnPropertyChanged(nameof(Tab_LessorOrganizations));
                    OnPropertyChanged(nameof(Tab_LicensePlates));
                    OnPropertyChanged(nameof(Tab_Roles));
                    OnPropertyChanged(nameof(Tab_ShiftRequests));
                    OnPropertyChanged(nameof(Tab_TransportProgram));
                    OnPropertyChanged(nameof(Tab_UserDepartmentAccess));
                    OnPropertyChanged(nameof(Tab_UserFavorites));
                    OnPropertyChanged(nameof(Tab_Users));
                    OnPropertyChanged(nameof(Tab_UserWarehouseAccess));
                    OnPropertyChanged(nameof(Tab_WarehouseAreas));
                    OnPropertyChanged(nameof(Tab_Warehouses));
                    OnPropertyChanged(nameof(Spec_ExportData));
                    OnPropertyChanged(nameof(Spec_ViewReports));
                    OnPropertyChanged(nameof(Spec_ManageAllDepartments));
                    OnPropertyChanged(nameof(Spec_ManageUsers));
                    OnPropertyChanged(nameof(Spec_SystemAdmin));
                    OnPropertyChanged(nameof(EditIsActive));
                    ((RelayCommand)SaveRoleCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteRoleCommand).RaiseCanExecuteChanged();
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
                    ((RelayCommand)SaveRoleCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditName
        {
            get => _editName;
            set => SetProperty(ref _editName, value);
        }

        public string EditCode
        {
            get => _editCode;
            set => SetProperty(ref _editCode, value);
        }

        public string? EditDescription
        {
            get => _editDescription;
            set => SetProperty(ref _editDescription, value);
        }

        // Геттеры и сеттеры для прав на таблицы
        public short Tab_AuditLogs { get => _tab_AuditLogs; set => SetProperty(ref _tab_AuditLogs, value); }
        public short Tab_Departments { get => _tab_Departments; set => SetProperty(ref _tab_Departments, value); }
        public short Tab_EquipmentDependencies { get => _tab_EquipmentDependencies; set => SetProperty(ref _tab_EquipmentDependencies, value); }
        public short Tab_Equipments { get => _tab_Equipments; set => SetProperty(ref _tab_Equipments, value); }
        public short Tab_LessorOrganizations { get => _tab_LessorOrganizations; set => SetProperty(ref _tab_LessorOrganizations, value); }
        public short Tab_LicensePlates { get => _tab_LicensePlates; set => SetProperty(ref _tab_LicensePlates, value); }
        public short Tab_Roles { get => _tab_Roles; set => SetProperty(ref _tab_Roles, value); }
        public short Tab_ShiftRequests { get => _tab_ShiftRequests; set => SetProperty(ref _tab_ShiftRequests, value); }
        public short Tab_TransportProgram { get => _tab_TransportProgram; set => SetProperty(ref _tab_TransportProgram, value); }
        public short Tab_UserDepartmentAccess { get => _tab_UserDepartmentAccess; set => SetProperty(ref _tab_UserDepartmentAccess, value); }
        public short Tab_UserFavorites { get => _tab_UserFavorites; set => SetProperty(ref _tab_UserFavorites, value); }
        public short Tab_Users { get => _tab_Users; set => SetProperty(ref _tab_Users, value); }
        public short Tab_UserWarehouseAccess { get => _tab_UserWarehouseAccess; set => SetProperty(ref _tab_UserWarehouseAccess, value); }
        public short Tab_WarehouseAreas { get => _tab_WarehouseAreas; set => SetProperty(ref _tab_WarehouseAreas, value); }
        public short Tab_Warehouses { get => _tab_Warehouses; set => SetProperty(ref _tab_Warehouses, value); }

        // Геттеры и сеттеры для специальных прав
        public bool Spec_ExportData { get => _spec_ExportData; set => SetProperty(ref _spec_ExportData, value); }
        public bool Spec_ViewReports { get => _spec_ViewReports; set => SetProperty(ref _spec_ViewReports, value); }
        public bool Spec_ManageAllDepartments { get => _spec_ManageAllDepartments; set => SetProperty(ref _spec_ManageAllDepartments, value); }
        public bool Spec_ManageUsers { get => _spec_ManageUsers; set => SetProperty(ref _spec_ManageUsers, value); }
        public bool Spec_SystemAdmin { get => _spec_SystemAdmin; set => SetProperty(ref _spec_SystemAdmin, value); }

        public bool EditIsActive
        {
            get => _editIsActive;
            set => SetProperty(ref _editIsActive, value);
        }

        public ICommand LoadRolesCommand { get; }
        public ICommand SaveRoleCommand { get; }
        public ICommand DeleteRoleCommand { get; }
        public ICommand CancelEditCommand { get; }

        private async Task LoadRolesAsync()
        {
            try
            {
                var dbRoles = await _roleRepository.GetAllAsync();
                Roles.Clear();
                foreach (var role in dbRoles)
                {
                    // Фильтруем неактивные при отображении в основном списке, если нужно
                    // В данном случае, оставим все, чтобы пользователь мог редактировать IsActive
                    Roles.Add(role);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки ролей: {ex.Message}", "Ошибка");
            }
        }

        private bool CanSaveRole()
        {
            return _authorizationService.CanWriteTable("Roles") &&
                   !string.IsNullOrWhiteSpace(EditId) &&
                   !string.IsNullOrWhiteSpace(EditName) &&
                   !string.IsNullOrWhiteSpace(EditCode) &&
                   (!_isEditing || SelectedRole?.Id == EditId);
        }

        private async Task SaveRoleAsync()
        {
            if (!CanSaveRole()) return;

            try
            {
                Role role;
                bool isNew = !_isEditing;

                if (isNew)
                {
                    if (await _roleRepository.ExistsAsync(EditId))
                    {
                        _messageService.ShowErrorMessage($"Роль с ID '{EditId}' уже существует.", "Ошибка");
                        return;
                    }
                    role = new Role
                    {
                        Id = EditId,
                        Name = EditName,
                        Code = EditCode,
                        Description = EditDescription,
                        TAB_AuditLogs = Tab_AuditLogs,
                        TAB_Departments = Tab_Departments,
                        TAB_EquipmentDependencies = Tab_EquipmentDependencies,
                        TAB_Equipments = Tab_Equipments,
                        TAB_LessorOrganizations = Tab_LessorOrganizations,
                        TAB_LicensePlates = Tab_LicensePlates,
                        TAB_Roles = Tab_Roles,
                        TAB_ShiftRequests = Tab_ShiftRequests,
                        TAB_TransportProgram = Tab_TransportProgram,
                        TAB_UserDepartmentAccess = Tab_UserDepartmentAccess,
                        TAB_UserFavorites = Tab_UserFavorites,
                        TAB_Users = Tab_Users,
                        TAB_UserWarehouseAccess = Tab_UserWarehouseAccess,
                        TAB_WarehouseAreas = Tab_WarehouseAreas,
                        TAB_Warehouses = Tab_Warehouses,
                        SPEC_ExportData = Spec_ExportData,
                        SPEC_ViewReports = Spec_ViewReports,
                        SPEC_ManageAllDepartments = Spec_ManageAllDepartments,
                        SPEC_ManageUsers = Spec_ManageUsers,
                        SPEC_SystemAdmin = Spec_SystemAdmin,
                        IsActive = EditIsActive
                    };
                    await _roleRepository.AddAsync(role);
                }
                else
                {
                    role = SelectedRole!;
                    role.Name = EditName;
                    role.Code = EditCode;
                    role.Description = EditDescription;
                    role.TAB_AuditLogs = Tab_AuditLogs;
                    role.TAB_Departments = Tab_Departments;
                    role.TAB_EquipmentDependencies = Tab_EquipmentDependencies;
                    role.TAB_Equipments = Tab_Equipments;
                    role.TAB_LessorOrganizations = Tab_LessorOrganizations;
                    role.TAB_LicensePlates = Tab_LicensePlates;
                    role.TAB_Roles = Tab_Roles;
                    role.TAB_ShiftRequests = Tab_ShiftRequests;
                    role.TAB_TransportProgram = Tab_TransportProgram;
                    role.TAB_UserDepartmentAccess = Tab_UserDepartmentAccess;
                    role.TAB_UserFavorites = Tab_UserFavorites;
                    role.TAB_Users = Tab_Users;
                    role.TAB_UserWarehouseAccess = Tab_UserWarehouseAccess;
                    role.TAB_WarehouseAreas = Tab_WarehouseAreas;
                    role.TAB_Warehouses = Tab_Warehouses;
                    role.SPEC_ExportData = Spec_ExportData;
                    role.SPEC_ViewReports = Spec_ViewReports;
                    role.SPEC_ManageAllDepartments = Spec_ManageAllDepartments;
                    role.SPEC_ManageUsers = Spec_ManageUsers;
                    role.SPEC_SystemAdmin = Spec_SystemAdmin;
                    role.IsActive = EditIsActive;
                    _roleRepository.Update(role);
                }

                await _roleRepository.SaveChangesAsync();

                if (isNew)
                {
                    // Добавляем в список, даже если IsActive = false, для видимости
                    Roles.Add(role);
                }
                else
                {
                    await LoadRolesAsync(); // Обновление списка
                }

                ResetEditFields();
                _messageService.ShowInfoMessage(isNew ? "Роль добавлена." : "Роль обновлена.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка сохранения роли: {ex.Message}", "Ошибка");
            }
        }

        private bool CanDeleteRole()
        {
            return _authorizationService.CanWriteTable("Roles") &&
                   SelectedRole != null && SelectedRole.Key != 0;
        }

        private async Task DeleteRoleAsync()
        {
            if (!CanDeleteRole() || SelectedRole == null) return;

            if (!_messageService.ShowConfirmationDialog($"Вы действительно хотите удалить роль '{SelectedRole.Name}'?")) return;

            try
            {
                _roleRepository.Delete(SelectedRole);
                await _roleRepository.SaveChangesAsync();
                Roles.Remove(SelectedRole);
                ResetEditFields();
                _messageService.ShowInfoMessage("Роль удалена.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка удаления роли: {ex.Message}", "Ошибка");
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
            _editName = string.Empty;
            _editCode = string.Empty;
            _editDescription = null;
            _tab_AuditLogs = 0;
            _tab_Departments = 0;
            _tab_EquipmentDependencies = 0;
            _tab_Equipments = 0;
            _tab_LessorOrganizations = 0;
            _tab_LicensePlates = 0;
            _tab_Roles = 0;
            _tab_ShiftRequests = 0;
            _tab_TransportProgram = 0;
            _tab_UserDepartmentAccess = 0;
            _tab_UserFavorites = 0;
            _tab_Users = 0;
            _tab_UserWarehouseAccess = 0;
            _tab_WarehouseAreas = 0;
            _tab_Warehouses = 0;
            _spec_ExportData = false;
            _spec_ViewReports = false;
            _spec_ManageAllDepartments = false;
            _spec_ManageUsers = false;
            _spec_SystemAdmin = false;
            _editIsActive = true;
            SelectedRole = null;
            OnPropertyChanged(nameof(EditId));
            OnPropertyChanged(nameof(EditName));
            OnPropertyChanged(nameof(EditCode));
            OnPropertyChanged(nameof(EditDescription));
            OnPropertyChanged(nameof(Tab_AuditLogs));
            OnPropertyChanged(nameof(Tab_Departments));
            OnPropertyChanged(nameof(Tab_EquipmentDependencies));
            OnPropertyChanged(nameof(Tab_Equipments));
            OnPropertyChanged(nameof(Tab_LessorOrganizations));
            OnPropertyChanged(nameof(Tab_LicensePlates));
            OnPropertyChanged(nameof(Tab_Roles));
            OnPropertyChanged(nameof(Tab_ShiftRequests));
            OnPropertyChanged(nameof(Tab_TransportProgram));
            OnPropertyChanged(nameof(Tab_UserDepartmentAccess));
            OnPropertyChanged(nameof(Tab_UserFavorites));
            OnPropertyChanged(nameof(Tab_Users));
            OnPropertyChanged(nameof(Tab_UserWarehouseAccess));
            OnPropertyChanged(nameof(Tab_WarehouseAreas));
            OnPropertyChanged(nameof(Tab_Warehouses));
            OnPropertyChanged(nameof(Spec_ExportData));
            OnPropertyChanged(nameof(Spec_ViewReports));
            OnPropertyChanged(nameof(Spec_ManageAllDepartments));
            OnPropertyChanged(nameof(Spec_ManageUsers));
            OnPropertyChanged(nameof(Spec_SystemAdmin));
            OnPropertyChanged(nameof(EditIsActive));
            ((RelayCommand)SaveRoleCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteRoleCommand).RaiseCanExecuteChanged();
        }
    }
}