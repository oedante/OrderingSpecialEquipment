using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Конвертер для отображения уровня доступа в текст
    /// </summary>
    public class AccessLevelToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is short level)
            {
                return level switch
                {
                    0 => "Запрещено",
                    1 => "Чтение",
                    2 => "Полный доступ",
                    _ => "Неизвестно"
                };
            }
            return "Запрещено";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Диалог редактирования роли
    /// </summary>
    public partial class RoleEditDialog : Window
    {
        #region Поля

        private readonly IDbContextFactory _contextFactory;
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
            _contextFactory = App.Services.GetRequiredService<IDbContextFactory>();
            _role = new Role { IsActive = true };
            _isEditMode = false;

            // Устанавливаем значения по умолчанию
            SetDefaultPermissions();
        }

        /// <summary>
        /// Конструктор для редактирования существующей роли
        /// </summary>
        public RoleEditDialog(Role role)
        {
            InitializeComponent();
            _contextFactory = App.Services.GetRequiredService<IDbContextFactory>();
            _role = role;
            _isEditMode = true;

            LoadRoleData();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Установка прав по умолчанию
        /// </summary>
        private void SetDefaultPermissions()
        {
            // Все права по умолчанию - "Запрещено" (индекс 0)
            cmbDepartmentsAccess.SelectedIndex = 0;
            cmbEquipmentsAccess.SelectedIndex = 0;
            cmbLessorOrganizationsAccess.SelectedIndex = 0;
            cmbLicensePlatesAccess.SelectedIndex = 0;
            cmbEquipmentDependenciesAccess.SelectedIndex = 0;
            cmbTransportProgramAccess.SelectedIndex = 0;
            cmbShiftRequestsAccess.SelectedIndex = 0;
            cmbWarehousesAccess.SelectedIndex = 0;
            cmbWarehouseAreasAccess.SelectedIndex = 0;
            cmbUsersAccess.SelectedIndex = 0;
            cmbRolesAccess.SelectedIndex = 0;
            cmbUserDepartmentAccessAccess.SelectedIndex = 0;
            cmbUserWarehouseAccessAccess.SelectedIndex = 0;
            cmbUserFavoritesAccess.SelectedIndex = 0;
            cmbAuditLogsAccess.SelectedIndex = 0;

            // Специальные права
            chkExportData.IsChecked = false;
            chkViewReports.IsChecked = false;
            chkManageAllDepartments.IsChecked = false;
            chkManageUsers.IsChecked = false;
            chkSystemAdmin.IsChecked = false;
            chkConfigureConnection.IsChecked = false;
            chkIsSystem.IsChecked = false;
            chkIsActive.IsChecked = true;
        }

        /// <summary>
        /// Загрузка данных роли в форму
        /// </summary>
        private void LoadRoleData()
        {
            txtCode.Text = _role.Code;
            txtName.Text = _role.Name;
            txtDescription.Text = _role.Description;

            // Установка уровня доступа для таблиц
            SetComboBoxAccess(cmbDepartmentsAccess, _role.TAB_Departments);
            SetComboBoxAccess(cmbEquipmentsAccess, _role.TAB_Equipments);
            SetComboBoxAccess(cmbLessorOrganizationsAccess, _role.TAB_LessorOrganizations);
            SetComboBoxAccess(cmbLicensePlatesAccess, _role.TAB_LicensePlates);
            SetComboBoxAccess(cmbEquipmentDependenciesAccess, _role.TAB_EquipmentDependencies);
            SetComboBoxAccess(cmbTransportProgramAccess, _role.TAB_TransportProgram);
            SetComboBoxAccess(cmbShiftRequestsAccess, _role.TAB_ShiftRequests);
            SetComboBoxAccess(cmbWarehousesAccess, _role.TAB_Warehouses);
            SetComboBoxAccess(cmbWarehouseAreasAccess, _role.TAB_WarehouseAreas);
            SetComboBoxAccess(cmbUsersAccess, _role.TAB_Users);
            SetComboBoxAccess(cmbRolesAccess, _role.TAB_Roles);
            SetComboBoxAccess(cmbUserDepartmentAccessAccess, _role.TAB_UserDepartmentAccess);
            SetComboBoxAccess(cmbUserWarehouseAccessAccess, _role.TAB_UserWarehouseAccess);
            SetComboBoxAccess(cmbUserFavoritesAccess, _role.TAB_UserFavorites);
            SetComboBoxAccess(cmbAuditLogsAccess, _role.TAB_AuditLogs);

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
        /// Установка значения в ComboBox по уровню доступа
        /// </summary>
        private void SetComboBoxAccess(ComboBox comboBox, short accessLevel)
        {
            comboBox.SelectedIndex = accessLevel switch
            {
                0 => 0, // Запрещено
                1 => 1, // Только чтение
                2 => 2, // Полный доступ
                _ => 0
            };
        }

        /// <summary>
        /// Получение уровня доступа из ComboBox
        /// </summary>
        private short GetAccessLevel(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                return short.Parse(item.Tag.ToString());
            }
            return (short)comboBox.SelectedIndex;
        }

        /// <summary>
        /// Сохранение данных роли
        /// </summary>
        private void SaveRoleData()
        {
            _role.Code = txtCode.Text.Trim();
            _role.Name = txtName.Text.Trim();
            _role.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();

            // Сохранение уровня доступа для таблиц
            _role.TAB_Departments = GetAccessLevel(cmbDepartmentsAccess);
            _role.TAB_Equipments = GetAccessLevel(cmbEquipmentsAccess);
            _role.TAB_LessorOrganizations = GetAccessLevel(cmbLessorOrganizationsAccess);
            _role.TAB_LicensePlates = GetAccessLevel(cmbLicensePlatesAccess);
            _role.TAB_EquipmentDependencies = GetAccessLevel(cmbEquipmentDependenciesAccess);
            _role.TAB_TransportProgram = GetAccessLevel(cmbTransportProgramAccess);
            _role.TAB_ShiftRequests = GetAccessLevel(cmbShiftRequestsAccess);
            _role.TAB_Warehouses = GetAccessLevel(cmbWarehousesAccess);
            _role.TAB_WarehouseAreas = GetAccessLevel(cmbWarehouseAreasAccess);
            _role.TAB_Users = GetAccessLevel(cmbUsersAccess);
            _role.TAB_Roles = GetAccessLevel(cmbRolesAccess);
            _role.TAB_UserDepartmentAccess = GetAccessLevel(cmbUserDepartmentAccessAccess);
            _role.TAB_UserWarehouseAccess = GetAccessLevel(cmbUserWarehouseAccessAccess);
            _role.TAB_UserFavorites = GetAccessLevel(cmbUserFavoritesAccess);
            _role.TAB_AuditLogs = GetAccessLevel(cmbAuditLogsAccess);

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

        #endregion

        #region Обработчики событий

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
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
                using var context = _contextFactory.CreateDbContext();

                SaveRoleData();

                if (!_isEditMode)
                {
                    // Проверка уникальности кода
                    bool codeExists = await context.Roles
                        .AnyAsync(r => r.Code == _role.Code);

                    if (codeExists)
                    {
                        MessageBox.Show("Роль с таким кодом уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _role.CreatedAt = DateTime.UtcNow;
                    await context.Roles.AddAsync(_role);
                }
                else
                {
                    // Проверка уникальности кода (исключая текущую роль)
                    bool codeExists = await context.Roles
                        .AnyAsync(r => r.Code == _role.Code && r.Id != _role.Id);

                    if (codeExists)
                    {
                        MessageBox.Show("Роль с таким кодом уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    context.Roles.Update(_role);
                }

                await context.SaveChangesAsync();

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