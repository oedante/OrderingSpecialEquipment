using System.Windows;

namespace OrderingSpecialEquipment.Services.Interfaces
{
    public interface IThemeService
    {
        bool IsDarkTheme { get; }
        void ToggleTheme();
        void ApplyTheme(bool isDark);
        void LoadThemeResources(); // Добавлен новый метод
        event EventHandler<bool> ThemeChanged;
    }
}