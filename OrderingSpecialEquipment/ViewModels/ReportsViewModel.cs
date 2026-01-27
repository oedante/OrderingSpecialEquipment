using OrderingSpecialEquipment.Commands;
using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// ViewModel для окна отчетов.
    /// </summary>
    public class ReportsViewModel : ViewModelBase
    {
        private readonly IShiftRequestRepository _shiftRequestRepository;
        private readonly ITransportProgramRepository _transportProgramRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEquipmentRepositoryBase _equipmentRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IExcelExportService _excelExportService;

        // Пример данных для отчета
        private ObservableCollection<ShiftRequest> _reportData;
        private DateTime _reportStartDate = DateTime.Today.AddDays(-7);
        private DateTime _reportEndDate = DateTime.Today;

        public ReportsViewModel(
            IShiftRequestRepository shiftRequestRepository,
            ITransportProgramRepository transportProgramRepository,
            IUserRepository userRepository,
            IDepartmentRepository departmentRepository,
            IEquipmentRepositoryBase equipmentRepository,
            IWarehouseRepository warehouseRepository,
            IMessageService messageService,
            IAuthorizationService authorizationService,
            IExcelExportService excelExportService)
        {
            _shiftRequestRepository = shiftRequestRepository;
            _transportProgramRepository = transportProgramRepository;
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
            _equipmentRepository = equipmentRepository;
            _warehouseRepository = warehouseRepository;
            _messageService = messageService;
            _authorizationService = authorizationService;
            _excelExportService = excelExportService;

            _reportData = new ObservableCollection<ShiftRequest>();

            GenerateReportCommand = new RelayCommand(async _ => await GenerateReportAsync(), _ => _authorizationService.HasSpecialPermission("SPEC_ViewReports"));
            ExportToExcelCommand = new RelayCommand(async _ => await ExportToExcelAsync(), _ => ReportData.Count > 0);

            // Инициализация данных, если нужно
            // Task.Run(async () => await GenerateReportAsync()); // Загрузка отчета по умолчанию
        }

        public ObservableCollection<ShiftRequest> ReportData
        {
            get => _reportData;
            set => SetProperty(ref _reportData, value);
        }

        public DateTime ReportStartDate
        {
            get => _reportStartDate;
            set => SetProperty(ref _reportStartDate, value);
        }

        public DateTime ReportEndDate
        {
            get => _reportEndDate;
            set => SetProperty(ref _reportEndDate, value);
        }

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        private async Task GenerateReportAsync()
        {
            try
            {
                // Очищаем старые данные
                ReportData.Clear();

                // Загружаем данные за выбранный период
                var data = await _shiftRequestRepository.GetByDateRangeAsync(ReportStartDate, ReportEndDate);

                // Можно добавить фильтрацию по правам доступа пользователя (отдел, склад)
                var filteredData = new List<ShiftRequest>();
                foreach (var item in data)
                {
                    // Проверяем права доступа к отделу и складу
                    if (_authorizationService.CanAccessDepartment(item.DepartmentId ?? "") &&
                        _authorizationService.CanAccessWarehouse(item.WarehouseId))
                    {
                        filteredData.Add(item);
                    }
                }

                // Заполняем коллекцию
                foreach (var item in filteredData)
                {
                    ReportData.Add(item);
                }

                _messageService.ShowInfoMessage($"Сформирован отчет за период с {ReportStartDate:d} по {ReportEndDate:d}. Найдено {ReportData.Count} записей.", "Отчет сформирован");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка формирования отчета: {ex.Message}", "Ошибка");
            }
        }

        private async Task ExportToExcelAsync()
        {
            if (ReportData.Count == 0) return;

            try
            {
                // Диалог сохранения файла
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                    FileName = $"Отчет_Заявки_{ReportStartDate:yyyyMMdd}_{ReportEndDate:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    bool success = _excelExportService.ExportShiftRequestsToExcel(ReportData.ToList(), saveFileDialog.FileName);
                    if (success)
                    {
                        _messageService.ShowInfoMessage($"Данные экспортированы в {saveFileDialog.FileName}", "Экспорт завершен");
                    }
                    else
                    {
                        _messageService.ShowErrorMessage("Ошибка экспорта в Excel.", "Ошибка");
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка экспорта в Excel: {ex.Message}", "Ошибка");
            }
        }
    }
}