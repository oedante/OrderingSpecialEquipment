using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Логика взаимодействия для DepartmentsView.xaml
    /// </summary>
    public partial class DepartmentsView : Window
    {
        #region Поля

        private readonly IDbContextFactory _contextFactory;
        private readonly IAuthorizationService _authorizationService;
        private Department _editingDepartment;
        private bool _isEditMode;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор окна отделов
        /// </summary>
        public DepartmentsView()
        {
            InitializeComponent();

            _contextFactory = App.Services.GetRequiredService<IDbContextFactory>();
            _authorizationService = App.Services.GetRequiredService<IAuthorizationService>();

            // Проверка прав
            if (!_authorizationService.CanReadTable("Departments"))
            {
                MessageBox.Show("У вас нет прав для просмотра этого справочника.",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            // Настройка кнопок в зависимости от прав
            btnAdd.IsEnabled = _authorizationService.CanWriteTable("Departments");
            btnEdit.IsEnabled = _authorizationService.CanWriteTable("Departments");
            btnDelete.IsEnabled = _authorizationService.CanWriteTable("Departments");

            Loaded += DepartmentsView_Loaded;
        }

        #endregion

        #region Обработчики событий загрузки

        /// <summary>
        /// Загрузка окна
        /// </summary>
        private async void DepartmentsView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// Загрузка данных
        /// </summary>
        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                txtStatus.Text = "Загрузка...";

                using var context = _contextFactory.CreateDbContext();

                IQueryable<Department> query = context.Departments;

                if (!chkShowInactive.IsChecked == true)
                {
                    query = query.Where(d => d.IsActive);
                }

                var departments = await query
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                dgDepartments.ItemsSource = departments;
                txtStatus.Text = $"Загружено записей: {departments.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Ошибка загрузки";
            }
        }

        #endregion

        #region Обработчики кнопок

        /// <summary>
        /// Добавление отдела
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            _editingDepartment = new Department
            {
                IsActive = true
            };
            _isEditMode = false;

            PopupTitle.Text = "Добавление отдела";
            txtName.Text = "";
            chkIsActive.IsChecked = true;

            EditPopup.IsOpen = true;
            txtName.Focus();
        }

        /// <summary>
        /// Редактирование отдела
        /// </summary>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgDepartments.SelectedItem is Department selected)
            {
                _editingDepartment = selected;
                _isEditMode = true;

                PopupTitle.Text = "Редактирование отдела";
                txtName.Text = selected.Name;
                chkIsActive.IsChecked = selected.IsActive;

                EditPopup.IsOpen = true;
                txtName.Focus();
            }
            else
            {
                MessageBox.Show("Выберите отдел для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Двойной клик по строке
        /// </summary>
        private void DgDepartments_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_authorizationService.CanWriteTable("Departments"))
            {
                BtnEdit_Click(sender, e);
            }
        }

        /// <summary>
        /// Удаление отдела
        /// </summary>
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDepartments.SelectedItem is Department selected)
            {
                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить отдел '{selected.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        txtStatus.Text = "Удаление...";

                        using var context = _contextFactory.CreateDbContext();

                        // Проверяем, есть ли связанные данные
                        bool hasWarehouses = await context.Warehouses
                            .AnyAsync(w => w.DepartmentId == selected.Id);

                        bool hasUsers = await context.Users
                            .AnyAsync(u => u.DefaultDepartmentId == selected.Id);

                        bool hasAccess = await context.UserDepartmentAccesses
                            .AnyAsync(uda => uda.DepartmentId == selected.Id);

                        if (hasWarehouses || hasUsers || hasAccess)
                        {
                            // Если есть связи, просто деактивируем
                            selected.IsActive = false;
                            context.Departments.Update(selected);
                            await context.SaveChangesAsync();

                            MessageBox.Show("Отдел деактивирован, так как есть связанные данные.",
                                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            // Если связей нет, удаляем физически
                            context.Departments.Remove(selected);
                            await context.SaveChangesAsync();
                        }

                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        txtStatus.Text = "Ошибка удаления";
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите отдел для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Обновление данных
        /// </summary>
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// Изменение фильтра показа неактивных
        /// </summary>
        private async void ChkShowInactive_Changed(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// Сохранение отдела
        /// </summary>
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Наименование отдела обязательно", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = _contextFactory.CreateDbContext();

                _editingDepartment.Name = txtName.Text.Trim();
                _editingDepartment.IsActive = chkIsActive.IsChecked == true;

                if (!_isEditMode)
                {
                    // Добавление нового
                    _editingDepartment.CreatedAt = DateTime.UtcNow;
                    await context.Departments.AddAsync(_editingDepartment);
                }
                else
                {
                    // Обновление существующего
                    context.Departments.Update(_editingDepartment);
                }

                await context.SaveChangesAsync();

                EditPopup.IsOpen = false;
                await LoadDataAsync();

                txtStatus.Text = _isEditMode ? "Отдел обновлен" : "Отдел добавлен";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отмена редактирования
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            EditPopup.IsOpen = false;
        }

        #endregion
    }
}