using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация сервиса авторизации
    /// Проверяет права доступа на основе роли пользователя
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public AuthorizationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Проверяет право на чтение таблицы
        /// </summary>
        public bool CanReadTable(User user, string tableName)
        {
            if (user.Role == null)
                return false;

            // Получаем значение права для таблицы
            short permission = GetTablePermission(user.Role, tableName);
            return permission >= 1; // 1 = Чтение, 2 = Запись
        }

        /// <summary>
        /// Проверяет право на запись в таблицу
        /// </summary>
        public bool CanWriteTable(User user, string tableName)
        {
            if (user.Role == null)
                return false;

            // Получаем значение права для таблицы
            short permission = GetTablePermission(user.Role, tableName);
            return permission >= 2; // 2 = Запись
        }

        /// <summary>
        /// Проверяет специальное право
        /// </summary>
        public bool HasSpecialPermission(User user, string permissionName)
        {
            if (user.Role == null)
                return false;

            return permissionName switch
            {
                "ExportData" => user.Role.SPEC_ExportData,
                "ViewReports" => user.Role.SPEC_ViewReports,
                "ManageAllDepartments" => user.Role.SPEC_ManageAllDepartments,
                "ManageUsers" => user.Role.SPEC_ManageUsers,
                "SystemAdmin" => user.Role.SPEC_SystemAdmin,
                _ => false
            };
        }

        /// <summary>
        /// Проверяет доступ пользователя к отделу
        /// </summary>
        public async Task<bool> HasAccessToDepartmentAsync(User user, string departmentId)
        {
            // Если у пользователя полный доступ ко всем отделам
            if (user.HasAllDepartments || HasSpecialPermission(user, "ManageAllDepartments"))
                return true;

            // Создаем новый сервис скоуп для работы с репозиторием
            using (var scope = _serviceProvider.CreateScope())
            {
                var userDeptAccessRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<UserDepartmentAccess>>();

                // Проверяем, есть ли запись доступа
                var access = await userDeptAccessRepo.FindAsync(a =>
                    a.UserId == user.Id && a.DepartmentId == departmentId);

                return access.Any();
            }
        }

        /// <summary>
        /// Проверяет доступ пользователя к складу
        /// </summary>
        public async Task<bool> HasAccessToWarehouseAsync(User user, string warehouseId)
        {
            // Если у пользователя полный доступ ко всем отделам
            if (user.HasAllDepartments || HasSpecialPermission(user, "ManageAllDepartments"))
                return true;

            // Создаем новый сервис скоуп для работы с репозиторием
            using (var scope = _serviceProvider.CreateScope())
            {
                var warehouseRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Warehouse>>();
                var userDeptAccessRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<UserDepartmentAccess>>();
                var userWarehouseAccessRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<UserWarehouseAccess>>();

                // Получаем склад
                var warehouse = await warehouseRepo.GetByIdAsync(warehouseId);
                if (warehouse == null)
                    return false;

                // Проверяем доступ к отделу склада
                bool hasDeptAccess = await HasAccessToDepartmentAsync(user, warehouse.DepartmentId);
                if (!hasDeptAccess)
                    return false;

                // Получаем доступ пользователя к отделу
                var deptAccess = await userDeptAccessRepo.FindAsync(a =>
                    a.UserId == user.Id && a.DepartmentId == warehouse.DepartmentId);

                if (!deptAccess.Any())
                    return false;

                // Если у пользователя доступ ко всем складам отдела
                if (deptAccess.First().HasAllWarehouses)
                    return true;

                // Проверяем конкретный доступ к складу
                var access = await userWarehouseAccessRepo.FindAsync(a =>
                    a.UserDepartmentAccessKey == deptAccess.First().Key &&
                    a.WarehouseId == warehouseId);

                return access.Any();
            }
        }

        /// <summary>
        /// Проверяет доступ пользователя ко всем отделам
        /// </summary>
        public bool HasAccessToAllDepartments(User user)
        {
            return user.HasAllDepartments || HasSpecialPermission(user, "ManageAllDepartments");
        }

        /// <summary>
        /// Получает значение права для таблицы из роли
        /// </summary>
        private short GetTablePermission(Role role, string tableName)
        {
            return tableName switch
            {
                "AuditLogs" => role.TAB_AuditLogs,
                "Departments" => role.TAB_Departments,
                "EquipmentDependencies" => role.TAB_EquipmentDependencies,
                "Equipments" => role.TAB_Equipments,
                "LessorOrganizations" => role.TAB_LessorOrganizations,
                "LicensePlates" => role.TAB_LicensePlates,
                "Roles" => role.TAB_Roles,
                "ShiftRequests" => role.TAB_ShiftRequests,
                "TransportProgram" => role.TAB_TransportProgram,
                "UserDepartmentAccess" => role.TAB_UserDepartmentAccess,
                "UserFavorites" => role.TAB_UserFavorites,
                "Users" => role.TAB_Users,
                "UserWarehouseAccess" => role.TAB_UserWarehouseAccess,
                "WarehouseAreas" => role.TAB_WarehouseAreas,
                "Warehouses" => role.TAB_Warehouses,
                _ => 0
            };
        }
    }
}