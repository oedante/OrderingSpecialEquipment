using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Extensions
{
    /// <summary>
    /// Расширения для DbContext для удобной работы с данными
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Получает активные отделы с проверкой доступа пользователя
        /// </summary>
        public static async Task<List<Department>> GetAccessibleDepartmentsAsync(
            this ApplicationDbContext context,
            User user,
            IAuthorizationService authorizationService)
        {
            var allDepartments = await context.Departments
                .Where(d => d.IsActive)
                .ToListAsync();

            if (authorizationService.HasAccessToAllDepartments(user))
                return allDepartments;

            var accessibleDepartments = new List<Department>();

            foreach (var dept in allDepartments)
            {
                if (await authorizationService.HasAccessToDepartmentAsync(user, dept.Id))
                {
                    accessibleDepartments.Add(dept);
                }
            }

            return accessibleDepartments;
        }

        /// <summary>
        /// Получает активные склады с проверкой доступа пользователя
        /// </summary>
        public static async Task<List<Warehouse>> GetAccessibleWarehousesAsync(
            this ApplicationDbContext context,
            User user,
            IAuthorizationService authorizationService)
        {
            var allWarehouses = await context.Warehouses
                .Include(w => w.Department)
                .Where(w => w.IsActive && w.Department.IsActive)
                .ToListAsync();

            if (authorizationService.HasAccessToAllDepartments(user))
                return allWarehouses;

            var accessibleWarehouses = new List<Warehouse>();

            foreach (var warehouse in allWarehouses)
            {
                if (await authorizationService.HasAccessToWarehouseAsync(user, warehouse.Id))
                {
                    accessibleWarehouses.Add(warehouse);
                }
            }

            return accessibleWarehouses;
        }

        /// <summary>
        /// Защита от SQL инъекций: экранирование строки для LIKE
        /// </summary>
        public static string EscapeLikePattern(this string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return pattern;

            // Экранируем специальные символы LIKE: % _ [
            return pattern
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");
        }
    }
}