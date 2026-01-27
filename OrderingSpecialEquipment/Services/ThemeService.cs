using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IThemeService для управления темами оформления.
    /// Темы определяются через ресурсы в Themes.xaml.
    /// </summary>
    public class ThemeService : IThemeService
    {
        private const string SettingsFileName = "AppSettings.json"; // Можно использовать и стандартный Properties.Settings
        private const string ThemeKey = "SelectedTheme";
        private readonly string _settingsFilePath;

        public ThemeService()
        {
            // Путь к файлу настроек в папке приложения
            _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
        }

        public void ApplyTheme(string themeName)
        {
            try
            {
                var app = Application.Current;
                if (app == null) return;

                // Удаляем предыдущие ресурсы темы (если они были загружены из Themes.xaml)
                // Предполагаем, что все темы находятся в Themes.xaml
                var existingResourceDicts = app.Resources.MergedDictionaries
                    .Where(rd => rd.Source?.OriginalString?.Contains("Themes.xaml") == true)
                    .ToList();

                foreach (var dict in existingResourceDicts)
                {
                    app.Resources.MergedDictionaries.Remove(dict);
                }

                // Загружаем новый файл темы
                // Предположим, у нас есть файл Themes.xaml в папке Themes, и в нем ResourceDictionary с ключом "LightTheme" и "DarkTheme"
                // Но проще всего, если каждая тема - отдельный файл XAML.
                // Пусть будет Themes/LightTheme.xaml и Themes/DarkTheme.xaml
                string themeUri = $"pack://application:,,,/Themes/{themeName}.xaml";
                var resourceDict = new ResourceDictionary { Source = new Uri(themeUri) };
                app.Resources.MergedDictionaries.Add(resourceDict);

                Console.WriteLine($"Тема '{themeName}' применена.");
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при применении темы '{themeName}': {ex.Message}");
            }
        }

        public void SaveThemePreference(string themeName)
        {
            try
            {
                // Простое сохранение в JSON файл. Для продвинутого использования можно использовать Properties.Settings.
                var settings = System.Text.Json.JsonSerializer.Serialize(new { SelectedTheme = themeName });
                File.WriteAllText(_settingsFilePath, settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении настроек темы: {ex.Message}");
            }
        }

        public string LoadThemePreference()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var jsonContent = File.ReadAllText(_settingsFilePath);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<DynamicSettings>(jsonContent);
                    return settings.SelectedTheme ?? "Light"; // Значение по умолчанию
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке настроек темы: {ex.Message}");
            }
            return "Light"; // Значение по умолчанию, если файл не найден или ошибка
        }

        public string[] GetAvailableThemes()
        {
            // Возвращает список тем, основываясь на наличии файлов в папке Themes
            string themesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
            if (Directory.Exists(themesDir))
            {
                return Directory.GetFiles(themesDir, "*.xaml")
                                .Select(Path.GetFileNameWithoutExtension)
                                .ToArray();
            }
            return new string[] { "Light", "Dark" }; // Значения по умолчанию
        }

        // Вспомогательный класс для десериализации JSON
        private class DynamicSettings
        {
            public string? SelectedTheme { get; set; }
        }
    }
}