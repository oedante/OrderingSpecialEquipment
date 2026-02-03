using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация сервиса управления темами оформления
    /// Загружает и применяет темы из файлов ресурсов
    /// Сохраняет предпочтения в appsettings.json
    /// </summary>
    public class ThemeService : IThemeService
    {
        private readonly IConfiguration _configuration;
        private readonly string _configFilePath;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public ThemeService(IConfiguration configuration)
        {
            _configuration = configuration;
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        }

        /// <summary>
        /// Применяет указанную тему
        /// Загружает файл темы и добавляет его в ресурсы приложения
        /// </summary>
        public void ApplyTheme(string themeName)
        {
            try
            {
                // Определяем путь к файлу темы
                string themeFile = themeName.ToLower() switch
                {
                    "dark" => "Themes/DarkTheme.xaml",
                    _ => "Themes/LightTheme.xaml" // По умолчанию светлая тема
                };

                // Создаем новый ресурсный словарь для темы
                ResourceDictionary themeDict = new ResourceDictionary
                {
                    Source = new Uri(themeFile, UriKind.Relative)
                };

                // Получаем текущие объединенные словари
                var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

                // Удаляем старые темы (если есть)
                for (int i = mergedDictionaries.Count - 1; i >= 0; i--)
                {
                    if (mergedDictionaries[i].Source?.ToString().Contains("Theme.xaml") == true)
                    {
                        mergedDictionaries.RemoveAt(i);
                    }
                }

                // Добавляем новую тему
                mergedDictionaries.Add(themeDict);

                Console.WriteLine($"Тема '{themeName}' успешно применена");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при применении темы {themeName}: {ex.Message}");
                // Применяем светлую тему по умолчанию
                ApplyTheme("Light");
            }
        }

        /// <summary>
        /// Загружает предпочтения темы из конфигурации
        /// </summary>
        public string LoadThemePreference()
        {
            try
            {
                string? theme = _configuration["AppSettings:Theme"];
                return string.IsNullOrEmpty(theme) ? "Light" : theme;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке темы: {ex.Message}");
                return "Light"; // По умолчанию светлая тема
            }
        }

        /// <summary>
        /// Сохраняет предпочтения темы в appsettings.json
        /// </summary>
        public async Task SaveThemePreferenceAsync(string themeName)
        {
            try
            {
                // Читаем текущий файл конфигурации
                string json = await File.ReadAllTextAsync(_configFilePath);
                JObject config = JObject.Parse(json);

                // Обновляем значение темы
                if (config["AppSettings"] == null)
                {
                    config["AppSettings"] = new JObject();
                }
                config["AppSettings"]["Theme"] = themeName;

                // Записываем обратно в файл
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(_configFilePath, updatedJson);

                Console.WriteLine($"Тема '{themeName}' сохранена в конфигурации");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении темы: {ex.Message}");
            }
        }
    }
}