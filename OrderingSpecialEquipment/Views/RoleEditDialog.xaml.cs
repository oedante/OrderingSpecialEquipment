using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Диалог редактирования роли
    /// </summary>
    public partial class RoleEditDialog : Window
    {
        #region Поля

        private readonly IDatabaseService _databaseService;
        private readonly Role _role;
        private readonly bool _isEditMode;

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор для создания новой роли
        /// </summary>
        public RoleEditDialog()
        {
            InitializeComponent();

            _databaseService = App.Services.GetRequiredService<IDatabaseService>();
            _role = new Role { IsActive = true };
            _isEditMode = false;
        }

        /// <summary>
        /// Конструктор для редактирования существующей роли
        /// </summary>
        public RoleEditDialog(Role role)
        {
            InitializeComponent();

            _databaseService = App.Services.GetRequiredService<IDatabaseService>();
            _role = role;
            _isEditMode = true;

            LoadRoleData();
        }

        #endregion

        #region Загрузка данных

        /// <summary>
        /// Загрузка данных роли в форму
        /// </summary>
        private void LoadRoleData()
        {
            txtCode.Text = _role.Code;
            txtName.Text = _role.Name;
            txtDescription.Text = _role.Description;

            // Права на чтение/запись
            chkDepartmentsRead.IsChecked = _role.TAB_Departments >= 1;
            chkDepartmentsWrite.IsChecked = _role.TAB_Departments >= 2;

            chkEquipmentsRead.IsChecked = _role.TAB_Equipments >= 1;
            chkEquipmentsWrite.IsChecked = _role.TAB_Equipments >= 2;

            chkLessorOrganizationsRead.IsChecked = _role.TAB_LessorOrganizations >= 1;
            chkLessorOrganizationsWrite.IsChecked = _role.TAB_LessorOrganizations >= 2;

            chkLicensePlatesRead.IsChecked = _role.TAB_LicensePlates >= 1;
            chkLicensePlatesWrite.IsChecked = _role.TAB_LicensePlates >= 2;

            chkEquipmentDependenciesRead.IsChecked = _role.TAB_EquipmentDependencies >= 1;
            chkEquipmentDependenciesWrite.IsChecked = _role.TAB_EquipmentDependencies >= 2;

            chkTransportProgramRead.IsChecked = _role.TAB_TransportProgram >= 1;
            chkTransportProgramWrite.IsChecked = _role.TAB_TransportProgram >= 2;

            chkShiftRequestsRead.IsChecked = _role.TAB_ShiftRequests >= 1;
            chkShiftRequestsWrite.IsChecked = _role.TAB_ShiftRequests >= 2;

            chkWarehousesRead.IsChecked = _role.TAB_Warehouses >= 1;
            chkWarehousesWrite.IsChecked = _role.TAB_Warehouses >= 2;

            chkWarehouseAreasRead.IsChecked = _role.TAB_WarehouseAreas >= 1;
            chkWarehouseAreasWrite.IsChecked = _role.TAB_WarehouseAreas >= 2;

            chkUsersRead.IsChecked = _role.TAB_Users >= 1;
            chkUsersWrite.IsChecked = _role.TAB_Users >= 2;

            chkRolesRead.IsChecked = _role.TAB_Roles >= 1;
            chkRolesWrite.IsChecked = _role.TAB_Roles >= 2;

            chkUserDepartmentAccessRead.IsChecked = _role.TAB_UserDepartmentAccess >= 1;
            chkUserDepartmentAccessWrite.IsChecked = _role.TAB_UserDepartmentAccess >= 2;

            chkUserWarehouseAccessRead.IsChecked = _role.TAB_UserWarehouseAccess >= 1;
            chkUserWarehouseAccessWrite.IsChecked = _role.TAB_UserWarehouseAccess >= 2;

            chkUserFavoritesRead.IsChecked = _role.TAB_UserFavorites >= 1;
            chkUserFavoritesWrite.IsChecked = _role.TAB_UserFavorites >= 2;

            chkAuditLogsRead.IsChecked = _role.TAB_AuditLogs >= 1;
            chkAuditLogsWrite.IsChecked = _role.TAB_AuditLogs >= 2;

            // Специальные права
            chkExportData.IsChecked = _role.SPEC_ExportData;
            chkViewReports.IsChecked = _role.SPEC_ViewReports;
            chkManageAllDepartments.IsChecked = _role.SPEC_ManageAllDepartments;
            chkManageUsers.IsChecked = _role.SPEC_ManageUsers;
            chkSystemAdmin.IsChecked = _role.SPEC_SystemAdmin;
            chkConfigureConnection.IsChecked = _role.SPEC_ConfigureConnection;
            chkIsSystem.IsChecked = _role.IsSystem;
            chkIsActive.IsChecked = _role.IsActive;
        }

        /// <summary>
        /// Сохранение данных роли
        /// </summary>
        private void SaveRoleData()
        {
            _role.Code = txtCode.Text.Trim();
            _role.Name = txtName.Text.Trim();
            _role.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();

            // Права на чтение/запись
            _role.TAB_Departments = GetPermissionLevel(chkDepartmentsRead, chkDepartmentsWrite);
            _role.TAB_Equipments = GetPermissionLevel(chkEquipmentsRead, chkEquipmentsWrite);
            _role.TAB_LessorOrganizations = GetPermissionLevel(chkLessorOrganizationsRead, chkLessorOrganizationsWrite);
            _role.TAB_LicensePlates = GetPermissionLevel(chkLicensePlatesRead, chkLicensePlatesWrite);
            _role.TAB_EquipmentDependencies = GetPermissionLevel(chkEquipmentDependenciesRead, chkEquipmentDependenciesWrite);
            _role.TAB_TransportProgram = GetPermissionLevel(chkTransportProgramRead, chkTransportProgramWrite);
            _role.TAB_ShiftRequests = GetPermissionLevel(chkShiftRequestsRead, chkShiftRequestsWrite);
            _role.TAB_Warehouses = GetPermissionLevel(chkWarehousesRead, chkWarehousesWrite);
            _role.TAB_WarehouseAreas = GetPermissionLevel(chkWarehouseAreasRead, chkWarehouseAreasWrite);
            _role.TAB_Users = GetPermissionLevel(chkUsersRead, chkUsersWrite);
            _role.TAB_Roles = GetPermissionLevel(chkRolesRead, chkRolesWrite);
            _role.TAB_UserDepartmentAccess = GetPermissionLevel(chkUserDepartmentAccessRead, chkUserDepartmentAccessWrite);
            _role.TAB_UserWarehouseAccess = GetPermissionLevel(chkUserWarehouseAccessRead, chkUserWarehouseAccessWrite);
            _role.TAB_UserFavorites = GetPermissionLevel(chkUserFavoritesRead, chkUserFavoritesWrite);
            _role.TAB_AuditLogs = GetPermissionLevel(chkAuditLogsRead, chkAuditLogsWrite);

            // Специальные права
            _role.SPEC_ExportData = chkExportData.IsChecked == true;
            _role.SPEC_ViewReports = chkViewReports.IsChecked == true;
            _role.SPEC_ManageAllDepartments = chkManageAllDepartments.IsChecked == true;
            _role.SPEC_ManageUsers = chkManageUsers.IsChecked == true;
            _role.SPEC_SystemAdmin = chkSystemAdmin.IsChecked == true;
            _role.SPEC_ConfigureConnection = chkConfigureConnection.IsChecked == true;
            _role.IsSystem = chkIsSystem.IsChecked == true;
            _role.IsActive = chkIsActive.IsChecked == true;
        }

        /// <summary>
        /// Получение уровня доступа (0-нет, 1-чтение, 2-запись)
        /// </summary>
        private short GetPermissionLevel(CheckBox readBox, CheckBox writeBox)
        {
            if (writeBox.IsChecked == true)
                return 2;
            if (readBox.IsChecked == true)
                return 1;
            return 0;
        }

        #endregion

        #region Обработчики событий

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("Код роли обязателен", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Наименование роли обязательно", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveRoleData();

                if (!_isEditMode)
                {
                    // Проверка уникальности кода
                    bool codeExists = await _databaseService.Context.Roles
                        .AnyAsync(r => r.Code == _role.Code);

                    if (codeExists)
                    {
                        MessageBox.Show("Роль с таким кодом уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _role.CreatedAt = DateTime.UtcNow;
                    await _databaseService.Context.Roles.AddAsync(_role);
                }
                else
                {
                    // Проверка уникальности кода (исключая текущую роль)
                    bool codeExists = await _databaseService.Context.Roles
                        .AnyAsync(r => r.Code == _role.Code && r.Id != _role.Id);

                    if (codeExists)
                    {
                        MessageBox.Show("Роль с таким кодом уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _databaseService.Context.Roles.Update(_role);
                }

                await _databaseService.Context.SaveChangesAsync();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}