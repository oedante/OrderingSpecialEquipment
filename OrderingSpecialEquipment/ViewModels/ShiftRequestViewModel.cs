using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// ViewModel для заявки
    /// </summary>
    public class ShiftRequestViewModel : ViewModelBase, IDisposable
    {
        private readonly ShiftRequest _request;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDatabaseService _databaseService;
        private readonly IDbContextFactory _contextFactory;
        private readonly MainWindowViewModel _parent;
        private bool _isNew;
        private bool _isEdit;
        private bool _isExpanded;
        private bool _isUpdatingRelatedProperties = false;
        private bool _disposed = false;

        // Кэши для производительности
        private List<Warehouse> _availableWarehouses;
        private List<WarehouseArea> _availableAreas;

        // Навигационные свойства для привязки
        private Department _department;
        private Warehouse _warehouse;
        private WarehouseArea _area;
        private Equipment _equipment;
        private LessorOrganization _lessorOrganization;
        private LicensePlate _licensePlate;

        public ShiftRequestViewModel(ShiftRequest request,
            IAuthorizationService authorizationService,
            IDatabaseService databaseService,
            IDbContextFactory contextFactory,
            MainWindowViewModel parent)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public List<LicensePlate> FilteredLicensePlates => _parent?.FilteredLicensePlates ?? new List<LicensePlate>();
        public ShiftRequest OriginalRequest => _request;
        public int Key => _request.Key;

        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        public bool IsEdit
        {
            get => _isEdit;
            set => SetProperty(ref _isEdit, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        // Основные свойства
        public DateTime Date
        {
            get => _request.Date;
            set
            {
                _request.Date = value.ToUniversalTime();
                OnPropertyChanged();
                OnPropertyChanged(nameof(GroupDisplayString));
            }
        }

        public int Shift
        {
            get => _request.Shift;
            set
            {
                _request.Shift = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShiftName));
                OnPropertyChanged(nameof(GroupDisplayString));
            }
        }

        public string ShiftName => _request.Shift == 0 ? "Дневная" : "Ночная";

        public string EquipmentId
        {
            get => _request.EquipmentId;
            set
            {
                _request.EquipmentId = value;
                _equipment = null; // Сбрасываем кэш
                OnPropertyChanged();
                OnPropertyChanged(nameof(EquipmentName));
                OnPropertyChanged(nameof(CanOrderMultiple));
                OnPropertyChanged(nameof(GroupDisplayString));

                if (!CanOrderMultiple && RequestedCount > 1)
                {
                    RequestedCount = 1;
                }
            }
        }

        public string EquipmentName
        {
            get
            {
                if (_equipment != null)
                    return _equipment.Name;
                if (_request.Equipment != null)
                    return _request.Equipment.Name;
                return "";
            }
        }

        public string LicensePlateId
        {
            get => _request.LicensePlateId;
            set
            {
                if (_isUpdatingRelatedProperties) return;
                try
                {
                    _isUpdatingRelatedProperties = true;
                    _request.LicensePlateId = value;
                    _licensePlate = null; // Сбрасываем кэш
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PlateNumber));
                    OnPropertyChanged(nameof(PlateDisplay));
                    OnPropertyChanged(nameof(GroupDisplayString));

                    if (!string.IsNullOrEmpty(value))
                    {
                        var plate = _parent.AllLicensePlates?.FirstOrDefault(lp => lp.Id == value);
                        if (plate != null && !string.IsNullOrEmpty(plate.LessorOrganizationId))
                        {
                            LessorOrganizationId = plate.LessorOrganizationId;
                        }
                    }
                }
                finally
                {
                    _isUpdatingRelatedProperties = false;
                }
            }
        }

        public string PlateNumber
        {
            get
            {
                if (_licensePlate != null)
                    return _licensePlate.PlateNumber;
                if (_request.LicensePlate != null)
                    return _request.LicensePlate.PlateNumber;
                return "";
            }
        }

        public string PlateDisplay
        {
            get
            {
                var plate = _licensePlate ?? _request.LicensePlate;
                if (plate != null)
                {
                    return $"{plate.PlateNumber} - {plate.Brand}";
                }
                return "";
            }
        }

        public string WarehouseId
        {
            get => _request.WarehouseId;
            set
            {
                _request.WarehouseId = value;
                _warehouse = null; // Сбрасываем кэш
                _availableAreas = null; // Сбрасываем кэш территорий
                OnPropertyChanged();
                OnPropertyChanged(nameof(WarehouseName));
                OnPropertyChanged(nameof(AvailableAreas));

                // Сбрасываем территорию при смене склада
                AreaId = null;
                Area = null;
            }
        }

        public string WarehouseName
        {
            get
            {
                if (_warehouse != null)
                    return _warehouse.Name;
                if (_request.Warehouse != null)
                    return _request.Warehouse.Name;
                return "";
            }
        }

        public string AreaId
        {
            get => _request.AreaId;
            set
            {
                _request.AreaId = value;
                _area = null; // Сбрасываем кэш
                OnPropertyChanged();
                OnPropertyChanged(nameof(AreaName));
                OnPropertyChanged(nameof(GroupDisplayString));
            }
        }

        public string AreaName
        {
            get
            {
                if (_area != null)
                    return _area.Name;
                if (_request.Area != null)
                    return _request.Area.Name;
                return "";
            }
        }

        public string LessorOrganizationId
        {
            get => _request.LessorOrganizationId;
            set
            {
                if (_isUpdatingRelatedProperties) return;
                try
                {
                    _isUpdatingRelatedProperties = true;
                    _request.LessorOrganizationId = value;
                    _lessorOrganization = null; // Сбрасываем кэш
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LessorName));
                    OnPropertyChanged(nameof(GroupDisplayString));
                    _parent?.NotifyPropertyChanged(nameof(MainWindowViewModel.FilteredLicensePlates));

                    if (!string.IsNullOrEmpty(LicensePlateId))
                    {
                        var plate = _parent.AllLicensePlates?.FirstOrDefault(lp => lp.Id == LicensePlateId);
                        if (plate != null && plate.LessorOrganizationId != value)
                        {
                            LicensePlateId = null;
                            LicensePlate = null;
                        }
                    }
                }
                finally
                {
                    _isUpdatingRelatedProperties = false;
                }
            }
        }

        public string LessorName
        {
            get
            {
                if (_lessorOrganization != null)
                    return _lessorOrganization.Name;
                if (_request.LessorOrganization != null)
                    return _request.LessorOrganization.Name;
                return "";
            }
        }

        public int RequestedCount
        {
            get => _request.RequestedCount;
            set
            {
                if (value >= 1)
                {
                    _request.RequestedCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(GroupDisplayString));
                }
            }
        }

        public decimal? WorkedHours
        {
            get => _request.WorkedHours;
            set
            {
                _request.WorkedHours = value;
                OnPropertyChanged();
                if (value.HasValue && _request.HourlyCost.HasValue)
                {
                    ActualCost = Math.Round(value.Value * _request.HourlyCost.Value, 2);
                }
            }
        }

        public decimal? ActualCost
        {
            get => _request.ActualCost;
            set
            {
                _request.ActualCost = value;
                OnPropertyChanged();
            }
        }

        public bool IsWorked
        {
            get => _request.IsWorked;
            set
            {
                _request.IsWorked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RowBackgroundColor));
            }
        }

        public bool IsNotProvided
        {
            get => _request.IsNotProvided;
            set
            {
                _request.IsNotProvided = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RowBackgroundColor));
                if (value)
                {
                    WorkedHours = 0;
                    IsWeatherCancellation = false;
                }
            }
        }

        public bool IsWeatherCancellation
        {
            get => _request.IsWeatherCancellation;
            set
            {
                _request.IsWeatherCancellation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RowBackgroundColor));
                if (value)
                {
                    WorkedHours = 0;
                    IsNotProvided = false;
                }
            }
        }

        public string CancellationReason
        {
            get => _request.CancellationReason;
            set
            {
                _request.CancellationReason = value;
                OnPropertyChanged();
            }
        }

        public bool IsBlocked => _request.IsBlocked;
        public string LockedByUserId => _request.LockedByUserId;
        public DateTime? LockedAt => _request.LockedAt;

        public string Comment
        {
            get => _request.Comment;
            set
            {
                _request.Comment = value;
                OnPropertyChanged();
            }
        }

        public string CreatedByUserId => _request.CreatedByUserId;
        public string CreatedByUser => _request.CreatedByUser?.FullName ?? "";
        public DateTime CreatedAt => _request.CreatedAt;

        public string DepartmentId
        {
            get => _request.DepartmentId;
            set
            {
                _request.DepartmentId = value;
                _department = null; // Сбрасываем кэш
                _availableWarehouses = null; // Сбрасываем кэш складов
                OnPropertyChanged();
                OnPropertyChanged(nameof(DepartmentName));
                OnPropertyChanged(nameof(AvailableWarehouses));
            }
        }

        public string DepartmentName
        {
            get
            {
                if (_department != null)
                    return _department.Name;
                if (_request.Department != null)
                    return _request.Department.Name;
                return "";
            }
        }

        // Группировка в одну строку
        public string GroupDisplayString
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(DepartmentName)) parts.Add(DepartmentName);
                if (Date != default) parts.Add(Date.ToString("dd.MM.yyyy"));
                if (!string.IsNullOrEmpty(ShiftName)) parts.Add(ShiftName);
                if (!string.IsNullOrEmpty(WarehouseName)) parts.Add(WarehouseName);
                return string.Join(" | ", parts);
            }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(EquipmentId) &&
                       !string.IsNullOrEmpty(WarehouseId) &&
                       RequestedCount > 0;
            }
        }

        public bool CanEditEquipment => _authorizationService.CanWriteTable("Equipments");
        public bool CanEditWarehouse => _authorizationService.CanWriteTable("Warehouses");
        public bool CanEditLessor => _authorizationService.CanWriteTable("LessorOrganizations");

        public bool CanOrderMultiple
        {
            get
            {
                if (_equipment != null)
                    return _equipment.CanOrderMultiple;
                if (_request.Equipment != null)
                    return _request.Equipment.CanOrderMultiple;
                return false;
            }
        }

        // Навигационные свойства для привязки
        public Department Department
        {
            get
            {
                if (_department == null && !string.IsNullOrEmpty(DepartmentId))
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        _department = context.Departments.Find(DepartmentId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки Department: {ex.Message}");
                    }
                }
                return _department ?? _request.Department;
            }
            set
            {
                _department = value;
                DepartmentId = value?.Id;
                OnPropertyChanged();
            }
        }

        public Warehouse Warehouse
        {
            get
            {
                if (_warehouse == null && !string.IsNullOrEmpty(WarehouseId))
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        _warehouse = context.Warehouses.Find(WarehouseId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки Warehouse: {ex.Message}");
                    }
                }
                return _warehouse ?? _request.Warehouse;
            }
            set
            {
                _warehouse = value;
                WarehouseId = value?.Id;
                OnPropertyChanged();
            }
        }

        public WarehouseArea Area
        {
            get
            {
                if (_area == null && !string.IsNullOrEmpty(AreaId))
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        _area = context.WarehouseAreas.Find(AreaId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки Area: {ex.Message}");
                    }
                }
                return _area ?? _request.Area;
            }
            set
            {
                _area = value;
                AreaId = value?.Id;
                OnPropertyChanged();
            }
        }

        public Equipment Equipment
        {
            get
            {
                if (_equipment == null && !string.IsNullOrEmpty(EquipmentId))
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        _equipment = context.Equipments.Find(EquipmentId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки Equipment: {ex.Message}");
                    }
                }
                return _equipment ?? _request.Equipment;
            }
            set
            {
                _equipment = value;
                EquipmentId = value?.Id;
                OnPropertyChanged(nameof(CanOrderMultiple));
                OnPropertyChanged(nameof(EquipmentName));
            }
        }

        public LessorOrganization LessorOrganization
        {
            get
            {
                if (_lessorOrganization == null && !string.IsNullOrEmpty(LessorOrganizationId))
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        _lessorOrganization = context.LessorOrganizations.Find(LessorOrganizationId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки LessorOrganization: {ex.Message}");
                    }
                }
                return _lessorOrganization ?? _request.LessorOrganization;
            }
            set
            {
                _lessorOrganization = value;
                LessorOrganizationId = value?.Id;
                OnPropertyChanged();
            }
        }

        public LicensePlate LicensePlate
        {
            get
            {
                if (_licensePlate == null && !string.IsNullOrEmpty(LicensePlateId))
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        _licensePlate = context.LicensePlates
                            .Include(lp => lp.Equipment)
                            .Include(lp => lp.LessorOrganization)
                            .FirstOrDefault(lp => lp.Id == LicensePlateId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки LicensePlate: {ex.Message}");
                    }
                }
                return _licensePlate ?? _request.LicensePlate;
            }
            set
            {
                _licensePlate = value;
                LicensePlateId = value?.Id;
                OnPropertyChanged(nameof(PlateDisplay));

                // Автоматически подтягиваем арендодателя
                if (value != null && !_isUpdatingRelatedProperties)
                {
                    LessorOrganizationId = value.LessorOrganizationId;
                }
            }
        }

        // Доступные склады для выбранного отдела
        public List<Warehouse> AvailableWarehouses
        {
            get
            {
                if (string.IsNullOrEmpty(DepartmentId))
                    return new List<Warehouse>();

                if (_availableWarehouses == null)
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        _availableWarehouses = context.Warehouses
                            .Where(w => w.DepartmentId == DepartmentId && w.IsActive)
                            .OrderBy(w => w.Name)
                            .ToList();
                        System.Diagnostics.Debug.WriteLine($"AvailableWarehouses: загружено {_availableWarehouses.Count} складов");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading AvailableWarehouses: {ex.Message}");
                        _availableWarehouses = new List<Warehouse>();
                    }
                }
                return _availableWarehouses;
            }
        }

        // Доступные территории для выбранного склада (через связи многие-ко-многим)
        public List<WarehouseArea> AvailableAreas
        {
            get
            {
                if (string.IsNullOrEmpty(WarehouseId))
                    return new List<WarehouseArea>();

                if (_availableAreas == null)
                {
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        // Загружаем территории через связи (многие-ко-многим)
                        _availableAreas = context.Set<WarehouseAreaLink>()
                            .Include(wal => wal.Area)
                            .Where(wal => wal.WarehouseId == WarehouseId)
                            .Select(wal => wal.Area)
                            .Where(a => a.IsActive)
                            .OrderBy(a => a.Name)
                            .ToList();
                        System.Diagnostics.Debug.WriteLine($"AvailableAreas: загружено {_availableAreas.Count} территорий");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading AvailableAreas: {ex.Message}");
                        _availableAreas = new List<WarehouseArea>();
                    }
                }
                return _availableAreas;
            }
        }

        public string RowBackgroundColor
        {
            get
            {
                if (IsBlocked) return "#FFF0F0F0";
                if (IsWorked) return "#FFE0FFE0";
                if (IsNotProvided) return "#FFFFE0E0";
                if (IsWeatherCancellation) return "#FFE0F0FF";
                return "White";
            }
        }

        public void Cleanup()
        {
            _availableWarehouses = null;
            _availableAreas = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Cleanup();
                }
                _disposed = true;
            }
        }

        ~ShiftRequestViewModel()
        {
            Dispose(false);
        }
    }
}