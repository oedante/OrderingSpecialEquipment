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
    /// Логика взаимодействия для TransportProgramView.xaml
    /// </summary>
    public partial class TransportProgramView : Window
    {
        #region Поля

        private readonly IDbContextFactory _contextFactory;
        private readonly IAuthorizationService _authorizationService;
        private TransportProgram _editingProgram;
        private bool _isEditMode;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор окна транспортной программы
        /// </summary>
        public TransportProgramView()
        {
            InitializeComponent();

            _contextFactory = App.Services.GetRequiredService<IDbContextFactory>();
            _authorizationService = App.Services.GetRequiredService<IAuthorizationService>();

            // Проверка прав
            if (!_authorizationService.CanReadTable("TransportProgram"))
            {
                MessageBox.Show("У вас нет прав для просмотра этого справочника.",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            // Настройка кнопок в зависимости от прав
            btnAdd.IsEnabled = _authorizationService.CanWriteTable("TransportProgram");
            btnEdit.IsEnabled = _authorizationService.CanWriteTable("TransportProgram");
            btnDelete.IsEnabled = _authorizationService.CanWriteTable("TransportProgram");
            btnCopy.IsEnabled = _authorizationService.CanWriteTable("TransportProgram");

            Loaded += TransportProgramView_Loaded;
        }

        #endregion

        #region Обработчики событий загрузки

        /// <summary>
        /// Загрузка окна
        /// </summary>
        private async void TransportProgramView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDepartmentsAsync();
            await LoadYearsAsync();
            await LoadEquipmentsAsync();
            await LoadDataAsync();
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

                // Для редактора и копирования
                var activeDepartments = departments.Where(d => !string.IsNullOrEmpty(d.Id)).ToList();
                cmbDepartment.ItemsSource = activeDepartments;
                cmbCopyDepartment.ItemsSource = activeDepartments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка годов для фильтра
        /// </summary>
        private async System.Threading.Tasks.Task LoadYearsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var years = await context.TransportProgram
                    .Select(tp => tp.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                var currentYear = DateTime.Now.Year;
                if (!years.Contains(currentYear))
                {
                    years.Insert(0, currentYear);
                }

                cmbYearFilter.ItemsSource = years;
                cmbYearFilter.SelectedItem = currentYear;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки годов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка техники для выпадающего списка
        /// </summary>
        private async System.Threading.Tasks.Task LoadEquipmentsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var equipments = await context.Equipments
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                cmbEquipment.ItemsSource = equipments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки техники: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка данных транспортной программы
        /// </summary>
        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                txtStatus.Text = "Загрузка...";

                using var context = _contextFactory.CreateDbContext();

                IQueryable<TransportProgram> query = context.TransportProgram
                    .Include(tp => tp.Department)
                    .Include(tp => tp.Equipment);

                string selectedDepartmentId = cmbDepartmentFilter.SelectedValue as string;
                if (!string.IsNullOrEmpty(selectedDepartmentId))
                {
                    query = query.Where(tp => tp.DepartmentId == selectedDepartmentId);
                }

                if (cmbYearFilter.SelectedItem != null)
                {
                    int selectedYear = (int)cmbYearFilter.SelectedItem;
                    query = query.Where(tp => tp.Year == selectedYear);
                }

                var programs = await query
                    .OrderBy(tp => tp.Department.Name)
                    .ThenBy(tp => tp.Equipment.Name)
                    .ToListAsync();

                dgTransportProgram.ItemsSource = programs;
                txtStatus.Text = $"Загружено записей: {programs.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Ошибка загрузки";
            }
        }

        #endregion

        #region Обработчики фильтров

        private async void CmbDepartmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async void CmbYearFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadDataAsync();
        }

        #endregion

        #region Обработчики кнопок

        /// <summary>
        /// Добавление записи
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            _editingProgram = new TransportProgram();
            _isEditMode = false;

            PopupTitle.Text = "Добавление записи";

            // Сброс полей
            cmbDepartment.SelectedIndex = -1;
            cmbYear.SelectedIndex = -1;
            cmbEquipment.SelectedIndex = -1;
            txtHourlyCost.Text = "";
            txtJan.Text = "0";
            txtFeb.Text = "0";
            txtMar.Text = "0";
            txtApr.Text = "0";
            txtMay.Text = "0";
            txtJun.Text = "0";
            txtJul.Text = "0";
            txtAug.Text = "0";
            txtSep.Text = "0";
            txtOct.Text = "0";
            txtNov.Text = "0";
            txtDec.Text = "0";

            EditPopup.IsOpen = true;
        }

        /// <summary>
        /// Редактирование записи
        /// </summary>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgTransportProgram.SelectedItem is TransportProgram selected)
            {
                _editingProgram = selected;
                _isEditMode = true;

                PopupTitle.Text = "Редактирование записи";

                cmbDepartment.SelectedValue = selected.DepartmentId;
                cmbYear.SelectedItem = selected.Year;
                cmbEquipment.SelectedValue = selected.EquipmentId;
                txtHourlyCost.Text = selected.HourlyCost.ToString();
                txtJan.Text = selected.JanuaryHours.ToString();
                txtFeb.Text = selected.FebruaryHours.ToString();
                txtMar.Text = selected.MarchHours.ToString();
                txtApr.Text = selected.AprilHours.ToString();
                txtMay.Text = selected.MayHours.ToString();
                txtJun.Text = selected.JuneHours.ToString();
                txtJul.Text = selected.JulyHours.ToString();
                txtAug.Text = selected.AugustHours.ToString();
                txtSep.Text = selected.SeptemberHours.ToString();
                txtOct.Text = selected.OctoberHours.ToString();
                txtNov.Text = selected.NovemberHours.ToString();
                txtDec.Text = selected.DecemberHours.ToString();

                EditPopup.IsOpen = true;
            }
            else
            {
                MessageBox.Show("Выберите запись для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Двойной клик по строке
        /// </summary>
        private void DgTransportProgram_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_authorizationService.CanWriteTable("TransportProgram"))
            {
                BtnEdit_Click(sender, e);
            }
        }

        /// <summary>
        /// Удаление записи
        /// </summary>
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgTransportProgram.SelectedItem is TransportProgram selected)
            {
                var result = MessageBox.Show(
                    $"Удалить запись для {selected.Equipment?.Name} за {selected.Year} год?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        context.TransportProgram.Remove(selected);
                        await context.SaveChangesAsync();

                        await LoadDataAsync();
                        await LoadYearsAsync();

                        txtStatus.Text = "Запись удалена";
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
                MessageBox.Show("Выберите запись для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Копирование из года
        /// </summary>
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!_authorizationService.CanWriteTable("TransportProgram"))
                return;

            cmbCopyDepartment.SelectedIndex = -1;
            cmbCopyFromYear.SelectedItem = DateTime.Now.Year;
            cmbCopyToYear.SelectedItem = DateTime.Now.Year + 1;

            CopyPopup.IsOpen = true;
        }

        /// <summary>
        /// Выполнение копирования
        /// </summary>
        private async void BtnCopyExecute_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCopyDepartment.SelectedValue == null)
            {
                MessageBox.Show("Выберите отдел", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbCopyFromYear.SelectedItem == null || cmbCopyToYear.SelectedItem == null)
            {
                MessageBox.Show("Выберите годы", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string departmentId = cmbCopyDepartment.SelectedValue.ToString();
            int fromYear = (int)cmbCopyFromYear.SelectedItem;
            int toYear = (int)cmbCopyToYear.SelectedItem;

            if (fromYear == toYear)
            {
                MessageBox.Show("Годы должны быть разными", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                txtStatus.Text = "Копирование...";

                using var context = _contextFactory.CreateDbContext();

                // Получаем записи из исходного года
                var sourcePrograms = await context.TransportProgram
                    .Where(tp => tp.DepartmentId == departmentId && tp.Year == fromYear)
                    .ToListAsync();

                if (!sourcePrograms.Any())
                {
                    MessageBox.Show("В исходном году нет записей", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CopyPopup.IsOpen = false;
                    return;
                }

                // Проверяем, нет ли уже записей в целевом году
                var existingPrograms = await context.TransportProgram
                    .Where(tp => tp.DepartmentId == departmentId && tp.Year == toYear)
                    .ToListAsync();

                if (existingPrograms.Any())
                {
                    var overwriteResult = MessageBox.Show(
                        "В целевом году уже есть записи. Перезаписать их?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (overwriteResult == MessageBoxResult.Yes)
                    {
                        context.TransportProgram.RemoveRange(existingPrograms);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        CopyPopup.IsOpen = false;
                        return;
                    }
                }

                // Копируем записи
                var newPrograms = new List<TransportProgram>();
                foreach (var source in sourcePrograms)
                {
                    var newProgram = new TransportProgram
                    {
                        DepartmentId = source.DepartmentId,
                        Year = toYear,
                        EquipmentId = source.EquipmentId,
                        HourlyCost = source.HourlyCost,
                        JanuaryHours = source.JanuaryHours,
                        FebruaryHours = source.FebruaryHours,
                        MarchHours = source.MarchHours,
                        AprilHours = source.AprilHours,
                        MayHours = source.MayHours,
                        JuneHours = source.JuneHours,
                        JulyHours = source.JulyHours,
                        AugustHours = source.AugustHours,
                        SeptemberHours = source.SeptemberHours,
                        OctoberHours = source.OctoberHours,
                        NovemberHours = source.NovemberHours,
                        DecemberHours = source.DecemberHours,
                        CreatedAt = DateTime.UtcNow
                    };
                    newPrograms.Add(newProgram);
                }

                await context.TransportProgram.AddRangeAsync(newPrograms);
                await context.SaveChangesAsync();

                CopyPopup.IsOpen = false;
                await LoadYearsAsync();
                cmbYearFilter.SelectedItem = toYear;
                await LoadDataAsync();

                txtStatus.Text = $"Скопировано записей: {newPrograms.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Ошибка копирования";
            }
        }

        /// <summary>
        /// Отмена копирования
        /// </summary>
        private void BtnCopyCancel_Click(object sender, RoutedEventArgs e)
        {
            CopyPopup.IsOpen = false;
        }

        /// <summary>
        /// Сохранение записи
        /// </summary>
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbDepartment.SelectedValue == null)
            {
                MessageBox.Show("Выберите отдел", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbYear.SelectedItem == null)
            {
                MessageBox.Show("Выберите год", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbEquipment.SelectedValue == null)
            {
                MessageBox.Show("Выберите технику", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtHourlyCost.Text, out decimal hourlyCost) || hourlyCost <= 0)
            {
                MessageBox.Show("Введите корректную стоимость часа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = _contextFactory.CreateDbContext();

                _editingProgram.DepartmentId = cmbDepartment.SelectedValue.ToString();
                _editingProgram.Year = (int)cmbYear.SelectedItem;
                _editingProgram.EquipmentId = cmbEquipment.SelectedValue.ToString();
                _editingProgram.HourlyCost = hourlyCost;

                // Часы по месяцам
                _editingProgram.JanuaryHours = ParseDecimal(txtJan.Text);
                _editingProgram.FebruaryHours = ParseDecimal(txtFeb.Text);
                _editingProgram.MarchHours = ParseDecimal(txtMar.Text);
                _editingProgram.AprilHours = ParseDecimal(txtApr.Text);
                _editingProgram.MayHours = ParseDecimal(txtMay.Text);
                _editingProgram.JuneHours = ParseDecimal(txtJun.Text);
                _editingProgram.JulyHours = ParseDecimal(txtJul.Text);
                _editingProgram.AugustHours = ParseDecimal(txtAug.Text);
                _editingProgram.SeptemberHours = ParseDecimal(txtSep.Text);
                _editingProgram.OctoberHours = ParseDecimal(txtOct.Text);
                _editingProgram.NovemberHours = ParseDecimal(txtNov.Text);
                _editingProgram.DecemberHours = ParseDecimal(txtDec.Text);

                if (!_isEditMode)
                {
                    // Проверка на дубликат
                    bool exists = await context.TransportProgram
                        .AnyAsync(tp => tp.DepartmentId == _editingProgram.DepartmentId &&
                                       tp.Year == _editingProgram.Year &&
                                       tp.EquipmentId == _editingProgram.EquipmentId);

                    if (exists)
                    {
                        MessageBox.Show("Запись для данного отдела, года и техники уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _editingProgram.CreatedAt = DateTime.UtcNow;
                    await context.TransportProgram.AddAsync(_editingProgram);
                }
                else
                {
                    context.TransportProgram.Update(_editingProgram);
                }

                await context.SaveChangesAsync();

                EditPopup.IsOpen = false;
                await LoadYearsAsync();
                await LoadDataAsync();

                txtStatus.Text = _isEditMode ? "Запись обновлена" : "Запись добавлена";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Парсинг десятичного числа из строки
        /// </summary>
        private decimal ParseDecimal(string text)
        {
            if (decimal.TryParse(text, out decimal result))
                return result;
            return 0;
        }

        /// <summary>
        /// Отмена редактирования
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            EditPopup.IsOpen = false;
        }

        /// <summary>
        /// Обновление данных
        /// </summary>
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDepartmentsAsync();
            await LoadYearsAsync();
            await LoadEquipmentsAsync();
            await LoadDataAsync();
        }

        #endregion
    }
}