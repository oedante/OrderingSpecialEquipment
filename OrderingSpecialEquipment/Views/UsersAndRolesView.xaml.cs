using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Логика взаимодействия для UsersAndRolesView.xaml
    /// </summary>
    /// <summary>
    /// Логика взаимодействия для UsersAndRolesView.xaml
    /// </summary>
    public partial class UsersAndRolesView : Window
    {
        #region Поля

        private readonly IDatabaseService _databaseService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationService _authenticationService;

        // Эти поля не используются, но оставим для будущего использования
        // private Role _editingRole;
        // private User _editingUser;
        // private bool _isRoleEditMode;
        // private bool _isUserEditMode;

        private string _selectedUserId = string.Empty;
        private string _selectedDepartmentId = string.Empty;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор окна пользователей и прав
        /// </summary>
        public UsersAndRolesView()
        {
            InitializeComponent();

            _databaseService = App.Services.GetRequiredService<IDatabaseService>();
            _authorizationService = App.Services.GetRequiredService<IAuthorizationService>();
            _authenticationService = App.Services.GetRequiredService<IAuthenticationService>();

            // Проверка прав
            if (!_authorizationService.HasSpecialPermission("ManageUsers") && !_authorizationService.IsSystemAdmin)
            {
                MessageBox.Show("У вас нет прав для управления пользователями.",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            Loaded += UsersAndRolesView_Loaded;
        }

        #endregion

        #region Загрузка данных

        private async void UsersAndRolesView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRolesAsync();
            await LoadUsersAsync();
            await LoadUsersForAccessAsync();
        }

        /// <summary>
        /// Загрузка ролей
        /// </summary>
        private async System.Threading.Tasks.Task LoadRolesAsync()
        {
            try
            {
                txtRolesStatus.Text = "Загрузка...";

                var roles = await _databaseService.Context.Roles
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                dgRoles.ItemsSource = roles;
                txtRolesStatus.Text = $"Загружено ролей: {roles.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtRolesStatus.Text = "Ошибка загрузки";
            }
        }

        /// <summary>
        /// Загрузка пользователей
        /// </summary>
        private async System.Threading.Tasks.Task LoadUsersAsync()
        {
            try
            {
                txtUsersStatus.Text = "Загрузка...";

                IQueryable<User> query = _databaseService.Context.Users
                    .Include(u => u.Role)
                    .Include(u => u.DefaultDepartment);

                if (!chkShowInactiveUsers.IsChecked == true)
                {
                    query = query.Where(u => u.IsActive);
                }

                var users = await query
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                dgUsers.ItemsSource = users;
                txtUsersStatus.Text = $"Загружено пользователей: {users.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsersStatus.Text = "Ошибка загрузки";
            }
        }

        /// <summary>
        /// Загрузка пользователей для вкладки доступа
        /// </summary>
        private async System.Threading.Tasks.Task LoadUsersForAccessAsync()
        {
            try
            {
                var users = await _databaseService.Context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                cmbAccessUser.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка доступных отделов для пользователя
        /// </summary>
        private async System.Threading.Tasks.Task LoadUserDepartmentsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    lbDepartments.ItemsSource = null;
                    return;
                }

                // Получаем все отделы
                var allDepartments = await _databaseService.Context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                // Получаем доступы пользователя
                var userAccesses = await _databaseService.Context.UserDepartmentAccesses
                    .Where(uda => uda.UserId == userId)
                    .ToDictionaryAsync(uda => uda.DepartmentId);

                // Формируем список с информацией о доступе
                var departmentItems = new List<DepartmentAccessItem>();
                foreach (var dept in allDepartments)
                {
                    departmentItems.Add(new DepartmentAccessItem
                    {
                        Department = dept,
                        HasAccess = userAccesses.ContainsKey(dept.Id),
                        UserDepartmentAccess = userAccesses.GetValueOrDefault(dept.Id),
                        IsSelected = false
                    });
                }

                lbDepartments.ItemsSource = departmentItems;
                lbDepartments.DisplayMemberPath = "Department.Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка складов для выбранного отдела и пользователя
        /// </summary>
        private async System.Threading.Tasks.Task LoadWarehousesForDepartmentAsync(string userId, string departmentId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(departmentId))
                {
                    lbWarehouses.ItemsSource = null;
                    txtWarehousesTitle.Text = "Склады выбранного отдела";
                    return;
                }

                var department = await _databaseService.Context.Departments
                    .FirstOrDefaultAsync(d => d.Id == departmentId);

                txtWarehousesTitle.Text = $"Склады отдела: {department?.Name}";

                // Получаем доступ к отделу
                var departmentAccess = await _databaseService.Context.UserDepartmentAccesses
                    .FirstOrDefaultAsync(uda => uda.UserId == userId && uda.DepartmentId == departmentId);

                if (departmentAccess == null)
                {
                    lbWarehouses.ItemsSource = null;
                    return;
                }

                chkHasAllWarehouses.IsChecked = departmentAccess.HasAllWarehouses;

                // Получаем все склады отдела
                var allWarehouses = await _databaseService.Context.Warehouses
                    .Where(w => w.DepartmentId == departmentId && w.IsActive)
                    .OrderBy(w => w.Name)
                    .ToListAsync();

                // Получаем доступы пользователя к складам - ИСПРАВЛЕНО
                var userWarehouseAccesses = await _databaseService.Context.UserWarehouseAccesses
                    .Where(uwa => uwa.UserDepartmentAccessKey == departmentAccess.Key)
                    .Select(uwa => uwa.WarehouseId)
                    .ToListAsync();

                var accessSet = new HashSet<string>(userWarehouseAccesses);

                // Формируем список с информацией о доступе
                var warehouseItems = new List<WarehouseAccessItem>();
                foreach (var wh in allWarehouses)
                {
                    warehouseItems.Add(new WarehouseAccessItem
                    {
                        Warehouse = wh,
                        HasAccess = accessSet.Contains(wh.Id)
                    });
                }

                lbWarehouses.ItemsSource = warehouseItems;
                lbWarehouses.DisplayMemberPath = "Warehouse.Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки складов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Обработчики ролей

        private void BtnAddRole_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RoleEditDialog
            {
                Owner = this,
                Title = "Добавление роли"
            };

            if (dialog.ShowDialog() == true)
            {
                _ = LoadRolesAsync();
            }
        }

        private void BtnEditRole_Click(object sender, RoutedEventArgs e)
        {
            if (dgRoles.SelectedItem is Role selected)
            {
                var dialog = new RoleEditDialog(selected)
                {
                    Owner = this,
                    Title = "Редактирование роли"
                };

                if (dialog.ShowDialog() == true)
                {
                    _ = LoadRolesAsync();
                }
            }
            else
            {
                MessageBox.Show("Выберите роль для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgRoles_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnEditRole_Click(sender, e);
        }

        private async void BtnDeleteRole_Click(object sender, RoutedEventArgs e)
        {
            if (dgRoles.SelectedItem is Role selected)
            {
                if (selected.IsSystem)
                {
                    MessageBox.Show("Системную роль нельзя удалить", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Удалить роль '{selected.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Проверяем, есть ли пользователи с этой ролью
                        bool hasUsers = await _databaseService.Context.Users
                            .AnyAsync(u => u.RoleId == selected.Id);

                        if (hasUsers)
                        {
                            // Если есть пользователи, просто деактивируем
                            selected.IsActive = false;
                            await _databaseService.Context.SaveChangesAsync();

                            MessageBox.Show("Роль деактивирована, так как есть пользователи с этой ролью",
                                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            _databaseService.Context.Roles.Remove(selected);
                            await _databaseService.Context.SaveChangesAsync();
                        }

                        await LoadRolesAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите роль для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnRefreshRoles_Click(object sender, RoutedEventArgs e)
        {
            await LoadRolesAsync();
        }

        #endregion

        #region Обработчики пользователей

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserEditDialog
            {
                Owner = this,
                Title = "Добавление пользователя"
            };

            if (dialog.ShowDialog() == true)
            {
                _ = LoadUsersAsync();
                _ = LoadUsersForAccessAsync();
            }
        }

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is User selected)
            {
                var dialog = new UserEditDialog(selected)
                {
                    Owner = this,
                    Title = "Редактирование пользователя"
                };

                if (dialog.ShowDialog() == true)
                {
                    _ = LoadUsersAsync();
                    _ = LoadUsersForAccessAsync();
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgUsers_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnEditUser_Click(sender, e);
        }

        private async void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is User selected)
            {
                // Нельзя удалить себя
                if (selected.Id == _authenticationService.CurrentUser?.Id)
                {
                    MessageBox.Show("Нельзя удалить текущего пользователя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Удалить пользователя '{selected.FullName}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Проверяем, есть ли связанные данные
                        bool hasShiftRequests = await _databaseService.Context.ShiftRequests
                            .AnyAsync(sr => sr.CreatedByUserId == selected.Id);

                        if (hasShiftRequests)
                        {
                            // Если есть заявки, просто деактивируем
                            selected.IsActive = false;
                            await _databaseService.Context.SaveChangesAsync();

                            MessageBox.Show("Пользователь деактивирован, так как есть связанные заявки",
                                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            // Удаляем связанные доступы
                            var accesses = await _databaseService.Context.UserDepartmentAccesses
                                .Where(uda => uda.UserId == selected.Id)
                                .ToListAsync();

                            if (accesses.Any())
                            {
                                _databaseService.Context.UserDepartmentAccesses.RemoveRange(accesses);
                            }

                            _databaseService.Context.Users.Remove(selected);
                            await _databaseService.Context.SaveChangesAsync();
                        }

                        await LoadUsersAsync();
                        await LoadUsersForAccessAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnRefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }

        private async void ChkShowInactiveUsers_Changed(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }

        #endregion

        #region Обработчики доступа

        private async void CmbAccessUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAccessUser.SelectedValue is string userId)
            {
                _selectedUserId = userId;
                await LoadUserDepartmentsAsync(userId);
            }
        }

        private async void LbDepartments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbDepartments.SelectedItem is DepartmentAccessItem selected && !string.IsNullOrEmpty(_selectedUserId))
            {
                _selectedDepartmentId = selected.Department.Id;
                await LoadWarehousesForDepartmentAsync(_selectedUserId, selected.Department.Id);
            }
        }

        private async void ChkHasAllWarehouses_Changed(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedUserId) && !string.IsNullOrEmpty(_selectedDepartmentId))
            {
                await LoadWarehousesForDepartmentAsync(_selectedUserId, _selectedDepartmentId);
            }
        }

        private async void BtnSaveDepartmentAccess_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedUserId))
            {
                MessageBox.Show("Выберите пользователя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (lbDepartments.SelectedItem is not DepartmentAccessItem selected)
            {
                MessageBox.Show("Выберите отдел", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var existingAccess = await _databaseService.Context.UserDepartmentAccesses
                    .FirstOrDefaultAsync(uda => uda.UserId == _selectedUserId &&
                                                uda.DepartmentId == selected.Department.Id);

                if (selected.HasAccess)
                {
                    // Уже есть доступ, обновляем
                    if (existingAccess != null)
                    {
                        existingAccess.HasAllWarehouses = chkHasAllWarehouses.IsChecked == true;
                        _databaseService.Context.UserDepartmentAccesses.Update(existingAccess);
                    }
                }
                else
                {
                    // Нет доступа, добавляем
                    if (existingAccess == null)
                    {
                        var newAccess = new UserDepartmentAccess
                        {
                            UserId = _selectedUserId,
                            DepartmentId = selected.Department.Id,
                            HasAllWarehouses = chkHasAllWarehouses.IsChecked == true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _databaseService.Context.UserDepartmentAccesses.AddAsync(newAccess);
                    }
                }

                await _databaseService.Context.SaveChangesAsync();
                await LoadUserDepartmentsAsync(_selectedUserId);

                MessageBox.Show("Доступ сохранен", "Успешно",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnSaveWarehouseAccess_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedUserId) || string.IsNullOrEmpty(_selectedDepartmentId))
            {
                MessageBox.Show("Выберите пользователя и отдел", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var departmentAccess = await _databaseService.Context.UserDepartmentAccesses
                    .FirstOrDefaultAsync(uda => uda.UserId == _selectedUserId &&
                                                uda.DepartmentId == _selectedDepartmentId);

                if (departmentAccess == null)
                {
                    MessageBox.Show("Сначала сохраните доступ к отделу", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Если установлен флаг "все склады", удаляем все конкретные доступы
                if (chkHasAllWarehouses.IsChecked == true)
                {
                    var existingAccesses = await _databaseService.Context.UserWarehouseAccesses
                        .Where(uwa => uwa.UserDepartmentAccessKey == departmentAccess.Key)
                        .ToListAsync();

                    if (existingAccesses.Any())
                    {
                        _databaseService.Context.UserWarehouseAccesses.RemoveRange(existingAccesses);
                    }
                }
                else
                {
                    // Получаем выбранные склады
                    var selectedWarehouses = lbWarehouses.ItemsSource.Cast<WarehouseAccessItem>()
                        .Where(w => w.HasAccess)
                        .Select(w => w.Warehouse.Id)
                        .ToHashSet();

                    // Получаем существующие доступы
                    var existingAccesses = await _databaseService.Context.UserWarehouseAccesses
                        .Where(uwa => uwa.UserDepartmentAccessKey == departmentAccess.Key)
                        .ToListAsync();

                    // Удаляем те, которых нет в выбранных
                    var toRemove = existingAccesses
                        .Where(ea => !selectedWarehouses.Contains(ea.WarehouseId))
                        .ToList();

                    if (toRemove.Any())
                    {
                        _databaseService.Context.UserWarehouseAccesses.RemoveRange(toRemove);
                    }

                    // Добавляем новые
                    var existingIds = existingAccesses.Select(ea => ea.WarehouseId).ToHashSet();
                    var toAdd = selectedWarehouses
                        .Where(whId => !existingIds.Contains(whId))
                        .Select(whId => new UserWarehouseAccess
                        {
                            UserDepartmentAccessKey = departmentAccess.Key,
                            WarehouseId = whId,
                            CreatedAt = DateTime.UtcNow
                        });

                    if (toAdd.Any())
                    {
                        await _databaseService.Context.UserWarehouseAccesses.AddRangeAsync(toAdd);
                    }
                }

                await _databaseService.Context.SaveChangesAsync();
                await LoadWarehousesForDepartmentAsync(_selectedUserId, _selectedDepartmentId);

                MessageBox.Show("Доступ к складам сохранен", "Успешно",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnRefreshAccess_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsersForAccessAsync();
            if (!string.IsNullOrEmpty(_selectedUserId))
            {
                await LoadUserDepartmentsAsync(_selectedUserId);
            }
        }

        #endregion

        #region Вспомогательные классы

        private class DepartmentAccessItem
        {
            public Department Department { get; set; }
            public bool HasAccess { get; set; }
            public UserDepartmentAccess UserDepartmentAccess { get; set; }
            public bool IsSelected { get; set; }
        }

        private class WarehouseAccessItem
        {
            public Warehouse Warehouse { get; set; }
            public bool HasAccess { get; set; }
        }

        #endregion
    }
}