using OrderingSpecialEquipment.ViewModels.Reports;
using System.Windows;
using System.Windows.Controls;

namespace OrderingSpecialEquipment.Views.Reports
{
    /// <summary>
    /// Логика взаимодействия для окна предпросмотра отчетов
    /// </summary>
    public partial class ReportPreviewWindow : Window
    {
        public ReportPreviewWindow(ReportPreviewViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Динамическое создание колонок таблицы в зависимости от типа отчета
            Loaded += ReportPreviewWindow_Loaded;
        }

        private void ReportPreviewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReportPreviewViewModel viewModel)
            {
                // Подписка на изменение типа отчета для обновления колонок
                viewModel.Parameters.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(viewModel.Parameters.SelectedReportType))
                    {
                        UpdateDataGridColumns(viewModel.Parameters.SelectedReportType);
                    }
                };

                // Инициализация колонок
                UpdateDataGridColumns(viewModel.Parameters.SelectedReportType);
            }
        }

        /// <summary>
        /// Обновляет колонки таблицы в зависимости от типа отчета
        /// </summary>
        private void UpdateDataGridColumns(ViewModels.Reports.ReportType reportType)
        {
            var dataGrid = FindName("ReportDataGrid") as DataGrid;
            if (dataGrid == null) return;

            dataGrid.Columns.Clear();

            // Общая колонка номера
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "№",
                Binding = new System.Windows.Data.Binding("RowNumber"),
                Width = 40
            });

            if (reportType == ViewModels.Reports.ReportType.Execution)
            {
                // Колонки для отчета по исполнению
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Дата",
                    Binding = new System.Windows.Data.Binding("Date") { StringFormat = "d" },
                    Width = 90
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Смена",
                    Binding = new System.Windows.Data.Binding("ShiftName"),
                    Width = 80
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Отдел",
                    Binding = new System.Windows.Data.Binding("DepartmentName"),
                    Width = 120
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Склад",
                    Binding = new System.Windows.Data.Binding("WarehouseName"),
                    Width = 120
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Территория",
                    Binding = new System.Windows.Data.Binding("AreaName"),
                    Width = 120
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Техника",
                    Binding = new System.Windows.Data.Binding("EquipmentName"),
                    Width = 180
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Госномер",
                    Binding = new System.Windows.Data.Binding("PlateNumber"),
                    Width = 100
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Организация",
                    Binding = new System.Windows.Data.Binding("LessorOrganizationName"),
                    Width = 150
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Марка",
                    Binding = new System.Windows.Data.Binding("Brand"),
                    Width = 120
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Кол-во",
                    Binding = new System.Windows.Data.Binding("RequestedCount"),
                    Width = 60
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Часы",
                    Binding = new System.Windows.Data.Binding("WorkedHours"),
                    Width = 60
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Стоимость",
                    Binding = new System.Windows.Data.Binding("ActualCost"),
                    ElementStyle = CreateCurrencyElementStyle(),
                    Width = 100
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Статус",
                    Binding = new System.Windows.Data.Binding("Status"),
                    Width = 80
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Пользователь",
                    Binding = new System.Windows.Data.Binding("CreatedByUserName"),
                    Width = 150
                });
            }
            else
            {
                // Колонки для отчета по транспортной программе
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Отдел",
                    Binding = new System.Windows.Data.Binding("DepartmentName"),
                    Width = 150
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Техника",
                    Binding = new System.Windows.Data.Binding("EquipmentName"),
                    Width = 200
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Год",
                    Binding = new System.Windows.Data.Binding("Year"),
                    Width = 60
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Ст-ть часа",
                    Binding = new System.Windows.Data.Binding("HourlyCost"),
                    ElementStyle = CreateCurrencyElementStyle(),
                    Width = 100
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "План (часы)",
                    Binding = new System.Windows.Data.Binding("PlannedHours"),
                    Width = 80
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Факт (часы)",
                    Binding = new System.Windows.Data.Binding("ActualHours"),
                    Width = 80
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "% выполнения",
                    Binding = new System.Windows.Data.Binding("CompletionPercentage") { StringFormat = "{0:F2}%" },
                    Width = 100
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "План (руб)",
                    Binding = new System.Windows.Data.Binding("PlannedCost"),
                    ElementStyle = CreateCurrencyElementStyle(),
                    Width = 100
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Факт (руб)",
                    Binding = new System.Windows.Data.Binding("ActualCost"),
                    ElementStyle = CreateCurrencyElementStyle(),
                    Width = 100
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Разница",
                    Binding = new System.Windows.Data.Binding("CostDifference"),
                    ElementStyle = CreateCurrencyElementStyle(),
                    Width = 100
                });
            }
        }

        /// <summary>
        /// Создает стиль для отображения валюты
        /// </summary>
        private Style CreateCurrencyElementStyle()
        {
            var style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right));
            style.Setters.Add(new Setter(TextBlock.MarginProperty, new Thickness(4, 0, 4, 0)));
            return style;
        }
    }
}