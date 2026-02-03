using OrderingSpecialEquipment.Models;
using System;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс сервиса авторизации
    /// Проверяет права доступа пользователя к различным операциям
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Проверяет право на чтение таблицы
        /// </summary>
        bool CanReadTable(User user, string tableName);

        /// <summary>
        /// Проверяет право на запись в таблицу
        /// </summary>
        bool CanWriteTable(User user, string tableName);

        /// <summary>
        /// Проверяет специальное право
        /// </summary>
        bool HasSpecialPermission(User user, string permissionName);

        /// <summary>
        /// Проверяет доступ пользователя к отделу
        /// </summary>
        Task<bool> HasAccessToDepartmentAsync(User user, string departmentId);

        /// <summary>
        /// Проверяет доступ пользователя к складу
        /// </summary>
        Task<bool> HasAccessToWarehouseAsync(User user, string warehouseId);

        /// <summary>
        /// Проверяет доступ пользователя ко всем отделам
        /// </summary>
        bool HasAccessToAllDepartments(User user);
    }
}