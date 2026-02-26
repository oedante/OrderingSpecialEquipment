using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Сервис для работы с настройками пользователей
    /// </summary>
    public class UserSettingsService : IUserSettingsService
    {
        #region Поля

        private readonly IDbContextFactory _contextFactory;
        private readonly Dictionary<string, Dictionary<string, object>> _cache;
        private readonly object _cacheLock = new object();
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private DateTime _lastCacheUpdate = DateTime.MinValue;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор сервиса настроек пользователей
        /// </summary>
        public UserSettingsService(IDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _cache = new Dictionary<string, Dictionary<string, object>>();
        }

        #endregion

        #region Публичные асинхронные методы

        /// <summary>
        /// Получение настройки пользователя
        /// </summary>
        public async Task<T> GetSettingAsync<T>(string userId, string key, T defaultValue = default)
        {
            try
            {
                // Проверяем кэш
                if (TryGetFromCache<T>(userId, key, out var cachedValue))
                {
                    return cachedValue;
                }

                using var context = _contextFactory.CreateDbContext();

                var setting = await context.UserSettings
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.SettingKey == key);

                if (setting != null)
                {
                    try
                    {
                        var value = JsonSerializer.Deserialize<T>(setting.SettingValue);
                        AddToCache(userId, key, value);
                        return value;
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения настройки {key}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Сохранение настройки пользователя
        /// </summary>
        public async Task SaveSettingAsync<T>(string userId, string key, T value)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var setting = await context.UserSettings
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.SettingKey == key);

                var jsonValue = JsonSerializer.Serialize(value);

                if (setting == null)
                {
                    setting = new UserSetting
                    {
                        UserId = userId,
                        SettingKey = key,
                        SettingValue = jsonValue,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await context.UserSettings.AddAsync(setting);
                }
                else
                {
                    setting.SettingValue = jsonValue;
                    setting.UpdatedAt = DateTime.UtcNow;
                    context.UserSettings.Update(setting);
                }

                await context.SaveChangesAsync();

                // Обновляем кэш
                AddToCache(userId, key, value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настройки {key}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Удаление настройки пользователя
        /// </summary>
        public async Task DeleteSettingAsync(string userId, string key)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var setting = await context.UserSettings
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.SettingKey == key);

                if (setting != null)
                {
                    context.UserSettings.Remove(setting);
                    await context.SaveChangesAsync();
                }

                // Удаляем из кэша
                RemoveFromCache(userId, key);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления настройки {key}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получение всех настроек пользователя
        /// </summary>
        public async Task<Dictionary<string, object>> GetAllSettingsAsync(string userId)
        {
            try
            {
                // Проверяем кэш
                lock (_cacheLock)
                {
                    if (_cache.ContainsKey(userId) && DateTime.Now - _lastCacheUpdate < _cacheDuration)
                    {
                        return new Dictionary<string, object>(_cache[userId]);
                    }
                }

                using var context = _contextFactory.CreateDbContext();

                var settings = await context.UserSettings
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                var result = new Dictionary<string, object>();

                foreach (var setting in settings)
                {
                    try
                    {
                        // Пытаемся определить тип и десериализовать
                        var jsonElement = JsonSerializer.Deserialize<JsonElement>(setting.SettingValue);
                        switch (jsonElement.ValueKind)
                        {
                            case JsonValueKind.String:
                                result[setting.SettingKey] = jsonElement.GetString();
                                break;
                            case JsonValueKind.Number:
                                if (jsonElement.TryGetInt32(out int intValue))
                                    result[setting.SettingKey] = intValue;
                                else if (jsonElement.TryGetDouble(out double doubleValue))
                                    result[setting.SettingKey] = doubleValue;
                                else if (jsonElement.TryGetDecimal(out decimal decimalValue))
                                    result[setting.SettingKey] = decimalValue;
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                result[setting.SettingKey] = jsonElement.GetBoolean();
                                break;
                            default:
                                result[setting.SettingKey] = jsonElement.ToString();
                                break;
                        }
                    }
                    catch
                    {
                        result[setting.SettingKey] = setting.SettingValue;
                    }
                }

                // Обновляем кэш
                lock (_cacheLock)
                {
                    _cache[userId] = result;
                    _lastCacheUpdate = DateTime.Now;
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения всех настроек: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Сохранение нескольких настроек пользователя
        /// </summary>
        public async Task SaveSettingsAsync(string userId, Dictionary<string, object> settings)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                foreach (var kvp in settings)
                {
                    var existing = await context.UserSettings
                        .FirstOrDefaultAsync(s => s.UserId == userId && s.SettingKey == kvp.Key);

                    var jsonValue = JsonSerializer.Serialize(kvp.Value);

                    if (existing == null)
                    {
                        await context.UserSettings.AddAsync(new UserSetting
                        {
                            UserId = userId,
                            SettingKey = kvp.Key,
                            SettingValue = jsonValue,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        existing.SettingValue = jsonValue;
                        existing.UpdatedAt = DateTime.UtcNow;
                        context.UserSettings.Update(existing);
                    }

                    // Обновляем кэш
                    AddToCache(userId, kvp.Key, kvp.Value);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения нескольких настроек: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Очистка всех настроек пользователя
        /// </summary>
        public async Task ClearAllSettingsAsync(string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var settings = await context.UserSettings
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                if (settings.Any())
                {
                    context.UserSettings.RemoveRange(settings);
                    await context.SaveChangesAsync();
                }

                // Очищаем кэш
                lock (_cacheLock)
                {
                    if (_cache.ContainsKey(userId))
                    {
                        _cache.Remove(userId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка очистки настроек: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Синхронные методы (обертки)

        /// <summary>
        /// Получение настройки пользователя (синхронный)
        /// </summary>
        public T GetSetting<T>(string userId, string key, T defaultValue = default)
        {
            return Task.Run(async () => await GetSettingAsync(userId, key, defaultValue)).Result;
        }

        /// <summary>
        /// Сохранение настройки пользователя (синхронный)
        /// </summary>
        public void SaveSetting<T>(string userId, string key, T value)
        {
            Task.Run(async () => await SaveSettingAsync(userId, key, value)).Wait();
        }

        #endregion

        #region Приватные методы для работы с кэшем

        private bool TryGetFromCache<T>(string userId, string key, out T value)
        {
            value = default;
            lock (_cacheLock)
            {
                if (_cache.ContainsKey(userId) &&
                    _cache[userId].ContainsKey(key) &&
                    DateTime.Now - _lastCacheUpdate < _cacheDuration)
                {
                    try
                    {
                        var cachedValue = _cache[userId][key];
                        if (cachedValue is T tValue)
                        {
                            value = tValue;
                            return true;
                        }
                        // Пытаемся конвертировать
                        value = (T)Convert.ChangeType(cachedValue, typeof(T));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        private void AddToCache(string userId, string key, object value)
        {
            lock (_cacheLock)
            {
                if (!_cache.ContainsKey(userId))
                {
                    _cache[userId] = new Dictionary<string, object>();
                }
                _cache[userId][key] = value;
                _lastCacheUpdate = DateTime.Now;
            }
        }

        private void RemoveFromCache(string userId, string key)
        {
            lock (_cacheLock)
            {
                if (_cache.ContainsKey(userId) && _cache[userId].ContainsKey(key))
                {
                    _cache[userId].Remove(key);
                    _lastCacheUpdate = DateTime.Now;
                }
            }
        }

        #endregion
    }
}