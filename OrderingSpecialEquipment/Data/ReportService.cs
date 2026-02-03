using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;
using OrderingSpecialEquipment.Services.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Реализация сервиса отчетов
    /// Генерирует данные для отчетов с учетом прав доступа пользователя
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserRepository _userRepository;

        public ReportService(
            IServiceProvider serviceProvider,
            IAuthorizationService authorizationService,
            IUserRepository userRepository)
        {
            _serviceProvider = serviceProvider;
            _authorizationService = authorizationService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Генерирует отчет по исполнению заявок
        /// </summary>
        public async Task<(List<ExecutionReportItem> items, ReportSummary summary)> GenerateExecutionReportAsync(ReportParameters parameters)
        {
            var items = new List<ExecutionReportItem>();
            var currentUser = await _userRepository.GetByWindowsLoginAsync(Environment.UserName);

            if (currentUser == null)
                throw new InvalidOperationException("Пользователь не авторизован");

            using (var scope = _serviceProvider.CreateScope())
            {
                var shiftRequestRepo = scope.ServiceProvider.GetRequiredService<IShiftRequestRepository>();
                var requests = await shiftRequestRepo.GetAllWithDetailsAsync();

                // Фильтрация по периоду
                requests = requests.Where(r =>
                    r.Date >= parameters.StartDate.Date &&
                    r.Date <= parameters.EndDate.Date).ToList();

                // Фильтрация по статусу (только выполненные)
                if (parameters.OnlyWorked)
                {
                    requests = requests.Where(r => r.IsWorked).ToList();
                }

                // Фильтрация по правам доступа пользователя
                var filteredRequests = new List<ShiftRequest>();
                foreach (var request in requests)
                {
                    if (request.Warehouse?.DepartmentId != null &&
                        await _authorizationService.HasAccessToDepartmentAsync(currentUser, request.Warehouse.DepartmentId))
                    {
                        // Дополнительная фильтрация по отделу, складу, технике если указаны параметры
                        if (!string.IsNullOrEmpty(parameters.DepartmentId) && request.DepartmentId != parameters.DepartmentId)
                            continue;

                        if (!string.IsNullOrEmpty(parameters.WarehouseId) && request.WarehouseId != parameters.WarehouseId)
                            continue;

                        if (!string.IsNullOrEmpty(parameters.EquipmentId) && request.EquipmentId != parameters.EquipmentId)
                            continue;

                        filteredRequests.Add(request);
                    }
                }

                // Формирование элементов отчета
                int rowNumber = 1;
                foreach (var request in filteredRequests.OrderBy(r => r.Date).ThenBy(r => r.Shift))
                {
                    items.Add(new ExecutionReportItem
                    {
                        RowNumber = rowNumber++,
                        Date = request.Date,
                        ShiftName = request.Shift == 0 ? "Ночная" : "Дневная",
                        DepartmentName = request.Department?.Name ?? "Не указан",
                        WarehouseName = request.Warehouse?.Name ?? "Не указан",
                        AreaName = request.Area?.Name ?? "Не указана",
                        EquipmentName = request.Equipment?.Name ?? "Не указана",
                        PlateNumber = request.LicensePlate?.PlateNumber ?? request.VehicleNumber ?? "Не указан",
                        LessorOrganizationName = request.LessorOrganization?.Name ?? request.LicensePlate?.LessorOrganization?.Name ?? "Не указана",
                        Brand = request.LicensePlate?.Brand ?? request.VehicleBrand ?? "Не указана",
                        RequestedCount = request.RequestedCount,
                        WorkedHours = request.WorkedHours,
                        ActualCost = request.ActualCost,
                        IsWorked = request.IsWorked,
                        Comment = request.Comment ?? string.Empty,
                        CreatedByUserName = request.CreatedByUser?.FullName ?? "Не указан"
                    });
                }

                // Формирование сводной информации
                var summary = new ReportSummary
                {
                    TotalRecords = items.Count,
                    TotalAmount = items.Where(i => i.ActualCost.HasValue).Sum(i => i.ActualCost.Value),
                    TotalHours = items.Where(i => i.WorkedHours.HasValue).Sum(i => i.WorkedHours.Value),
                    AverageHourlyCost = items.Where(i => i.WorkedHours.HasValue && i.WorkedHours > 0 && i.ActualCost.HasValue)
                        .Select(i => i.ActualCost.Value / i.WorkedHours.Value).DefaultIfEmpty(0).Average(),
                    CompletionPercentage = items.Any() ?
                        (items.Count(i => i.IsWorked) * 100m / items.Count) : 0
                };

                return (items, summary);
            }
        }

        /// <summary>
        /// Генерирует отчет по транспортной программе
        /// </summary>
        public async Task<(List<TransportProgramReportItem> items, ReportSummary summary)> GenerateTransportProgramReportAsync(ReportParameters parameters)
        {
            var items = new List<TransportProgramReportItem>();
            var currentUser = await _userRepository.GetByWindowsLoginAsync(Environment.UserName);

            if (currentUser == null)
                throw new InvalidOperationException("Пользователь не авторизован");

            if (!parameters.Year.HasValue)
                throw new ArgumentException("Год обязателен для отчета по транспортной программе");

            using (var scope = _serviceProvider.CreateScope())
            {
                var transportProgramRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<TransportProgram>>();
                var shiftRequestRepo = scope.ServiceProvider.GetRequiredService<IShiftRequestRepository>();
                var departmentRepo = scope.ServiceProvider.GetRequiredService<IDepartmentRepository>();

                // Получаем транспортные программы за указанный год
                var programs = await transportProgramRepo.FindAsync(tp =>
                    tp.Year == parameters.Year.Value &&
                    tp.Department.IsActive &&
                    tp.Equipment.IsActive);

                // Фильтрация по правам доступа пользователя
                var accessibleDepartments = new List<string>();
                var allDepartments = await departmentRepo.GetActiveDepartmentsAsync();

                foreach (var dept in allDepartments)
                {
                    if (await _authorizationService.HasAccessToDepartmentAsync(currentUser, dept.Id))
                    {
                        accessibleDepartments.Add(dept.Id);
                    }
                }

                // Фильтрация программ по доступным отделам
                programs = programs.Where(tp => accessibleDepartments.Contains(tp.DepartmentId)).ToList();

                // Дополнительная фильтрация по отделу если указан
                if (!string.IsNullOrEmpty(parameters.DepartmentId))
                {
                    programs = programs.Where(tp => tp.DepartmentId == parameters.DepartmentId).ToList();
                }

                // Получаем заявки за период для расчета фактических данных
                var requests = await shiftRequestRepo.GetAllWithDetailsAsync();
                requests = requests.Where(r =>
                    r.ProgramYear == parameters.Year.Value &&
                    (!parameters.Month.HasValue || r.ProgramMonth == parameters.Month.Value) &&
                    accessibleDepartments.Contains(r.DepartmentId ?? "")).ToList();

                // Формирование элементов отчета
                int rowNumber = 1;
                foreach (var program in programs)
                {
                    // Расчет плановых часов за период
                    decimal plannedHours = 0;
                    if (parameters.Month.HasValue)
                    {
                        // Для конкретного месяца
                        plannedHours = program.GetHoursForMonth(parameters.Month.Value);
                    }
                    else
                    {
                        // За весь год
                        plannedHours = program.JanuaryHours + program.FebruaryHours + program.MarchHours +
                                      program.AprilHours + program.MayHours + program.JuneHours +
                                      program.JulyHours + program.AugustHours + program.SeptemberHours +
                                      program.OctoberHours + program.NovemberHours + program.DecemberHours;
                    }

                    // Расчет фактических часов за период
                    var actualRequests = requests.Where(r =>
                        r.DepartmentId == program.DepartmentId &&
                        r.EquipmentId == program.EquipmentId).ToList();

                    decimal actualHours = actualRequests.Sum(r => r.WorkedHours ?? 0);
                    decimal actualCost = actualRequests.Sum(r => r.ActualCost ?? 0);

                    // Пропускаем записи без плановых и фактических данных
                    if (plannedHours == 0 && actualHours == 0)
                        continue;

                    items.Add(new TransportProgramReportItem
                    {
                        RowNumber = rowNumber++,
                        DepartmentName = program.Department?.Name ?? "Не указан",
                        EquipmentName = program.Equipment?.Name ?? "Не указана",
                        Year = program.Year,
                        HourlyCost = program.HourlyCost,
                        PlannedHours = plannedHours,
                        ActualHours = actualHours,
                        ActualCost = actualCost
                    });
                }

                // Сортировка отчета
                items = items.OrderBy(i => i.DepartmentName).ThenBy(i => i.EquipmentName).ToList();

                // Формирование сводной информации
                var summary = new ReportSummary
                {
                    TotalRecords = items.Count,
                    TotalAmount = items.Sum(i => i.PlannedCost),
                    TotalHours = items.Sum(i => i.PlannedHours),
                    AverageHourlyCost = items.Any() ? items.Average(i => i.HourlyCost) : 0,
                    CompletionPercentage = items.Any() ?
                        (items.Sum(i => i.ActualHours) * 100m / items.Sum(i => i.PlannedHours)) : 0
                };

                return (items, summary);
            }
        }

        /// <summary>
        /// Экспортирует отчет в Excel
        /// </summary>
        public async Task ExportToExcelAsync(object reportData, string reportType, ReportParameters parameters)
        {
            var exporter = new ExcelReportExporter();
            await exporter.ExportAsync(reportData, reportType, parameters);
        }
    }
}