using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Логика взаимодействия для ShiftRequestsReportView.xaml
    /// </summary>
    public partial class ShiftRequestsReportView : Window
    {
        #region Поля

        private readonly IDatabaseService _databaseService;
        private readonly IAuthorizationService _authorizationService;
        private List<ReportItem> _reportData;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор окна отчета по заявкам
        /// </summary>
        public ShiftRequestsReportView()
        {
            InitializeComponent();

            _databaseService = App.Services.GetRequiredService<IDatabaseService>();
            _authorizationService = App.Services.GetRequiredService<IAuthorizationService>();

            // Проверка прав
            if (!_authorizationService.HasSpecialPermission("ViewReports"))
            {
                MessageBox.Show("У вас нет прав для просмотра отчетов.",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            Loaded += ShiftRequestsReportView_Loaded;
        }

        #endregion

        #region Загрузка

        private async void ShiftRequestsReportView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDepartmentsAsync();
            await LoadEquipmentsAsync();

            // Устанавливаем период по умолчанию - текущий месяц
            var now = DateTime.Now;
            dpDateFrom.SelectedDate = new DateTime(now.Year, now.Month, 1);
            dpDateTo.SelectedDate = now;
        }

        /// <summary>
        /// Загрузка отделов
        /// </summary>
        private async System.Threading.Tasks.Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _authorizationService.GetAccessibleDepartmentsAsync();

                departments.Insert(0, new Department { Id = "", Name = "Все отделы" });
                cmbDepartment.ItemsSource = departments;
                cmbDepartment.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка техники для фильтра
        /// </summary>
        private async System.Threading.Tasks.Task LoadEquipmentsAsync()
        {
            try
            {
                var equipments = await _databaseService.Context.Equipments
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                equipments.Insert(0, new Equipment { Id = "", Name = "Вся техника" });
                cmbEquipment.ItemsSource = equipments;
                cmbEquipment.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки техники: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Изменение режима группировки
        /// </summary>
        private void ChkGroupByEquipment_Changed(object sender, RoutedEventArgs e)
        {
            if (_reportData != null && _reportData.Any())
            {
                DisplayReport();
            }
        }

        /// <summary>
        /// Формирование отчета
        /// </summary>
        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            await GenerateReportAsync();
        }

        /// <summary>
        /// Экспорт в Excel
        /// </summary>
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel();
        }

        #endregion

        #region Генерация отчета

        /// <summary>
        /// Генерация отчета
        /// </summary>
        private async System.Threading.Tasks.Task GenerateReportAsync()
        {
            try
            {
                if (dpDateFrom.SelectedDate == null || dpDateTo.SelectedDate == null)
                {
                    MessageBox.Show("Выберите период", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                txtStatus.Text = "Формирование отчета...";

                DateTime dateFrom = dpDateFrom.SelectedDate.Value.Date;
                DateTime dateTo = dpDateTo.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);

                string departmentId = cmbDepartment.SelectedValue as string;
                string equipmentId = cmbEquipment.SelectedValue as string;

                // Запрос заявок
                var query = _databaseService.Context.ShiftRequests
                    .Include(sr => sr.Equipment)
                    .Include(sr => sr.Warehouse)
                    .Include(sr => sr.Area)
                    .Include(sr => sr.LicensePlate)
                    .Include(sr => sr.LessorOrganization)
                    .Include(sr => sr.Department)
                    .Include(sr => sr.CreatedByUser)
                    .Where(sr => sr.Date >= dateFrom && sr.Date <= dateTo);

                if (!string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(sr => sr.DepartmentId == departmentId);
                }

                if (!string.IsNullOrEmpty(equipmentId))
                {
                    query = query.Where(sr => sr.EquipmentId == equipmentId);
                }

                if (chkShowOnlyWorked.IsChecked == true)
                {
                    query = query.Where(sr => sr.IsWorked == true);
                }

                var requests = await query
                    .OrderBy(sr => sr.Date)
                    .ThenBy(sr => sr.Shift)
                    .ThenBy(sr => sr.Department.Name)
                    .ThenBy(sr => sr.Equipment.Name)
                    .ToListAsync();

                if (!requests.Any())
                {
                    MessageBox.Show("Нет заявок за выбранный период", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    txtStatus.Text = "Нет данных";
                    dgReport.ItemsSource = null;
                    _reportData = null;
                    UpdateTotals();
                    return;
                }

                // Преобразуем в элементы отчета
                _reportData = requests.Select(r => new ReportItem
                {
                    Date = r.Date,
                    Shift = r.Shift,
                    ShiftName = r.Shift == 0 ? "Дневная" : "Ночная",
                    DepartmentName = r.Department?.Name,
                    WarehouseName = r.Warehouse?.Name,
                    AreaName = r.Area?.Name,
                    EquipmentName = r.Equipment?.Name,
                    PlateNumber = r.LicensePlate?.PlateNumber,
                    LessorName = r.LessorOrganization?.Name,
                    RequestedCount = r.RequestedCount,
                    WorkedHours = r.WorkedHours ?? 0,
                    ActualCost = r.ActualCost ?? 0,
                    IsWorked = r.IsWorked,
                    Comment = r.Comment,
                    CreatedByUser = r.CreatedByUser?.FullName,
                    EquipmentId = r.EquipmentId
                }).ToList();

                DisplayReport();

                txtStatus.Text = $"Отчет сформирован. Записей: {_reportData.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Ошибка формирования";
            }
        }

        /// <summary>
        /// Отображение отчета с учетом группировки
        /// </summary>
        private void DisplayReport()
        {
            if (_reportData == null) return;

            if (chkGroupByEquipment.IsChecked == true)
            {
                // Группируем по технике
                var grouped = _reportData
                    .GroupBy(r => r.EquipmentName)
                    .Select(g => new ReportItem
                    {
                        EquipmentName = g.Key,
                        WorkedHours = g.Sum(r => r.WorkedHours),
                        ActualCost = g.Sum(r => r.ActualCost),
                        RequestedCount = g.Sum(r => r.RequestedCount),
                        IsGrouped = true,
                        Items = g.ToList()
                    })
                    .OrderBy(g => g.EquipmentName)
                    .ToList();

                dgReport.ItemsSource = grouped;
            }
            else
            {
                dgReport.ItemsSource = _reportData;
            }

            UpdateTotals();
        }

        /// <summary>
        /// Обновление итогов
        /// </summary>
        private void UpdateTotals()
        {
            if (_reportData == null || !_reportData.Any())
            {
                txtTotalHours.Text = "0";
                txtTotalCost.Text = "0 ₽";
                return;
            }

            decimal totalHours = _reportData.Sum(r => r.WorkedHours);
            decimal totalCost = _reportData.Sum(r => r.ActualCost);

            txtTotalHours.Text = totalHours.ToString("N2");
            txtTotalCost.Text = totalCost.ToString("N2") + " ₽";
        }

        /// <summary>
        /// Экспорт в Excel
        /// </summary>
        private void ExportToExcel()
        {
            if (_reportData == null || !_reportData.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var columns = new List<(string Header, Func<ReportItem, object> Value)>();

            if (chkGroupByEquipment.IsChecked == true)
            {
                // Для группировки экспортируем детальные данные
                var allItems = _reportData.SelectMany(r => r.Items ?? new List<ReportItem> { r }).ToList();

                columns = new List<(string, Func<ReportItem, object>)>
                {
                    ("Дата", r => r.Date),
                    ("Смена", r => r.ShiftName),
                    ("Отдел", r => r.DepartmentName),
                    ("Склад", r => r.WarehouseName),
                    ("Территория", r => r.AreaName),
                    ("Техника", r => r.EquipmentName),
                    ("Госномер", r => r.PlateNumber),
                    ("Арендодатель", r => r.LessorName),
                    ("Кол-во", r => r.RequestedCount),
                    ("Часы", r => r.WorkedHours),
                    ("Стоимость", r => r.ActualCost),
                    ("Отработано", r => r.IsWorked ? "Да" : "Нет"),
                    ("Комментарий", r => r.Comment),
                    ("Создал", r => r.CreatedByUser)
                };

                string departmentText = cmbDepartment.SelectedItem is Department dept ? dept.Name : "Все отделы";
                string periodText = $"{dpDateFrom.SelectedDate:dd.MM.yyyy}-{dpDateTo.SelectedDate:dd.MM.yyyy}";

                ExcelExporter.ExportToExcel(
                    allItems,
                    columns,
                    $"Заявки_{departmentText}_{periodText}");
            }
            else
            {
                columns = new List<(string, Func<ReportItem, object>)>
                {
                    ("Дата", r => r.Date),
                    ("Смена", r => r.ShiftName),
                    ("Отдел", r => r.DepartmentName),
                    ("Склад", r => r.WarehouseName),
                    ("Территория", r => r.AreaName),
                    ("Техника", r => r.EquipmentName),
                    ("Госномер", r => r.PlateNumber),
                    ("Арендодатель", r => r.LessorName),
                    ("Кол-во", r => r.RequestedCount),
                    ("Часы", r => r.WorkedHours),
                    ("Стоимость", r => r.ActualCost),
                    ("Отработано", r => r.IsWorked ? "Да" : "Нет"),
                    ("Комментарий", r => r.Comment),
                    ("Создал", r => r.CreatedByUser)
                };

                string departmentText = cmbDepartment.SelectedItem is Department dept ? dept.Name : "Все отделы";
                string periodText = $"{dpDateFrom.SelectedDate:dd.MM.yyyy}-{dpDateTo.SelectedDate:dd.MM.yyyy}";

                ExcelExporter.ExportToExcel(
                    _reportData,
                    columns,
                    $"Заявки_{departmentText}_{periodText}");
            }
        }

        #endregion

        #region Вспомогательный класс для данных отчета

        /// <summary>
        /// Элемент отчета
        /// </summary>
        public class ReportItem
        {
            public DateTime Date { get; set; }
            public int Shift { get; set; }
            public string ShiftName { get; set; }
            public string DepartmentName { get; set; }
            public string WarehouseName { get; set; }
            public string AreaName { get; set; }
            public string EquipmentName { get; set; }
            public string PlateNumber { get; set; }
            public string LessorName { get; set; }
            public int RequestedCount { get; set; }
            public decimal WorkedHours { get; set; }
            public decimal ActualCost { get; set; }
            public bool IsWorked { get; set; }
            public string Comment { get; set; }
            public string CreatedByUser { get; set; }
            public string EquipmentId { get; set; }
            public bool IsGrouped { get; set; }
            public List<ReportItem> Items { get; set; }
        }

        #endregion
    }
}