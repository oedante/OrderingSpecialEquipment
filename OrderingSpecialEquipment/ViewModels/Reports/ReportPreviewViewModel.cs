using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Models.Reports;
using OrderingSpecialEquipment.Services.Reports;
using OrderingSpecialEquipment.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace OrderingSpecialEquipment.ViewModels.Reports
{
    /// <summary>
    /// ViewModel окна предпросмотра отчетов
    /// </summary>
    public class ReportPreviewViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IServiceProvider _serviceProvider;

        // Поля
        private ReportParametersViewModel _parameters;
        private ObservableCollection<BaseReportItem> _reportItems;
        private ReportSummary? _reportSummary;
        private bool _isReportGenerated;
        private bool _isLoading;
        private string _statusMessage;
        private string _reportTitle;

        public ReportPreviewViewModel(IReportService reportService, IServiceProvider serviceProvider)
        {
            _reportService = reportService;
            _serviceProvider = serviceProvider;

            // Инициализация
            Parameters = new ReportParametersViewModel(serviceProvider,
                serviceProvider.GetRequiredService<IAuthorizationService>());
            ReportItems = new ObservableCollection<BaseReportItem>();
            ReportTitle = "Предпросмотр отчета";

            // Команды
            GenerateReportCommand = new RelayCommand(GenerateReport, CanGenerateReport);
            ExportToExcelCommand = new RelayCommand(ExportToExcel, CanExport);
            PrintReportCommand = new RelayCommand(PrintReport);
            CloseCommand = new RelayCommand(() => Application.Current.Windows[0].Close());
        }

        // Свойства
        public ReportParametersViewModel Parameters
        {
            get => _parameters;
            set => SetProperty(ref _parameters, value);
        }

        public ObservableCollection<BaseReportItem> ReportItems
        {
            get => _reportItems;
            set => SetProperty(ref _reportItems, value);
        }

        public ReportSummary? ReportSummary
        {
            get => _reportSummary;
            set => SetProperty(ref _reportSummary, value);
        }

        public bool IsReportGenerated
        {
            get => _isReportGenerated;
            set => SetProperty(ref _isReportGenerated, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string ReportTitle
        {
            get => _reportTitle;
            set => SetProperty(ref _reportTitle, value);
        }

        // Команды
        public RelayCommand GenerateReportCommand { get; }
        public RelayCommand ExportToExcelCommand { get; }
        public RelayCommand PrintReportCommand { get; }
        public RelayCommand CloseCommand { get; }

        // Методы
        private bool CanGenerateReport() => !IsLoading;

        private async void GenerateReport()
        {
            try
            {
                IsLoading = true;
                IsReportGenerated = false;
                StatusMessage = "Формирование отчета...";
                ReportItems.Clear();

                var parameters = Parameters.ToReportParameters();

                if (Parameters.SelectedReportType == ReportType.Execution)
                {
                    ReportTitle = "Отчет по исполнению заявок";
                    var (items, summary) = await _reportService.GenerateExecutionReportAsync(parameters);

                    // Преобразование в базовый тип для отображения
                    foreach (var item in items)
                    {
                        ReportItems.Add(item);
                    }

                    ReportSummary = summary;
                }
                else if (Parameters.SelectedReportType == ReportType.TransportProgram)
                {
                    ReportTitle = "Отчет по транспортной программе";

                    // Проверка обязательных параметров
                    if (!parameters.Year.HasValue)
                    {
                        MessageBox.Show("Для отчета по транспортной программе необходимо указать год",
                            "Ошибка параметров", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var (items, summary) = await _reportService.GenerateTransportProgramReportAsync(parameters);

                    foreach (var item in items)
                    {
                        ReportItems.Add(item);
                    }

                    ReportSummary = summary;
                }

                IsReportGenerated = true;
                StatusMessage = $"Отчет сформирован: {ReportItems.Count} записей";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка формирования отчета: {ex.Message}";
                MessageBox.Show($"Ошибка при формировании отчета:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanExport() => IsReportGenerated && !IsLoading;

        private async void ExportToExcel()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Экспорт в Excel...";

                object reportData;
                if (Parameters.SelectedReportType == ReportType.Execution)
                {
                    // Преобразование обратно к конкретному типу
                    var items = new List<ExecutionReportItem>();
                    foreach (var item in ReportItems)
                    {
                        if (item is ExecutionReportItem executionItem)
                        {
                            items.Add(executionItem);
                        }
                    }
                    reportData = (items, ReportSummary!);
                }
                else
                {
                    var items = new List<TransportProgramReportItem>();
                    foreach (var item in ReportItems)
                    {
                        if (item is TransportProgramReportItem transportItem)
                        {
                            items.Add(transportItem);
                        }
                    }
                    reportData = (items, ReportSummary!);
                }

                await _reportService.ExportToExcelAsync(
                    reportData,
                    Parameters.SelectedReportType.ToString(),
                    Parameters.ToReportParameters());

                StatusMessage = "Отчет экспортирован в Excel";
                MessageBox.Show("Отчет успешно экспортирован на рабочий стол",
                    "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
                MessageBox.Show($"Ошибка при экспорте в Excel:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void PrintReport()
        {
            MessageBox.Show("Функция печати будет реализована в следующей версии",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}