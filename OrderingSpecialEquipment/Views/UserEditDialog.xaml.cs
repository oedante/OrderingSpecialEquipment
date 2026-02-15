using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Linq;
using System.Windows;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Диалог редактирования пользователя
    /// </summary>
    public partial class UserEditDialog : Window
    {
        #region Поля

        private readonly IDatabaseService _databaseService;
        private readonly User _user;
        private readonly bool _isEditMode;

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор для создания нового пользователя
        /// </summary>
        public UserEditDialog()
        {
            InitializeComponent();

            _databaseService = App.Services.GetRequiredService<IDatabaseService>();
            _user = new User { IsActive = true };
            _isEditMode = false;

            Loaded += UserEditDialog_Loaded;
        }

        /// <summary>
        /// Конструктор для редактирования существующего пользователя
        /// </summary>
        public UserEditDialog(User user)
        {
            InitializeComponent();

            _databaseService = App.Services.GetRequiredService<IDatabaseService>();
            _user = user;
            _isEditMode = true;

            Loaded += UserEditDialog_Loaded;
        }

        #endregion

        #region Загрузка

        private async void UserEditDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRolesAsync();
            await LoadDepartmentsAsync();

            if (_isEditMode)
            {
                LoadUserData();
            }
        }

        /// <summary>
        /// Загрузка ролей
        /// </summary>
        private async System.Threading.Tasks.Task LoadRolesAsync()
        {
            try
            {
                var roles = await _databaseService.Context.Roles
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                cmbRole.ItemsSource = roles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка отделов
        /// </summary>
        private async System.Threading.Tasks.Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _databaseService.Context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                departments.Insert(0, new Department { Id = "", Name = "Не выбран" });
                cmbDefaultDepartment.ItemsSource = departments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка данных пользователя
        /// </summary>
        private void LoadUserData()
        {
            txtWindowsLogin.Text = _user.WindowsLogin;
            txtFullName.Text = _user.FullName;
            txtEmail.Text = _user.Email;
            txtPhone.Text = _user.Phone;
            cmbRole.SelectedValue = _user.RoleId;
            cmbDefaultDepartment.SelectedValue = _user.DefaultDepartmentId;
            chkHasAllDepartments.IsChecked = _user.HasAllDepartments;
            chkIsActive.IsChecked = _user.IsActive;
        }

        #endregion

        #region Обработчики

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtWindowsLogin.Text))
            {
                MessageBox.Show("Windows логин обязателен", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Полное имя обязательно", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbRole.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _user.WindowsLogin = txtWindowsLogin.Text.Trim();
                _user.FullName = txtFullName.Text.Trim();
                _user.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
                _user.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
                _user.RoleId = cmbRole.SelectedValue.ToString();
                _user.DefaultDepartmentId = cmbDefaultDepartment.SelectedValue as string;
                _user.HasAllDepartments = chkHasAllDepartments.IsChecked == true;
                _user.IsActive = chkIsActive.IsChecked == true;

                if (!_isEditMode)
                {
                    // Проверка уникальности логина
                    bool loginExists = await _databaseService.Context.Users
                        .AnyAsync(u => u.WindowsLogin == _user.WindowsLogin);

                    if (loginExists)
                    {
                        MessageBox.Show("Пользователь с таким Windows логином уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _user.CreatedAt = DateTime.UtcNow;
                    await _databaseService.Context.Users.AddAsync(_user);
                }
                else
                {
                    // Проверка уникальности логина (исключая текущего пользователя)
                    bool loginExists = await _databaseService.Context.Users
                        .AnyAsync(u => u.WindowsLogin == _user.WindowsLogin && u.Id != _user.Id);

                    if (loginExists)
                    {
                        MessageBox.Show("Пользователь с таким Windows логином уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _databaseService.Context.Users.Update(_user);
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