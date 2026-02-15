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
    /// Логика взаимодействия для TransportProgramReportView.xaml
    /// </summary>
    public partial class TransportProgramReportView : Window
    {
        #region Поля

        private readonly IDatabaseService _databaseService;
        private readonly IAuthorizationService _authorizationService;
        private List<ReportItem> _reportData;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор окна отчета по транспортной программе
        /// </summary>
        public TransportProgramReportView()
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

            Loaded += TransportProgramReportView_Loaded;
        }

        #endregion

        #region Загрузка

        private async void TransportProgramReportView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDepartmentsAsync();
            await LoadYearsAsync();

            // Устанавливаем текущий год и месяц
            cmbYear.SelectedItem = DateTime.Now.Year;
            cmbMonth.SelectedIndex = DateTime.Now.Month - 1;
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
        /// Загрузка годов
        /// </summary>
        private async System.Threading.Tasks.Task LoadYearsAsync()
        {
            try
            {
                var years = await _databaseService.Context.TransportProgram
                    .Select(tp => tp.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                var currentYear = DateTime.Now.Year;
                if (!years.Contains(currentYear))
                {
                    years.Insert(0, currentYear);
                }

                cmbYear.ItemsSource = years;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки годов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Изменение типа периода
        /// </summary>
        private void CmbPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPeriod.SelectedItem is ComboBoxItem selected)
            {
                string periodType = selected.Tag.ToString();

                panelMonth.Visibility = periodType == "Month" ? Visibility.Visible : Visibility.Collapsed;
                panelQuarter.Visibility = periodType == "Quarter" ? Visibility.Visible : Visibility.Collapsed;

                lblYear.Visibility = periodType == "Year" ? Visibility.Collapsed : Visibility.Visible;
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
                txtStatus.Text = "Формирование отчета...";

                string departmentId = cmbDepartment.SelectedValue as string;
                int year = (int)cmbYear.SelectedItem;
                string periodType = (cmbPeriod.SelectedItem as ComboBoxItem)?.Tag.ToString();

                // Получаем данные транспортной программы
                var transportQuery = _databaseService.Context.TransportProgram
                    .Include(tp => tp.Equipment)
                    .Include(tp => tp.Department)
                    .Where(tp => tp.Year == year);

                if (!string.IsNullOrEmpty(departmentId))
                {
                    transportQuery = transportQuery.Where(tp => tp.DepartmentId == departmentId);
                }

                var transportData = await transportQuery.ToListAsync();

                if (!transportData.Any())
                {
                    MessageBox.Show("Нет данных транспортной программы за выбранный период", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    txtStatus.Text = "Нет данных";
                    return;
                }

                // Формируем отчет
                _reportData = new List<ReportItem>();

                foreach (var tp in transportData)
                {
                    decimal planHours = 0;
                    decimal planCost = 0;

                    // Расчет плановых показателей в зависимости от периода
                    switch (periodType)
                    {
                        case "Month":
                            int month = int.Parse((cmbMonth.SelectedItem as ComboBoxItem).Tag.ToString());
                            planHours = tp.GetHoursByMonth(month);
                            planCost = planHours * tp.HourlyCost;
                            break;

                        case "Quarter":
                            int quarter = int.Parse((cmbQuarter.SelectedItem as ComboBoxItem).Tag.ToString());
                            var months = GetQuarterMonths(quarter);
                            foreach (var m in months)
                            {
                                planHours += tp.GetHoursByMonth(m);
                            }
                            planCost = planHours * tp.HourlyCost;
                            break;

                        case "Year":
                            planHours = tp.TotalYearHours;
                            planCost = tp.TotalYearCost;
                            break;
                    }

                    if (planHours == 0) continue;

                    // Получаем фактические данные из заявок
                    var actualData = await GetActualDataAsync(tp.DepartmentId, tp.EquipmentId, year, periodType);

                    var item = new ReportItem
                    {
                        DepartmentName = tp.Department?.Name,
                        EquipmentName = tp.Equipment?.Name,
                        PlanHours = planHours,
                        ActualHours = actualData.ActualHours,
                        PlanCost = planCost,
                        ActualCost = actualData.ActualCost
                    };

                    _reportData.Add(item);
                }

                // Отображаем отчет
                dgReport.ItemsSource = _reportData;

                // Обновляем итоги
                UpdateTotals();

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
        /// Получение фактических данных из заявок
        /// </summary>
        private async System.Threading.Tasks.Task<(decimal ActualHours, decimal ActualCost)> GetActualDataAsync(
            string departmentId, string equipmentId, int year, string periodType)
        {
            var query = _databaseService.Context.ShiftRequests
                .Where(sr => sr.DepartmentId == departmentId &&
                            sr.EquipmentId == equipmentId &&
                            sr.Date.Year == year &&
                            sr.IsWorked == true);

            // Фильтр по периоду
            switch (periodType)
            {
                case "Month":
                    int month = int.Parse((cmbMonth.SelectedItem as ComboBoxItem).Tag.ToString());
                    query = query.Where(sr => sr.Date.Month == month);
                    break;

                case "Quarter":
                    int quarter = int.Parse((cmbQuarter.SelectedItem as ComboBoxItem).Tag.ToString());
                    var months = GetQuarterMonths(quarter);
                    query = query.Where(sr => months.Contains(sr.Date.Month));
                    break;

                    // Для года фильтр не нужен
            }

            var requests = await query.ToListAsync();

            decimal totalHours = requests.Sum(r => r.WorkedHours ?? 0);
            decimal totalCost = requests.Sum(r => r.ActualCost ?? 0);

            return (totalHours, totalCost);
        }

        /// <summary>
        /// Получение месяцев квартала
        /// </summary>
        private List<int> GetQuarterMonths(int quarter)
        {
            return quarter switch
            {
                1 => new List<int> { 1, 2, 3 },
                2 => new List<int> { 4, 5, 6 },
                3 => new List<int> { 7, 8, 9 },
                4 => new List<int> { 10, 11, 12 },
                _ => new List<int>()
            };
        }

        /// <summary>
        /// Обновление итоговых значений
        /// </summary>
        private void UpdateTotals()
        {
            if (_reportData == null || !_reportData.Any())
            {
                txtTotalPlanHours.Text = "0";
                txtTotalActualHours.Text = "0";
                txtTotalHoursDeviation.Text = "0";
                txtTotalPercent.Text = "0%";
                txtTotalPlanCost.Text = "0 ₽";
                txtTotalActualCost.Text = "0 ₽";
                txtTotalCostDeviation.Text = "0 ₽";
                return;
            }

            decimal totalPlanHours = _reportData.Sum(r => r.PlanHours);
            decimal totalActualHours = _reportData.Sum(r => r.ActualHours);
            decimal totalPlanCost = _reportData.Sum(r => r.PlanCost);
            decimal totalActualCost = _reportData.Sum(r => r.ActualCost);

            txtTotalPlanHours.Text = totalPlanHours.ToString("N2");
            txtTotalActualHours.Text = totalActualHours.ToString("N2");
            txtTotalHoursDeviation.Text = (totalActualHours - totalPlanHours).ToString("N2");

            if (totalPlanHours > 0)
            {
                txtTotalPercent.Text = (totalActualHours / totalPlanHours * 100).ToString("F1") + "%";
            }
            else
            {
                txtTotalPercent.Text = "0%";
            }

            txtTotalPlanCost.Text = totalPlanCost.ToString("N2") + " ₽";
            txtTotalActualCost.Text = totalActualCost.ToString("N2") + " ₽";
            txtTotalCostDeviation.Text = (totalActualCost - totalPlanCost).ToString("N2") + " ₽";
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

            var columns = new List<(string Header, Func<ReportItem, object> Value)>
            {
                ("Техника", r => r.EquipmentName),
                ("План, часы", r => r.PlanHours),
                ("Факт, часы", r => r.ActualHours),
                ("Отклонение, часы", r => r.HoursDeviation),
                ("% выполнения", r => r.ExecutionPercent),
                ("План, сумма", r => r.PlanCost),
                ("Факт, сумма", r => r.ActualCost),
                ("Отклонение, сумма", r => r.CostDeviation)
            };

            string periodText = GetPeriodText();
            string departmentText = cmbDepartment.SelectedItem is Department dept ? dept.Name : "Все отделы";

            ExcelExporter.ExportToExcel(
                _reportData,
                columns,
                $"Транспортная программа_{departmentText}_{periodText}");
        }

        /// <summary>
        /// Получение текстового описания периода
        /// </summary>
        private string GetPeriodText()
        {
            string periodType = (cmbPeriod.SelectedItem as ComboBoxItem)?.Tag.ToString();
            int year = (int)cmbYear.SelectedItem;

            return periodType switch
            {
                "Month" => $"{((ComboBoxItem)cmbMonth.SelectedItem).Content}_{year}",
                "Quarter" => $"{((ComboBoxItem)cmbQuarter.SelectedItem).Content}_{year}",
                "Year" => year.ToString(),
                _ => year.ToString()
            };
        }

        #endregion

        #region Вспомогательный класс для данных отчета

        /// <summary>
        /// Элемент отчета
        /// </summary>
        public class ReportItem
        {
            public string DepartmentName { get; set; }
            public string EquipmentName { get; set; }
            public decimal PlanHours { get; set; }
            public decimal ActualHours { get; set; }
            public decimal HoursDeviation => ActualHours - PlanHours;
            public decimal ExecutionPercent => PlanHours > 0 ? Math.Round(ActualHours / PlanHours * 100, 2) : 0;
            public decimal PlanCost { get; set; }
            public decimal ActualCost { get; set; }
            public decimal CostDeviation => ActualCost - PlanCost;
        }

        #endregion
    }
}