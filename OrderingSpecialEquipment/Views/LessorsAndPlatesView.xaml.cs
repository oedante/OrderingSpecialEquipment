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
    /// Логика взаимодействия для LessorsAndPlatesView.xaml
    /// </summary>
    public partial class LessorsAndPlatesView : Window
    {
        #region Поля

        private readonly IDbContextFactory _contextFactory;
        private readonly IAuthorizationService _authorizationService;
        private LessorOrganization _editingLessor;
        private LicensePlate _editingPlate;
        private bool _isLessorEditMode;
        private bool _isPlateEditMode;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор окна арендодателей и госномеров
        /// </summary>
        public LessorsAndPlatesView()
        {
            InitializeComponent();

            _contextFactory = App.Services.GetRequiredService<IDbContextFactory>();
            _authorizationService = App.Services.GetRequiredService<IAuthorizationService>();

            // Проверка прав
            if (!_authorizationService.CanReadTable("LessorOrganizations"))
            {
                MessageBox.Show("У вас нет прав для просмотра этого справочника.",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            // Настройка кнопок в зависимости от прав
            btnAddLessor.IsEnabled = _authorizationService.CanWriteTable("LessorOrganizations");
            btnEditLessor.IsEnabled = _authorizationService.CanWriteTable("LessorOrganizations");
            btnDeleteLessor.IsEnabled = _authorizationService.CanWriteTable("LessorOrganizations");

            btnAddPlate.IsEnabled = _authorizationService.CanWriteTable("LicensePlates");
            btnEditPlate.IsEnabled = _authorizationService.CanWriteTable("LicensePlates");
            btnDeletePlate.IsEnabled = _authorizationService.CanWriteTable("LicensePlates");

            Loaded += LessorsAndPlatesView_Loaded;
        }

        #endregion

        #region Обработчики событий загрузки

        /// <summary>
        /// Загрузка окна
        /// </summary>
        private async void LessorsAndPlatesView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLessorsAsync();
            await LoadEquipmentForPlatesAsync();
        }

        /// <summary>
        /// Загрузка арендодателей
        /// </summary>
        private async System.Threading.Tasks.Task LoadLessorsAsync()
        {
            try
            {
                txtStatus.Text = "Загрузка арендодателей...";

                using var context = _contextFactory.CreateDbContext();

                IQueryable<LessorOrganization> query = context.LessorOrganizations;

                if (!chkShowInactive.IsChecked == true)
                {
                    query = query.Where(lo => lo.IsActive);
                }

                var lessors = await query
                    .OrderBy(lo => lo.Name)
                    .ToListAsync();

                dgLessors.ItemsSource = lessors;
                txtStatus.Text = $"Загружено арендодателей: {lessors.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки арендодателей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Ошибка загрузки";
            }
        }

        /// <summary>
        /// Загрузка техники для выпадающего списка госномеров
        /// </summary>
        private async System.Threading.Tasks.Task LoadEquipmentForPlatesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var equipments = await context.Equipments
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                cmbPlateEquipment.ItemsSource = equipments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки техники: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка госномеров для выбранного арендодателя
        /// </summary>
        private async System.Threading.Tasks.Task LoadPlatesAsync(string lessorId)
        {
            try
            {
                if (string.IsNullOrEmpty(lessorId))
                {
                    dgPlates.ItemsSource = null;
                    txtPlatesTitle.Text = "Государственные номера";
                    return;
                }

                using var context = _contextFactory.CreateDbContext();

                var lessor = await context.LessorOrganizations
                    .FirstOrDefaultAsync(lo => lo.Id == lessorId);

                txtPlatesTitle.Text = $"Госномера организации: {lessor?.Name}";

                IQueryable<LicensePlate> query = context.LicensePlates
                    .Include(lp => lp.Equipment)
                    .Where(lp => lp.LessorOrganizationId == lessorId);

                if (!chkShowInactive.IsChecked == true)
                {
                    query = query.Where(lp => lp.IsActive);
                }

                var plates = await query
                    .OrderBy(lp => lp.PlateNumber)
                    .ToListAsync();

                dgPlates.ItemsSource = plates;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки госномеров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Обработчики событий арендодателей

        /// <summary>
        /// Выбор арендодателя
        /// </summary>
        private async void DgLessors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgLessors.SelectedItem is LessorOrganization selected)
            {
                await LoadPlatesAsync(selected.Id);
            }
            else
            {
                await LoadPlatesAsync(null);
            }
        }

        /// <summary>
        /// Добавление арендодателя
        /// </summary>
        private void BtnAddLessor_Click(object sender, RoutedEventArgs e)
        {
            _editingLessor = new LessorOrganization
            {
                IsActive = true
            };
            _isLessorEditMode = false;

            LessorPopupTitle.Text = "Добавление арендодателя";
            txtLessorName.Text = "";
            txtLessorINN.Text = "";
            txtLessorContact.Text = "";
            txtLessorPhone.Text = "";
            txtLessorEmail.Text = "";
            txtLessorAddress.Text = "";
            chkLessorIsActive.IsChecked = true;

            LessorEditPopup.IsOpen = true;
            txtLessorName.Focus();
        }

        /// <summary>
        /// Редактирование арендодателя
        /// </summary>
        private void BtnEditLessor_Click(object sender, RoutedEventArgs e)
        {
            if (dgLessors.SelectedItem is LessorOrganization selected)
            {
                _editingLessor = selected;
                _isLessorEditMode = true;

                LessorPopupTitle.Text = "Редактирование арендодателя";
                txtLessorName.Text = selected.Name;
                txtLessorINN.Text = selected.INN;
                txtLessorContact.Text = selected.ContactPerson;
                txtLessorPhone.Text = selected.Phone;
                txtLessorEmail.Text = selected.Email;
                txtLessorAddress.Text = selected.Address;
                chkLessorIsActive.IsChecked = selected.IsActive;

                LessorEditPopup.IsOpen = true;
                txtLessorName.Focus();
            }
            else
            {
                MessageBox.Show("Выберите арендодателя для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Двойной клик по арендодателю
        /// </summary>
        private void DgLessors_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_authorizationService.CanWriteTable("LessorOrganizations"))
            {
                BtnEditLessor_Click(sender, e);
            }
        }

        /// <summary>
        /// Удаление арендодателя
        /// </summary>
        private async void BtnDeleteLessor_Click(object sender, RoutedEventArgs e)
        {
            if (dgLessors.SelectedItem is LessorOrganization selected)
            {
                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить организацию '{selected.Name}'?\n" +
                    "Это также удалит все связанные госномера!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        txtStatus.Text = "Удаление...";

                        using var context = _contextFactory.CreateDbContext();

                        // Удаляем связанные госномера
                        var plates = await context.LicensePlates
                            .Where(lp => lp.LessorOrganizationId == selected.Id)
                            .ToListAsync();

                        if (plates.Any())
                        {
                            context.LicensePlates.RemoveRange(plates);
                        }

                        context.LessorOrganizations.Remove(selected);
                        await context.SaveChangesAsync();

                        await LoadLessorsAsync();
                        await LoadPlatesAsync(null);

                        txtStatus.Text = "Организация удалена";
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
                MessageBox.Show("Выберите организацию для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Сохранение арендодателя
        /// </summary>
        private async void BtnSaveLessor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLessorName.Text))
            {
                MessageBox.Show("Наименование организации обязательно", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = _contextFactory.CreateDbContext();

                _editingLessor.Name = txtLessorName.Text.Trim();
                _editingLessor.INN = string.IsNullOrWhiteSpace(txtLessorINN.Text) ? null : txtLessorINN.Text.Trim();
                _editingLessor.ContactPerson = string.IsNullOrWhiteSpace(txtLessorContact.Text) ? null : txtLessorContact.Text.Trim();
                _editingLessor.Phone = string.IsNullOrWhiteSpace(txtLessorPhone.Text) ? null : txtLessorPhone.Text.Trim();
                _editingLessor.Email = string.IsNullOrWhiteSpace(txtLessorEmail.Text) ? null : txtLessorEmail.Text.Trim();
                _editingLessor.Address = string.IsNullOrWhiteSpace(txtLessorAddress.Text) ? null : txtLessorAddress.Text.Trim();
                _editingLessor.IsActive = chkLessorIsActive.IsChecked == true;

                if (!_isLessorEditMode)
                {
                    // Добавление нового
                    _editingLessor.CreatedAt = DateTime.UtcNow;
                    await context.LessorOrganizations.AddAsync(_editingLessor);
                }
                else
                {
                    // Обновление существующего
                    context.LessorOrganizations.Update(_editingLessor);
                }

                await context.SaveChangesAsync();

                LessorEditPopup.IsOpen = false;
                await LoadLessorsAsync();

                txtStatus.Text = _isLessorEditMode ? "Организация обновлена" : "Организация добавлена";
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("UQ_LessorOrganizations_INN") == true)
                {
                    MessageBox.Show("Организация с таким ИНН уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отмена редактирования арендодателя
        /// </summary>
        private void BtnCancelLessor_Click(object sender, RoutedEventArgs e)
        {
            LessorEditPopup.IsOpen = false;
        }

        #endregion

        #region Обработчики событий госномеров

        /// <summary>
        /// Добавление госномера
        /// </summary>
        private void BtnAddPlate_Click(object sender, RoutedEventArgs e)
        {
            if (dgLessors.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите организацию-арендодателя", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _editingPlate = new LicensePlate
            {
                LessorOrganizationId = ((LessorOrganization)dgLessors.SelectedItem).Id,
                IsActive = true
            };
            _isPlateEditMode = false;

            PlatePopupTitle.Text = "Добавление госномера";
            txtPlateNumber.Text = "";
            cmbPlateEquipment.SelectedIndex = -1;
            txtPlateBrand.Text = "";
            txtPlateYear.Text = "";
            txtPlateCapacity.Text = "";
            txtPlateVIN.Text = "";
            chkPlateIsActive.IsChecked = true;

            PlateEditPopup.IsOpen = true;
            txtPlateNumber.Focus();
        }

        /// <summary>
        /// Редактирование госномера
        /// </summary>
        private void BtnEditPlate_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlates.SelectedItem is LicensePlate selected)
            {
                _editingPlate = selected;
                _isPlateEditMode = true;

                PlatePopupTitle.Text = "Редактирование госномера";
                txtPlateNumber.Text = selected.PlateNumber;
                cmbPlateEquipment.SelectedValue = selected.EquipmentId;
                txtPlateBrand.Text = selected.Brand;
                txtPlateYear.Text = selected.Year?.ToString();
                txtPlateCapacity.Text = selected.Capacity;
                txtPlateVIN.Text = selected.VIN;
                chkPlateIsActive.IsChecked = selected.IsActive;

                PlateEditPopup.IsOpen = true;
                txtPlateNumber.Focus();
            }
            else
            {
                MessageBox.Show("Выберите госномер для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Двойной клик по госномеру
        /// </summary>
        private void DgPlates_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_authorizationService.CanWriteTable("LicensePlates"))
            {
                BtnEditPlate_Click(sender, e);
            }
        }

        /// <summary>
        /// Удаление госномера
        /// </summary>
        private async void BtnDeletePlate_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlates.SelectedItem is LicensePlate selected)
            {
                var result = MessageBox.Show(
                    $"Удалить госномер '{selected.PlateNumber}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        context.LicensePlates.Remove(selected);
                        await context.SaveChangesAsync();

                        if (dgLessors.SelectedItem is LessorOrganization lessor)
                        {
                            await LoadPlatesAsync(lessor.Id);
                        }

                        txtStatus.Text = "Госномер удален";
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
                MessageBox.Show("Выберите госномер для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Сохранение госномера
        /// </summary>
        private async void BtnSavePlate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlateNumber.Text))
            {
                MessageBox.Show("Государственный номер обязателен", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbPlateEquipment.SelectedValue == null)
            {
                MessageBox.Show("Выберите технику", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = _contextFactory.CreateDbContext();

                _editingPlate.PlateNumber = txtPlateNumber.Text.Trim().ToUpper();
                _editingPlate.EquipmentId = cmbPlateEquipment.SelectedValue.ToString();
                _editingPlate.Brand = string.IsNullOrWhiteSpace(txtPlateBrand.Text) ? null : txtPlateBrand.Text.Trim();

                if (int.TryParse(txtPlateYear.Text, out int year))
                {
                    _editingPlate.Year = year;
                }
                else
                {
                    _editingPlate.Year = null;
                }

                _editingPlate.Capacity = string.IsNullOrWhiteSpace(txtPlateCapacity.Text) ? null : txtPlateCapacity.Text.Trim();
                _editingPlate.VIN = string.IsNullOrWhiteSpace(txtPlateVIN.Text) ? null : txtPlateVIN.Text.Trim();
                _editingPlate.IsActive = chkPlateIsActive.IsChecked == true;

                if (!_isPlateEditMode)
                {
                    // Добавление нового
                    _editingPlate.CreatedAt = DateTime.UtcNow;
                    await context.LicensePlates.AddAsync(_editingPlate);
                }
                else
                {
                    // Обновление существующего
                    context.LicensePlates.Update(_editingPlate);
                }

                await context.SaveChangesAsync();

                PlateEditPopup.IsOpen = false;

                if (dgLessors.SelectedItem is LessorOrganization lessor)
                {
                    await LoadPlatesAsync(lessor.Id);
                }

                txtStatus.Text = _isPlateEditMode ? "Госномер обновлен" : "Госномер добавлен";
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("UQ_LicensePlates_PlateNumber") == true)
                {
                    MessageBox.Show("Госномер с таким номером уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отмена редактирования госномера
        /// </summary>
        private void BtnCancelPlate_Click(object sender, RoutedEventArgs e)
        {
            PlateEditPopup.IsOpen = false;
        }

        #endregion

        /// <summary>
        /// Обновление данных
        /// </summary>
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadLessorsAsync();
            await LoadEquipmentForPlatesAsync();
        }

        /// <summary>
        /// Изменение фильтра показа неактивных
        /// </summary>
        private async void ChkShowInactive_Changed(object sender, RoutedEventArgs e)
        {
            await LoadLessorsAsync();
            if (dgLessors.SelectedItem is LessorOrganization lessor)
            {
                await LoadPlatesAsync(lessor.Id);
            }
        }
    }
}