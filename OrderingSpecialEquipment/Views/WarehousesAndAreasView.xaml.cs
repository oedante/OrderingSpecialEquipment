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
    /// Логика взаимодействия для WarehousesAndAreasView.xaml
    /// </summary>
    public partial class WarehousesAndAreasView : Window
    {
        #region Поля

        private readonly IDbContextFactory _contextFactory;
        private readonly IAuthorizationService _authorizationService;
        private Warehouse _editingWarehouse;
        private WarehouseArea _editingArea;
        private bool _isWarehouseEditMode;
        private bool _isAreaEditMode;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор окна складов и территорий
        /// </summary>
        public WarehousesAndAreasView()
        {
            InitializeComponent();

            _contextFactory = App.Services.GetRequiredService<IDbContextFactory>();
            _authorizationService = App.Services.GetRequiredService<IAuthorizationService>();

            // Проверка прав
            if (!_authorizationService.CanReadTable("Warehouses"))
            {
                MessageBox.Show("У вас нет прав для просмотра этого справочника.",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            // Настройка кнопок в зависимости от прав
            btnAddWarehouse.IsEnabled = _authorizationService.CanWriteTable("Warehouses");
            btnEditWarehouse.IsEnabled = _authorizationService.CanWriteTable("Warehouses");
            btnDeleteWarehouse.IsEnabled = _authorizationService.CanWriteTable("Warehouses");

            btnAddArea.IsEnabled = _authorizationService.CanWriteTable("WarehouseAreas");
            btnEditArea.IsEnabled = _authorizationService.CanWriteTable("WarehouseAreas");
            btnDeleteArea.IsEnabled = _authorizationService.CanWriteTable("WarehouseAreas");

            Loaded += WarehousesAndAreasView_Loaded;
        }

        #endregion

        #region Обработчики событий загрузки

        /// <summary>
        /// Загрузка окна
        /// </summary>
        private async void WarehousesAndAreasView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDepartmentsAsync();
            await LoadWarehousesAsync();
        }

        /// <summary>
        /// Загрузка отделов для фильтра
        /// </summary>
        private async System.Threading.Tasks.Task LoadDepartmentsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var departments = await context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                departments.Insert(0, new Department { Id = "", Name = "Все отделы" });
                cmbDepartmentFilter.ItemsSource = departments;
                cmbDepartmentFilter.SelectedIndex = 0;

                // Загружаем отделы для выпадающего списка в редакторе склада
                var activeDepartments = departments.Where(d => !string.IsNullOrEmpty(d.Id)).ToList();
                cmbWarehouseDepartment.ItemsSource = activeDepartments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка складов
        /// </summary>
        private async System.Threading.Tasks.Task LoadWarehousesAsync()
        {
            try
            {
                txtStatus.Text = "Загрузка складов...";

                using var context = _contextFactory.CreateDbContext();

                IQueryable<Warehouse> query = context.Warehouses
                    .Include(w => w.Department);

                string selectedDepartmentId = cmbDepartmentFilter.SelectedValue as string;
                if (!string.IsNullOrEmpty(selectedDepartmentId))
                {
                    query = query.Where(w => w.DepartmentId == selectedDepartmentId);
                }

                var warehouses = await query
                    .OrderBy(w => w.Department.Name)
                    .ThenBy(w => w.Name)
                    .ToListAsync();

                dgWarehouses.ItemsSource = warehouses;
                txtStatus.Text = $"Загружено складов: {warehouses.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки складов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Ошибка загрузки";
            }
        }

        /// <summary>
        /// Загрузка территорий для выбранного склада
        /// </summary>
        private async System.Threading.Tasks.Task LoadAreasAsync(string warehouseId)
        {
            try
            {
                if (string.IsNullOrEmpty(warehouseId))
                {
                    dgAreas.ItemsSource = null;
                    txtAreasTitle.Text = "Территории";
                    return;
                }

                using var context = _contextFactory.CreateDbContext();

                var warehouse = await context.Warehouses
                    .FirstOrDefaultAsync(w => w.Id == warehouseId);

                txtAreasTitle.Text = $"Территории склада: {warehouse?.Name}";

                var areas = await context.WarehouseAreas
                    .Where(a => a.WarehouseId == warehouseId)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                dgAreas.ItemsSource = areas;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки территорий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Обработчики событий складов

        /// <summary>
        /// Изменение фильтра по отделу
        /// </summary>
        private async void CmbDepartmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadWarehousesAsync();
        }

        /// <summary>
        /// Выбор склада
        /// </summary>
        private async void DgWarehouses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgWarehouses.SelectedItem is Warehouse selected)
            {
                await LoadAreasAsync(selected.Id);
            }
            else
            {
                await LoadAreasAsync(null);
            }
        }

        /// <summary>
        /// Добавление склада
        /// </summary>
        private void BtnAddWarehouse_Click(object sender, RoutedEventArgs e)
        {
            _editingWarehouse = new Warehouse
            {
                IsActive = true
            };
            _isWarehouseEditMode = false;

            WarehousePopupTitle.Text = "Добавление склада";
            txtWarehouseName.Text = "";
            txtWarehouseAddress.Text = "";
            chkWarehouseIsActive.IsChecked = true;
            cmbWarehouseDepartment.SelectedIndex = -1;

            WarehouseEditPopup.IsOpen = true;
            txtWarehouseName.Focus();
        }

        /// <summary>
        /// Редактирование склада
        /// </summary>
        private void BtnEditWarehouse_Click(object sender, RoutedEventArgs e)
        {
            if (dgWarehouses.SelectedItem is Warehouse selected)
            {
                _editingWarehouse = selected;
                _isWarehouseEditMode = true;

                WarehousePopupTitle.Text = "Редактирование склада";
                txtWarehouseName.Text = selected.Name;
                txtWarehouseAddress.Text = selected.Address;
                chkWarehouseIsActive.IsChecked = selected.IsActive;
                cmbWarehouseDepartment.SelectedValue = selected.DepartmentId;

                WarehouseEditPopup.IsOpen = true;
                txtWarehouseName.Focus();
            }
            else
            {
                MessageBox.Show("Выберите склад для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Двойной клик по складу
        /// </summary>
        private void DgWarehouses_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_authorizationService.CanWriteTable("Warehouses"))
            {
                BtnEditWarehouse_Click(sender, e);
            }
        }

        /// <summary>
        /// Удаление склада
        /// </summary>
        private async void BtnDeleteWarehouse_Click(object sender, RoutedEventArgs e)
        {
            if (dgWarehouses.SelectedItem is Warehouse selected)
            {
                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить склад '{selected.Name}'?\n" +
                    "Это также удалит все связанные территории!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        txtStatus.Text = "Удаление...";

                        using var context = _contextFactory.CreateDbContext();

                        // Удаляем связанные территории
                        var areas = await context.WarehouseAreas
                            .Where(a => a.WarehouseId == selected.Id)
                            .ToListAsync();

                        if (areas.Any())
                        {
                            context.WarehouseAreas.RemoveRange(areas);
                        }

                        context.Warehouses.Remove(selected);
                        await context.SaveChangesAsync();

                        await LoadWarehousesAsync();
                        await LoadAreasAsync(null);

                        txtStatus.Text = "Склад удален";
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
                MessageBox.Show("Выберите склад для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Сохранение склада
        /// </summary>
        private async void BtnSaveWarehouse_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtWarehouseName.Text))
            {
                MessageBox.Show("Наименование склада обязательно", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbWarehouseDepartment.SelectedValue == null)
            {
                MessageBox.Show("Выберите отдел для склада", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = _contextFactory.CreateDbContext();

                _editingWarehouse.Name = txtWarehouseName.Text.Trim();
                _editingWarehouse.DepartmentId = cmbWarehouseDepartment.SelectedValue.ToString();
                _editingWarehouse.Address = string.IsNullOrWhiteSpace(txtWarehouseAddress.Text) ? null : txtWarehouseAddress.Text.Trim();
                _editingWarehouse.IsActive = chkWarehouseIsActive.IsChecked == true;

                if (!_isWarehouseEditMode)
                {
                    // Добавление нового
                    _editingWarehouse.CreatedAt = DateTime.UtcNow;
                    await context.Warehouses.AddAsync(_editingWarehouse);
                }
                else
                {
                    // Обновление существующего
                    context.Warehouses.Update(_editingWarehouse);
                }

                await context.SaveChangesAsync();

                WarehouseEditPopup.IsOpen = false;
                await LoadWarehousesAsync();

                txtStatus.Text = _isWarehouseEditMode ? "Склад обновлен" : "Склад добавлен";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отмена редактирования склада
        /// </summary>
        private void BtnCancelWarehouse_Click(object sender, RoutedEventArgs e)
        {
            WarehouseEditPopup.IsOpen = false;
        }

        #endregion

        #region Обработчики событий территорий

        /// <summary>
        /// Добавление территории
        /// </summary>
        private void BtnAddArea_Click(object sender, RoutedEventArgs e)
        {
            if (dgWarehouses.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите склад", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _editingArea = new WarehouseArea
            {
                WarehouseId = ((Warehouse)dgWarehouses.SelectedItem).Id,
                IsActive = true
            };
            _isAreaEditMode = false;

            AreaPopupTitle.Text = "Добавление территории";
            txtAreaName.Text = "";
            cmbAreaType.SelectedIndex = -1;
            txtAreaCapacity.Text = "";
            chkAreaIsActive.IsChecked = true;

            AreaEditPopup.IsOpen = true;
            txtAreaName.Focus();
        }

        /// <summary>
        /// Редактирование территории
        /// </summary>
        private void BtnEditArea_Click(object sender, RoutedEventArgs e)
        {
            if (dgAreas.SelectedItem is WarehouseArea selected)
            {
                _editingArea = selected;
                _isAreaEditMode = true;

                AreaPopupTitle.Text = "Редактирование территории";
                txtAreaName.Text = selected.Name;

                // Выбор типа в ComboBox
                bool typeFound = false;
                for (int i = 0; i < cmbAreaType.Items.Count; i++)
                {
                    if ((cmbAreaType.Items[i] as ComboBoxItem)?.Content.ToString() == selected.AreaType)
                    {
                        cmbAreaType.SelectedIndex = i;
                        typeFound = true;
                        break;
                    }
                }
                if (!typeFound) cmbAreaType.SelectedIndex = -1;

                txtAreaCapacity.Text = selected.MaxCapacity?.ToString();
                chkAreaIsActive.IsChecked = selected.IsActive;

                AreaEditPopup.IsOpen = true;
                txtAreaName.Focus();
            }
            else
            {
                MessageBox.Show("Выберите территорию для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Двойной клик по территории
        /// </summary>
        private void DgAreas_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_authorizationService.CanWriteTable("WarehouseAreas"))
            {
                BtnEditArea_Click(sender, e);
            }
        }

        /// <summary>
        /// Удаление территории
        /// </summary>
        private async void BtnDeleteArea_Click(object sender, RoutedEventArgs e)
        {
            if (dgAreas.SelectedItem is WarehouseArea selected)
            {
                var result = MessageBox.Show(
                    $"Удалить территорию '{selected.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        context.WarehouseAreas.Remove(selected);
                        await context.SaveChangesAsync();

                        if (dgWarehouses.SelectedItem is Warehouse warehouse)
                        {
                            await LoadAreasAsync(warehouse.Id);
                        }

                        txtStatus.Text = "Территория удалена";
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
                MessageBox.Show("Выберите территорию для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Сохранение территории
        /// </summary>
        private async void BtnSaveArea_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAreaName.Text))
            {
                MessageBox.Show("Наименование территории обязательно", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = _contextFactory.CreateDbContext();

                _editingArea.Name = txtAreaName.Text.Trim();
                _editingArea.AreaType = cmbAreaType.SelectedItem is ComboBoxItem item ? item.Content.ToString() : null;

                if (int.TryParse(txtAreaCapacity.Text, out int capacity))
                {
                    _editingArea.MaxCapacity = capacity;
                }
                else
                {
                    _editingArea.MaxCapacity = null;
                }

                _editingArea.IsActive = chkAreaIsActive.IsChecked == true;

                if (!_isAreaEditMode)
                {
                    // Добавление новой
                    _editingArea.CreatedAt = DateTime.UtcNow;
                    await context.WarehouseAreas.AddAsync(_editingArea);
                }
                else
                {
                    // Обновление существующей
                    context.WarehouseAreas.Update(_editingArea);
                }

                await context.SaveChangesAsync();

                AreaEditPopup.IsOpen = false;

                if (dgWarehouses.SelectedItem is Warehouse warehouse)
                {
                    await LoadAreasAsync(warehouse.Id);
                }

                txtStatus.Text = _isAreaEditMode ? "Территория обновлена" : "Территория добавлена";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отмена редактирования территории
        /// </summary>
        private void BtnCancelArea_Click(object sender, RoutedEventArgs e)
        {
            AreaEditPopup.IsOpen = false;
        }

        #endregion

        /// <summary>
        /// Обновление данных
        /// </summary>
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDepartmentsAsync();
            await LoadWarehousesAsync();
        }
    }
}