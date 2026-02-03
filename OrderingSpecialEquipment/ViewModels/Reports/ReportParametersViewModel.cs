using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Models.Reports;
using OrderingSpecialEquipment.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.ViewModels.Reports
{
    /// <summary>
    /// ViewModel параметров отчета
    /// </summary>
    public class ReportParametersViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthorizationService _authorizationService;

        // Поля
        private ReportType _selectedReportType;
        private DateTime _startDate;
        private DateTime _endDate;
        private int? _selectedYear;
        private int? _selectedMonth;
        private string? _selectedDepartmentId;
        private string? _selectedWarehouseId;
        private string? _selectedEquipmentId;
        private bool _onlyWorked;
        private bool _monthlyBreakdown;
        private bool _isGenerating;
        private string _statusMessage;

        // Коллекции для выпадающих списков
        private ObservableCollection<Department> _departments;
        private ObservableCollection<Warehouse> _warehouses;
        private ObservableCollection<Equipment> _equipments;
        private ObservableCollection<int> _years;
        private ObservableCollection<(int Value, string Name)> _months;

        public ReportParametersViewModel(IServiceProvider serviceProvider, IAuthorizationService authorizationService)
        {
            _serviceProvider = serviceProvider;
            _authorizationService = authorizationService;

            // Инициализация значений по умолчанию
            SelectedReportType = ReportType.Execution;
            StartDate = DateTime.Today.AddDays(-30);
            EndDate = DateTime.Today;
            SelectedYear = DateTime.Today.Year;

            // Инициализация коллекций
            Departments = new ObservableCollection<Department>();
            Warehouses = new ObservableCollection<Warehouse>();
            Equipments = new ObservableCollection<Equipment>();
            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<(int Value, string Name)>();

            // Заполнение списка месяцев
            for (int i = 1; i <= 12; i++)
            {
                Months.Add((i, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i)));
            }

            // Заполнение списка лет (текущий год и 5 предыдущих)
            for (int i = 0; i <= 5; i++)
            {
                Years.Add(DateTime.Today.Year - i);
            }

            // Загрузка данных
            _ = LoadDataAsync();
        }

        // Свойства
        public ReportType SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                if (SetProperty(ref _selectedReportType, value))
                {
                    // Сброс параметров при смене типа отчета
                    OnPropertyChanged(nameof(IsTransportProgramReport));
                    OnPropertyChanged(nameof(ShowMonthSelection));
                }
            }
        }

        public bool IsTransportProgramReport => SelectedReportType == ReportType.TransportProgram;
        public bool ShowMonthSelection => IsTransportProgramReport && MonthlyBreakdown;

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public int? SelectedYear
        {
            get => _selectedYear;
            set => SetProperty(ref _selectedYear, value);
        }

        public int? SelectedMonth
        {
            get => _selectedMonth;
            set => SetProperty(ref _selectedMonth, value);
        }

        public string? SelectedDepartmentId
        {
            get => _selectedDepartmentId;
            set
            {
                if (SetProperty(ref _selectedDepartmentId, value))
                {
                    _ = LoadWarehousesAsync();
                }
            }
        }

        public string? SelectedWarehouseId
        {
            get => _selectedWarehouseId;
            set => SetProperty(ref _selectedWarehouseId, value);
        }

        public string? SelectedEquipmentId
        {
            get => _selectedEquipmentId;
            set => SetProperty(ref _selectedEquipmentId, value);
        }

        public bool OnlyWorked
        {
            get => _onlyWorked;
            set => SetProperty(ref _onlyWorked, value);
        }

        public bool MonthlyBreakdown
        {
            get => _monthlyBreakdown;
            set
            {
                if (SetProperty(ref _monthlyBreakdown, value))
                {
                    OnPropertyChanged(nameof(ShowMonthSelection));
                }
            }
        }

        public bool IsGenerating
        {
            get => _isGenerating;
            set => SetProperty(ref _isGenerating, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
        }

        public ObservableCollection<Equipment> Equipments
        {
            get => _equipments;
            set => SetProperty(ref _equipments, value);
        }

        public ObservableCollection<int> Years
        {
            get => _years;
            set => SetProperty(ref _years, value);
        }

        public ObservableCollection<(int Value, string Name)> Months
        {
            get => _months;
            set => SetProperty(ref _months, value);
        }

        // Методы
        private async Task LoadDataAsync()
        {
            try
            {
                IsGenerating = true;
                StatusMessage = "Загрузка справочников...";

                await LoadDepartmentsAsync();
                await LoadEquipmentsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки данных: {ex.Message}";
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private async Task LoadDepartmentsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var departmentRepo = scope.ServiceProvider.GetRequiredService<IDepartmentRepository>();
                var departments = await departmentRepo.GetActiveDepartmentsAsync();

                // Фильтрация по правам доступа будет выполнена в сервисе отчетов
                Departments = new ObservableCollection<Department>(departments);

                if (Departments.Any())
                {
                    SelectedDepartmentId = Departments.First().Id;
                }
            }
        }

        private async Task LoadWarehousesAsync()
        {
            if (string.IsNullOrEmpty(SelectedDepartmentId))
                return;

            using (var scope = _serviceProvider.CreateScope())
            {
                var warehouseRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Warehouse>>();
                var warehouses = await warehouseRepo.FindAsync(w =>
                    w.DepartmentId == SelectedDepartmentId && w.IsActive);

                Warehouses = new ObservableCollection<Warehouse>(warehouses);

                if (Warehouses.Any())
                {
                    SelectedWarehouseId = Warehouses.First().Id;
                }
            }
        }

        private async Task LoadEquipmentsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var equipmentRepo = scope.ServiceProvider.GetRequiredService<IEquipmentRepository>();
                var equipments = await equipmentRepo.GetActiveEquipmentAsync();

                Equipments = new ObservableCollection<Equipment>(equipments);
            }
        }

        /// <summary>
        /// Преобразует параметры ViewModel в модель параметров отчета
        /// </summary>
        public ReportParameters ToReportParameters()
        {
            return new ReportParameters
            {
                ReportType = SelectedReportType.ToString(),
                StartDate = StartDate,
                EndDate = EndDate,
                Year = SelectedYear,
                Month = SelectedMonth,
                DepartmentId = SelectedDepartmentId,
                WarehouseId = SelectedWarehouseId,
                EquipmentId = SelectedEquipmentId,
                OnlyWorked = OnlyWorked,
                MonthlyBreakdown = MonthlyBreakdown
            };
        }
    }
}