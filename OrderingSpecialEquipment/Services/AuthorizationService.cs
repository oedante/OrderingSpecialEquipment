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
        private readonly IDbContextFactory _contextFactory;
        private List<Department>? _cachedDepartments;
        private List<Warehouse>? _cachedWarehouses;
        private DateTime _cacheTime = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private readonly object _cacheLock = new object();

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
        /// <param name="contextFactory">Фабрика контекстов БД</param>
        public AuthorizationService(
            IAuthenticationService authenticationService,
            IDbContextFactory contextFactory)
        {
            _authenticationService = authenticationService;
            _contextFactory = contextFactory;

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

            using var context = _contextFactory.CreateDbContext();

            // Проверяем наличие доступа
            return await context.UserDepartmentAccesses
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
            lock (_cacheLock)
            {
                if (_cachedDepartments != null && DateTime.Now - _cacheTime < _cacheDuration)
                {
                    return _cachedDepartments;
                }
            }

            var user = _authenticationService.CurrentUser;
            if (user == null)
                return new List<Department>();

            using var context = _contextFactory.CreateDbContext();
            List<Department> departments;

            // Администратор или право на все отделы
            if (IsSystemAdmin || user.HasAllDepartments ||
                _authenticationService.CurrentUserRole?.SPEC_ManageAllDepartments == true)
            {
                departments = await context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
            else
            {
                // Получаем отделы, к которым есть доступ
                departments = await (from uda in context.UserDepartmentAccesses
                                     join d in context.Departments on uda.DepartmentId equals d.Id
                                     where uda.UserId == user.Id && d.IsActive
                                     orderby d.Name
                                     select d).ToListAsync();
            }

            // Обновляем кэш
            lock (_cacheLock)
            {
                _cachedDepartments = departments;
                _cacheTime = DateTime.Now;
            }

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

            using var context = _contextFactory.CreateDbContext();

            // Получаем доступ к отделу
            var departmentAccess = await context.UserDepartmentAccesses
                .FirstOrDefaultAsync(uda => uda.UserId == user.Id &&
                    context.Warehouses.Any(w => w.Id == warehouseId && w.DepartmentId == uda.DepartmentId));

            if (departmentAccess == null)
                return false;

            // Если есть доступ ко всем складам отдела
            if (departmentAccess.HasAllWarehouses)
                return true;

            // Проверяем доступ к конкретному складу
            return await context.UserWarehouseAccesses
                .AnyAsync(uwa => uwa.UserDepartmentAccessKey == departmentAccess.Key &&
                                 uwa.WarehouseId == warehouseId);
        }

        /// <summary>
        /// Получение списка доступных складов
        /// </summary>
        public async Task<List<Warehouse>> GetAccessibleWarehousesAsync(string? departmentId = null)
        {
            if (!_authenticationService.IsAuthenticated)
                return new List<Warehouse>();

            // Проверяем кэш
            lock (_cacheLock)
            {
                if (_cachedWarehouses != null && DateTime.Now - _cacheTime < _cacheDuration)
                {
                    if (departmentId == null)
                        return _cachedWarehouses;
                    else
                        return _cachedWarehouses.Where(w => w.DepartmentId == departmentId).ToList();
                }
            }

            var user = _authenticationService.CurrentUser;
            if (user == null)
                return new List<Warehouse>();

            using var context = _contextFactory.CreateDbContext();
            List<Warehouse> warehouses;

            // Администратор или право на все отделы
            if (IsSystemAdmin || user.HasAllDepartments ||
                _authenticationService.CurrentUserRole?.SPEC_ManageAllDepartments == true)
            {
                var query = context.Warehouses
                    .Include(w => w.Department)
                    .Where(w => w.IsActive);

                if (!string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(w => w.DepartmentId == departmentId);
                }

                warehouses = await query
                    .OrderBy(w => w.Department.Name)
                    .ThenBy(w => w.Name)
                    .ToListAsync();
            }
            else
            {
                // Получаем склады через доступ к отделам
                var departmentAccesses = await context.UserDepartmentAccesses
                    .Where(uda => uda.UserId == user.Id)
                    .ToListAsync();

                var warehouseIds = new List<string>();

                foreach (var access in departmentAccesses)
                {
                    if (access.HasAllWarehouses)
                    {
                        // Все склады отдела
                        var deptWarehouses = await context.Warehouses
                            .Where(w => w.DepartmentId == access.DepartmentId && w.IsActive)
                            .Select(w => w.Id)
                            .ToListAsync();
                        warehouseIds.AddRange(deptWarehouses);
                    }
                    else
                    {
                        // Конкретные склады
                        var accessWarehouses = await context.UserWarehouseAccesses
                            .Where(uwa => uwa.UserDepartmentAccessKey == access.Key)
                            .Select(uwa => uwa.WarehouseId)
                            .ToListAsync();
                        warehouseIds.AddRange(accessWarehouses);
                    }
                }

                warehouseIds = warehouseIds.Distinct().ToList();

                var query = context.Warehouses
                    .Include(w => w.Department)
                    .Where(w => warehouseIds.Contains(w.Id) && w.IsActive);

                if (!string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(w => w.DepartmentId == departmentId);
                }

                warehouses = await query
                    .OrderBy(w => w.Department.Name)
                    .ThenBy(w => w.Name)
                    .ToListAsync();
            }

            // Обновляем кэш
            lock (_cacheLock)
            {
                _cachedWarehouses = warehouses;
                _cacheTime = DateTime.Now;
            }

            return warehouses;
        }

        #endregion

        #region Приватные методы

        /// <summary>
        /// Очистка кэша
        /// </summary>
        private void ClearCache()
        {
            lock (_cacheLock)
            {
                _cachedDepartments = null;
                _cachedWarehouses = null;
                _cacheTime = DateTime.MinValue;
            }
        }

        #endregion
    }
}