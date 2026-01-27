using OrderingSpecialEquipment.Models;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс для сервиса авторизации.
    /// Проверяет права доступа пользователя к таблицам и специальным функциям.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Проверяет, имеет ли текущий пользователь право на чтение указанной таблицы.
        /// </summary>
        /// <param name="tableName">Название таблицы (например, "Departments").</param>
        /// <returns>True, если доступ разрешен.</returns>
        bool CanReadTable(string tableName);

        /// <summary>
        /// Проверяет, имеет ли текущий пользователь право на запись (создание/обновление/удаление) в указанную таблицу.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        /// <returns>True, если доступ разрешен.</returns>
        bool CanWriteTable(string tableName);

        /// <summary>
        /// Проверяет, имеет ли текущий пользователь специальное право.
        /// </summary>
        /// <param name="permissionName">Название специального права (например, "SPEC_ExportData").</param>
        /// <returns>True, если право установлено.</returns>
        bool HasSpecialPermission(string permissionName);

        /// <summary>
        /// Проверяет, имеет ли пользователь доступ к указанному отделу.
        /// </summary>
        /// <param name="departmentId">ID отдела.</param>
        /// <returns>True, если доступ разрешен.</returns>
        bool CanAccessDepartment(string departmentId);

        /// <summary>
        /// Проверяет, имеет ли пользователь доступ к указанному складу.
        /// </summary>
        /// <param name="warehouseId">ID склада.</param>
        /// <returns>True, если доступ разрешен.</returns>
        bool CanAccessWarehouse(string warehouseId);

        /// <summary>
        /// Инициализирует сервис с данными текущего пользователя.
        /// </summary>
        /// <param name="user">Текущий аутентифицированный пользователь.</param>
        void InitializeForUser(User user);

        /// <summary>
        /// Возвращает роль текущего аутентифицированного пользователя.
        /// </summary>
        /// <returns>Модель Role или null, если пользователь не аутентифицирован или роль не загружена.</returns>
        Role? GetCurrentUserRole();

        /// <summary>
        /// Проверяет, является ли текущий пользователь системным администратором.
        /// </summary>
        /// <returns>True, если пользователь является системным администратором.</returns>
        bool IsCurrentUserSystemAdmin();
    }
}