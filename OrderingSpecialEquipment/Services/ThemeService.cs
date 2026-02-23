using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.Utils;
using System;
using System.Windows;
using System.Windows.Media;

namespace OrderingSpecialEquipment.Services
{
    public class ThemeService : IThemeService
    {
        private const string THEME_SETTING_KEY = "Theme";
        private bool _isDarkTheme;

        public event EventHandler<bool> ThemeChanged;

        public bool IsDarkTheme => _isDarkTheme;

        public ThemeService()
        {
            LoadTheme();
        }

        private void LoadTheme()
        {
            var savedTheme = SettingsHelper.LoadSetting(THEME_SETTING_KEY, "Light");
            _isDarkTheme = savedTheme == "Dark";
        }

        public void LoadThemeResources()
        {
            try
            {
                // Очищаем существующие ресурсы
                Application.Current.Resources.MergedDictionaries.Clear();

                // Сначала загружаем тему (определяет цвета и кисти)
                string themeFile = _isDarkTheme
                    ? "Resources/Themes/DarkTheme.xaml"
                    : "Resources/Themes/LightTheme.xaml";

                Application.Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary { Source = new Uri(themeFile, UriKind.Relative) });

                // Потом загружаем стили (используют ресурсы из темы)
                Application.Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary { Source = new Uri("Resources/Styles.xaml", UriKind.Relative) });

                System.Diagnostics.Debug.WriteLine($"Тема загружена: {(_isDarkTheme ? "темная" : "светлая")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки темы: {ex.Message}");
            }
        }

        public void ToggleTheme()
        {
            ApplyTheme(!_isDarkTheme);
        }

        public void ApplyTheme(bool isDark)
        {
            try
            {
                // Сохраняем настройку
                SettingsHelper.SaveSetting(THEME_SETTING_KEY, isDark ? "Dark" : "Light");

                _isDarkTheme = isDark;

                // Перезагружаем ресурсы с новой темой
                LoadThemeResources();

                // Принудительно обновляем все окна
                foreach (Window window in Application.Current.Windows)
                {
                    // Обновляем фон окна
                    window.Background = (SolidColorBrush)Application.Current.Resources["WindowBackgroundBrush"];

                    // Принудительно обновляем все привязки
                    var viewModel = window.DataContext as ViewModels.ViewModelBase;
                    viewModel?.OnPropertyChanged(string.Empty);
                }

                ThemeChanged?.Invoke(this, isDark);

                System.Diagnostics.Debug.WriteLine($"Тема изменена на {(isDark ? "темную" : "светлую")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при смене темы: {ex.Message}");
            }
        }
    }
}