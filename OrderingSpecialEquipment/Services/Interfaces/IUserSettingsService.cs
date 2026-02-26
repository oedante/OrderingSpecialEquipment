using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с настройками пользователей
    /// </summary>
    public interface IUserSettingsService
    {
        /// <summary>
        /// Получение настройки пользователя
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="userId">ID пользователя</param>
        /// <param name="key">Ключ настройки</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns>Значение настройки</returns>
        Task<T> GetSettingAsync<T>(string userId, string key, T defaultValue = default);

        /// <summary>
        /// Сохранение настройки пользователя
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="userId">ID пользователя</param>
        /// <param name="key">Ключ настройки</param>
        /// <param name="value">Значение</param>
        Task SaveSettingAsync<T>(string userId, string key, T value);

        /// <summary>
        /// Удаление настройки пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="key">Ключ настройки</param>
        Task DeleteSettingAsync(string userId, string key);

        /// <summary>
        /// Получение всех настроек пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Словарь настроек</returns>
        Task<Dictionary<string, object>> GetAllSettingsAsync(string userId);

        /// <summary>
        /// Сохранение нескольких настроек пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="settings">Словарь настроек</param>
        Task SaveSettingsAsync(string userId, Dictionary<string, object> settings);

        /// <summary>
        /// Очистка всех настроек пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        Task ClearAllSettingsAsync(string userId);

        /// <summary>
        /// Получение настройки пользователя (синхронный метод для удобства)
        /// </summary>
        T GetSetting<T>(string userId, string key, T defaultValue = default);

        /// <summary>
        /// Сохранение настройки пользователя (синхронный метод для удобства)
        /// </summary>
        void SaveSetting<T>(string userId, string key, T value);
    }
}