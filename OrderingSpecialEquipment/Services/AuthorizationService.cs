using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Сервис авторизации и проверки прав доступа
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        #region Поля

        private readonly IAuthenticationService _authenticationService;
        private readonly IDatabaseService _databaseService;
        private List<Department> _cachedDepartments;
        private List<Warehouse> _cachedWarehouses;
        private DateTime _cacheTime = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        #endregion

        #region Свойства

        /// <summary>
        /// Является ли пользователь системным администратором
        /// </summary>
        public bool IsSystemAdmin =>
            _authenticationService.IsAuthenticated &&
            _authenticationService.CurrentUserRole?.SPEC_SystemAdmin == true;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор сервиса авторизации
        /// </summary>
        /// <param name="authenticationService">Сервис аутентификации</param>
        /// <param name="databaseService">Сервис БД</param>
        public AuthorizationService(
            IAuthenticationService authenticationService,
            IDatabaseService databaseService)
        {
            _authenticationService = authenticationService;
            _databaseService = databaseService;

            // Подписываемся на изменение пользователя для сброса кэша
            _authenticationService.UserChanged += (s, e) => ClearCache();
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Проверка права на чтение таблицы
        /// </summary>
        public bool CanReadTable(string tableName)
        {
            if (!_authenticationService.IsAuthenticated)
                return false;

            var role = _authenticationService.CurrentUserRole;
            if (role == null)
                return false;

            // Системный администратор имеет все права
            if (role.SPEC_SystemAdmin)
                return true;

            return role.HasTableAccess(tableName, 1);
        }

        /// <summary>
        /// Проверка права на запись в таблицу
        /// </summary>
        public bool CanWriteTable(string tableName)
        {
            if (!_authenticationService.IsAuthenticated)
                return false;

            var role = _authenticationService.CurrentUserRole;
            if (role == null)
                return false;

            // Системный администратор имеет все права
            if (role.SPEC_SystemAdmin)
                return true;

            return role.HasTableAccess(tableName, 2);
        }

        /// <summary>
        /// Проверка специального права
        /// </summary>
        public bool HasSpecialPermission(string permissionName)
        {
            if (!_authenticationService.IsAuthenticated)
                return false;

            var role = _authenticationService.CurrentUserRole;
            if (role == null)
                return false;

            // Системный администратор имеет все права
            if (role.SPEC_SystemAdmin)
                return true;

            return permissionName switch
            {
                "ExportData" => role.SPEC_ExportData,
                "ViewReports" => role.SPEC_ViewReports,
                "ManageAllDepartments" => role.SPEC_ManageAllDepartments,
                "ManageUsers" => role.SPEC_ManageUsers,
                "ConfigureConnection" => role.SPEC_ConfigureConnection,
                _ => false
            };
        }

        /// <summary>
        /// Проверка доступа к отделу
        /// </summary>
        public async Task<bool> HasDepartmentAccessAsync(string departmentId)
        {
            if (!_authenticationService.IsAuthenticated)
                return false;

            var user = _authenticationService.CurrentUser;
            if (user == null)
                return false;

            // Администратор или право на все отделы
            if (IsSystemAdmin || user.HasAllDepartments ||
                _authenticationService.CurrentUserRole?.SPEC_ManageAllDepartments == true)
            {
                return true;
            }

            // Проверяем наличие доступа
            return await _databaseService.Context.UserDepartmentAccesses
                .AnyAsync(uda => uda.UserId == user.Id && uda.DepartmentId == departmentId);
        }

        /// <summary>
        /// Получение списка доступных отделов
        /// </summary>
        public async Task<List<Department>> GetAccessibleDepartmentsAsync()
        {
            if (!_authenticationService.IsAuthenticated)
                return new List<Department>();

            // Проверяем кэш
            if (_cachedDepartments != null && DateTime.Now - _cacheTime < _cacheDuration)
            {
                return _cachedDepartments;
            }

            var user = _authenticationService.CurrentUser;
            if (user == null)
                return new List<Department>();

            IQueryable<Department> query;

            // Администратор или право на все отделы
            if (IsSystemAdmin || user.HasAllDepartments ||
                _authenticationService.CurrentUserRole?.SPEC_ManageAllDepartments == true)
            {
                query = _databaseService.Context.Departments.Where(d => d.IsActive);
            }
            else
            {
                // Получаем отделы, к которым есть доступ
                query = from uda in _databaseService.Context.UserDepartmentAccesses
                        join d in _databaseService.Context.Departments on uda.DepartmentId equals d.Id
                        where uda.UserId == user.Id && d.IsActive
                        select d;
            }

            var departments = await query
                .OrderBy(d => d.Name)
                .ToListAsync();

            // Обновляем кэш
            _cachedDepartments = departments;
            _cacheTime = DateTime.Now;

            return departments;
        }

        /// <summary>
        /// Проверка доступа к складу
        /// </summary>
        public async Task<bool> HasWarehouseAccessAsync(string warehouseId)
        {
            if (!_authenticationService.IsAuthenticated)
                return false;

            var user = _authenticationService.CurrentUser;
            if (user == null)
                return false;

            // Администратор или право на все отделы
            if (IsSystemAdmin || user.HasAllDepartments ||
                _authenticationService.CurrentUserRole?.SPEC_ManageAllDepartments == true)
            {
                return true;
            }

            // Получаем доступ к отделу
            var departmentAccess = await _databaseService.Context.UserDepartmentAccesses
                .FirstOrDefaultAsync(uda => uda.UserId == user.Id &&
                    _databaseService.Context.Warehouses.Any(w => w.Id == warehouseId && w.DepartmentId == uda.DepartmentId));

            if (departmentAccess == null)
                return false;

            // Если есть доступ ко всем складам отдела
            if (departmentAccess.HasAllWarehouses)
                return true;

            // Проверяем доступ к конкретному складу
            return await _databaseService.Context.UserWarehouseAccesses
                .AnyAsync(uwa => uwa.UserDepartmentAccessKey == departmentAccess.Key &&
                                 uwa.WarehouseId == warehouseId);
        }

        /// <summary>
        /// Получение списка доступных складов
        /// </summary>
        public async Task<List<Warehouse>> GetAccessibleWarehousesAsync(string departmentId = null)
        {
            if (!_authenticationService.IsAuthenticated)
                return new List<Warehouse>();

            // Проверяем кэш
            if (_cachedWarehouses != null && DateTime.Now - _cacheTime < _cacheDuration)
            {
                if (departmentId == null)
                    return _cachedWarehouses;
                else
                    return _cachedWarehouses.Where(w => w.DepartmentId == departmentId).ToList();
            }

            var user = _authenticationService.CurrentUser;
            if (user == null)
                return new List<Warehouse>();

            IQueryable<Warehouse> query;

            // Администратор или право на все отделы
            if (IsSystemAdmin || user.HasAllDepartments ||
                _authenticationService.CurrentUserRole?.SPEC_ManageAllDepartments == true)
            {
                query = _databaseService.Context.Warehouses
                    .Include(w => w.Department)
                    .Where(w => w.IsActive);
            }
            else
            {
                // Получаем склады через доступ к отделам
                var departmentAccesses = await _databaseService.Context.UserDepartmentAccesses
                    .Where(uda => uda.UserId == user.Id)
                    .ToListAsync();

                var warehouseIds = new List<string>();

                foreach (var access in departmentAccesses)
                {
                    if (access.HasAllWarehouses)
                    {
                        // Все склады отдела
                        var deptWarehouses = await _databaseService.Context.Warehouses
                            .Where(w => w.DepartmentId == access.DepartmentId && w.IsActive)
                            .Select(w => w.Id)
                            .ToListAsync();
                        warehouseIds.AddRange(deptWarehouses);
                    }
                    else
                    {
                        // Конкретные склады
                        var accessWarehouses = await _databaseService.Context.UserWarehouseAccesses
                            .Where(uwa => uwa.UserDepartmentAccessKey == access.Key)
                            .Select(uwa => uwa.WarehouseId)
                            .ToListAsync();
                        warehouseIds.AddRange(accessWarehouses);
                    }
                }

                warehouseIds = warehouseIds.Distinct().ToList();

                query = _databaseService.Context.Warehouses
                    .Include(w => w.Department)
                    .Where(w => warehouseIds.Contains(w.Id) && w.IsActive);
            }

            if (!string.IsNullOrEmpty(departmentId))
            {
                query = query.Where(w => w.DepartmentId == departmentId);
            }

            var warehouses = await query
                .OrderBy(w => w.Department.Name)
                .ThenBy(w => w.Name)
                .ToListAsync();

            // Обновляем кэш
            _cachedWarehouses = warehouses;
            _cacheTime = DateTime.Now;

            return warehouses;
        }

        #endregion

        #region Приватные методы

        /// <summary>
        /// Очистка кэша
        /// </summary>
        private void ClearCache()
        {
            _cachedDepartments = null;
            _cachedWarehouses = null;
            _cacheTime = DateTime.MinValue;
        }

        #endregion
    }
}