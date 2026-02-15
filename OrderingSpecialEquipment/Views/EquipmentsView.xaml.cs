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
    /// Логика взаимодействия для EquipmentsView.xaml
    /// </summary>
    public partial class EquipmentsView : Window
    {
        #region Поля

        private readonly IDatabaseService _databaseService;
        private readonly IAuthorizationService _authorizationService;
        private Equipment _editingEquipment;
        private bool _isEditMode;
        private Equipment _selectedEquipmentForDependencies;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор окна техники
        /// </summary>
        public EquipmentsView()
        {
            InitializeComponent();

            _databaseService = App.Services.GetRequiredService<IDatabaseService>();
            _authorizationService = App.Services.GetRequiredService<IAuthorizationService>();

            // Проверка прав
            if (!_authorizationService.CanReadTable("Equipments"))
            {
                MessageBox.Show("У вас нет прав для просмотра этого справочника.",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            // Настройка кнопок в зависимости от прав
            btnAdd.IsEnabled = _authorizationService.CanWriteTable("Equipments");
            btnEdit.IsEnabled = _authorizationService.CanWriteTable("Equipments");
            btnDelete.IsEnabled = _authorizationService.CanWriteTable("Equipments");
            btnDependencies.IsEnabled = _authorizationService.CanWriteTable("EquipmentDependencies");

            Loaded += EquipmentsView_Loaded;
        }

        #endregion

        #region Обработчики событий загрузки

        /// <summary>
        /// Загрузка окна
        /// </summary>
        private async void EquipmentsView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
            await LoadDependentEquipmentsAsync();
        }

        /// <summary>
        /// Загрузка данных
        /// </summary>
        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                txtStatus.Text = "Загрузка...";

                IQueryable<Equipment> query = _databaseService.Context.Equipments;

                if (!chkShowInactive.IsChecked == true)
                {
                    query = query.Where(e => e.IsActive);
                }

                var equipments = await query
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                dgEquipments.ItemsSource = equipments;
                txtStatus.Text = $"Загружено записей: {equipments.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Ошибка загрузки";
            }
        }

        /// <summary>
        /// Загрузка списка техники для зависимостей
        /// </summary>
        private async System.Threading.Tasks.Task LoadDependentEquipmentsAsync()
        {
            try
            {
                var equipments = await _databaseService.Context.Equipments
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                cmbDependentEquipment.ItemsSource = equipments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки техники для зависимостей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Обработчики кнопок основного окна

        /// <summary>
        /// Добавление техники
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            _editingEquipment = new Equipment
            {
                IsActive = true,
                CanOrderMultiple = false,
                RequiresOperator = false
            };
            _isEditMode = false;

            PopupTitle.Text = "Добавление техники";
            txtName.Text = "";
            txtCategory.Text = "";
            txtHourlyCost.Text = "";
            chkCanOrderMultiple.IsChecked = false;
            chkRequiresOperator.IsChecked = false;
            chkIsActive.IsChecked = true;
            txtDescription.Text = "";

            EditPopup.IsOpen = true;
            txtName.Focus();
        }

        /// <summary>
        /// Редактирование техники
        /// </summary>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgEquipments.SelectedItem is Equipment selected)
            {
                _editingEquipment = selected;
                _isEditMode = true;

                PopupTitle.Text = "Редактирование техники";
                txtName.Text = selected.Name;
                txtCategory.Text = selected.Category;
                txtHourlyCost.Text = selected.HourlyCost?.ToString() ?? "";
                chkCanOrderMultiple.IsChecked = selected.CanOrderMultiple;
                chkRequiresOperator.IsChecked = selected.RequiresOperator;
                chkIsActive.IsChecked = selected.IsActive;
                txtDescription.Text = selected.Description;

                EditPopup.IsOpen = true;
                txtName.Focus();
            }
            else
            {
                MessageBox.Show("Выберите технику для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Двойной клик по строке
        /// </summary>
        private void DgEquipments_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_authorizationService.CanWriteTable("Equipments"))
            {
                BtnEdit_Click(sender, e);
            }
        }

        /// <summary>
        /// Удаление техники
        /// </summary>
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgEquipments.SelectedItem is Equipment selected)
            {
                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить технику '{selected.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        txtStatus.Text = "Удаление...";

                        // Проверяем, есть ли связанные данные
                        bool hasLicensePlates = await _databaseService.Context.LicensePlates
                            .AnyAsync(lp => lp.EquipmentId == selected.Id);

                        bool hasShiftRequests = await _databaseService.Context.ShiftRequests
                            .AnyAsync(sr => sr.EquipmentId == selected.Id);

                        bool hasDependencies = await _databaseService.Context.EquipmentDependencies
                            .AnyAsync(ed => ed.MainEquipmentId == selected.Id || ed.DependentEquipmentId == selected.Id);

                        if (hasLicensePlates || hasShiftRequests || hasDependencies)
                        {
                            // Если есть связи, просто деактивируем
                            selected.IsActive = false;
                            await _databaseService.Context.SaveChangesAsync();

                            MessageBox.Show("Техника деактивирована, так как есть связанные данные.",
                                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            // Если связей нет, удаляем физически
                            _databaseService.Context.Equipments.Remove(selected);
                            await _databaseService.Context.SaveChangesAsync();
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
                MessageBox.Show("Выберите технику для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Управление зависимостями
        /// </summary>
        private async void BtnDependencies_Click(object sender, RoutedEventArgs e)
        {
            if (dgEquipments.SelectedItem is Equipment selected)
            {
                _selectedEquipmentForDependencies = selected;
                DependenciesTitle.Text = $"Зависимости для: {selected.Name}";

                await LoadDependenciesAsync(selected.Id);

                DependenciesPopup.IsOpen = true;
            }
            else
            {
                MessageBox.Show("Выберите технику для управления зависимостями", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Загрузка зависимостей
        /// </summary>
        private async System.Threading.Tasks.Task LoadDependenciesAsync(string equipmentId)
        {
            try
            {
                var dependencies = await _databaseService.Context.EquipmentDependencies
                    .Include(ed => ed.DependentEquipment)
                    .Where(ed => ed.MainEquipmentId == equipmentId)
                    .ToListAsync();

                dgDependencies.ItemsSource = dependencies;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки зависимостей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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

        #endregion

        #region Обработчики popup редактирования

        /// <summary>
        /// Сохранение техники
        /// </summary>
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Наименование техники обязательно", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _editingEquipment.Name = txtName.Text.Trim();
                _editingEquipment.Category = string.IsNullOrWhiteSpace(txtCategory.Text) ? null : txtCategory.Text.Trim();
                _editingEquipment.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
                _editingEquipment.CanOrderMultiple = chkCanOrderMultiple.IsChecked == true;
                _editingEquipment.RequiresOperator = chkRequiresOperator.IsChecked == true;
                _editingEquipment.IsActive = chkIsActive.IsChecked == true;

                if (decimal.TryParse(txtHourlyCost.Text, out decimal hourlyCost))
                {
                    _editingEquipment.HourlyCost = hourlyCost;
                }
                else
                {
                    _editingEquipment.HourlyCost = null;
                }

                if (!_isEditMode)
                {
                    // Добавление нового
                    _editingEquipment.CreatedAt = DateTime.UtcNow;
                    await _databaseService.Context.Equipments.AddAsync(_editingEquipment);
                }
                else
                {
                    // Обновление существующего
                    _databaseService.Context.Equipments.Update(_editingEquipment);
                }

                await _databaseService.Context.SaveChangesAsync();

                EditPopup.IsOpen = false;
                await LoadDataAsync();
                await LoadDependentEquipmentsAsync();

                txtStatus.Text = _isEditMode ? "Техника обновлена" : "Техника добавлена";
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

        #region Обработчики окна зависимостей

        /// <summary>
        /// Добавление зависимости
        /// </summary>
        private void BtnAddDependency_Click(object sender, RoutedEventArgs e)
        {
            txtDependencyCount.Text = "1";
            chkIsMandatory.IsChecked = true;
            cmbDependentEquipment.SelectedIndex = -1;

            AddDependencyPopup.IsOpen = true;
        }

        /// <summary>
        /// Удаление зависимости
        /// </summary>
        private async void BtnDeleteDependency_Click(object sender, RoutedEventArgs e)
        {
            if (dgDependencies.SelectedItem is EquipmentDependency selected)
            {
                var result = MessageBox.Show(
                    "Удалить выбранную зависимость?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _databaseService.Context.EquipmentDependencies.Remove(selected);
                        await _databaseService.Context.SaveChangesAsync();

                        await LoadDependenciesAsync(_selectedEquipmentForDependencies.Id);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении зависимости: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Закрытие окна зависимостей
        /// </summary>
        private void BtnCloseDependencies_Click(object sender, RoutedEventArgs e)
        {
            DependenciesPopup.IsOpen = false;
        }

        #endregion

        #region Обработчики popup добавления зависимости

        /// <summary>
        /// Сохранение зависимости
        /// </summary>
        private async void BtnSaveDependency_Click(object sender, RoutedEventArgs e)
        {
            if (cmbDependentEquipment.SelectedItem == null)
            {
                MessageBox.Show("Выберите зависимую технику", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtDependencyCount.Text, out int count) || count <= 0)
            {
                MessageBox.Show("Введите корректное количество", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dependentEquipment = (Equipment)cmbDependentEquipment.SelectedItem;

            // Проверка, не пытаемся ли добавить зависимость на саму себя
            if (dependentEquipment.Id == _selectedEquipmentForDependencies.Id)
            {
                MessageBox.Show("Техника не может зависеть сама от себя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка на существование такой зависимости
            bool exists = await _databaseService.Context.EquipmentDependencies
                .AnyAsync(ed => ed.MainEquipmentId == _selectedEquipmentForDependencies.Id &&
                               ed.DependentEquipmentId == dependentEquipment.Id);

            if (exists)
            {
                MessageBox.Show("Такая зависимость уже существует", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dependency = new EquipmentDependency
                {
                    MainEquipmentId = _selectedEquipmentForDependencies.Id,
                    DependentEquipmentId = dependentEquipment.Id,
                    RequiredCount = count,
                    IsMandatory = chkIsMandatory.IsChecked == true,
                    CreatedAt = DateTime.UtcNow
                };

                await _databaseService.Context.EquipmentDependencies.AddAsync(dependency);
                await _databaseService.Context.SaveChangesAsync();

                AddDependencyPopup.IsOpen = false;
                await LoadDependenciesAsync(_selectedEquipmentForDependencies.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении зависимости: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отмена добавления зависимости
        /// </summary>
        private void BtnCancelDependency_Click(object sender, RoutedEventArgs e)
        {
            AddDependencyPopup.IsOpen = false;
        }

        #endregion
    }
}