using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IThemeService для управления темой оформления.
    /// </summary>
    public class ThemeService : IThemeService
    {
        private readonly IConfiguration _configuration;
        private readonly string _appSettingsPath; // Путь к appsettings.json

        public ThemeService(IConfiguration configuration) // Принимаем IConfiguration через DI
        {
            _configuration = configuration;
            // Получаем путь к appsettings.json. Host.CreateDefaultBuilder загружает его из базовой директории.
            // Базовая директория для WPF приложения - это папка bin\x64\Debug\net10.0-windows\ или Release.
            // Путь к файлу конфигурации обычно не доступен напрямую через IConfiguration.
            // Найдём его вручную.
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _appSettingsPath = Path.Combine(baseDir, "appsettings.json");
            Console.WriteLine($"ThemeService: Ожидаемый путь к appsettings.json: {_appSettingsPath}");
        }

        public void ApplyTheme(string themeName)
        {
            try
            {
                // Найдём и удалим старую тему из MergedDictionaries
                var dictsToRemove = new List<ResourceDictionary>();
                foreach (var dict in Application.Current.Resources.MergedDictionaries)
                {
                    if (dict.Source?.OriginalString.Contains("LightTheme.xaml") == true ||
                        dict.Source?.OriginalString.Contains("DarkTheme.xaml") == true)
                    {
                        dictsToRemove.Add(dict);
                    }
                }
                foreach (var dict in dictsToRemove)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(dict);
                }

                // Определим путь к новой теме
                string themeResourcePath = "/Themes/";
                switch (themeName.ToLower())
                {
                    case "light":
                        themeResourcePath += "LightTheme.xaml";
                        break;
                    case "dark":
                        themeResourcePath += "DarkTheme.xaml";
                        break;
                    default:
                        themeResourcePath += "LightTheme.xaml"; // Значение по умолчанию
                        Console.WriteLine($"ThemeService: Неизвестная тема '{themeName}', используется 'Light'.");
                        break;
                }

                // Загрузим и добавим новую тему
                var newThemeDict = new ResourceDictionary { Source = new Uri(themeResourcePath, UriKind.Relative) };
                Application.Current.Resources.MergedDictionaries.Add(newThemeDict);

                Console.WriteLine($"ThemeService: Применена тема '{themeName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ThemeService: Ошибка применения темы '{themeName}': {ex.Message}");
                // Логировать через ILogger
            }
        }

        public string LoadThemePreference()
        {
            // Получаем сохранённую тему из IConfiguration (appsettings.json)
            var savedTheme = _configuration["AppSettings:Theme"] ?? "Light"; // Значение по умолчанию
            Console.WriteLine($"ThemeService: Загружена предпочтительная тема: {savedTheme}");
            return savedTheme;
        }

        // В ThemeService.cs
        public void SaveThemePreference(string themeName)
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                var jsonString = File.ReadAllText(path);
                var jsonObj = JObject.Parse(jsonString);

                jsonObj["AppSettings"]["Theme"] = themeName;

                File.WriteAllText(path, jsonObj.ToString(Formatting.Indented));

                Console.WriteLine($"ThemeService: Предпочтение темы '{themeName}' сохранено в appsettings.json.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ThemeService: Ошибка сохранения темы в appsettings.json: {ex.Message}");
                // Логировать через ILogger
            }
        }

        public string[] GetAvailableThemes()
        {
            return new[] { "Light", "Dark" };
        }
    }
}