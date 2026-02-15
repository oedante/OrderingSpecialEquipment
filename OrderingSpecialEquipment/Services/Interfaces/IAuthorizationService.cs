using OrderingSpecialEquipment.Models;

namespace OrderingSpecialEquipment.Services.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса авторизации
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Проверка права на чтение таблицы
        /// </summary>
        bool CanReadTable(string tableName);

        /// <summary>
        /// Проверка права на запись в таблицу
        /// </summary>
        bool CanWriteTable(string tableName);

        /// <summary>
        /// Проверка специального права
        /// </summary>
        bool HasSpecialPermission(string permissionName);

        /// <summary>
        /// Проверка доступа к отделу
        /// </summary>
        Task<bool> HasDepartmentAccessAsync(string departmentId);

        /// <summary>
        /// Получение списка доступных отделов
        /// </summary>
        Task<List<Department>> GetAccessibleDepartmentsAsync();

        /// <summary>
        /// Проверка доступа к складу
        /// </summary>
        Task<bool> HasWarehouseAccessAsync(string warehouseId);

        /// <summary>
        /// Получение списка доступных складов
        /// </summary>
        Task<List<Warehouse>> GetAccessibleWarehousesAsync(string departmentId = null);

        /// <summary>
        /// Является ли пользователь системным администратором
        /// </summary>
        bool IsSystemAdmin { get; }
    }
}