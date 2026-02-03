using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс сервиса управления темами оформления
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Применяет указанную тему
        /// </summary>
        void ApplyTheme(string themeName);

        /// <summary>
        /// Загружает предпочтения темы из конфигурации
        /// </summary>
        string LoadThemePreference();

        /// <summary>
        /// Сохраняет предпочтения темы в конфигурацию
        /// </summary>
        Task SaveThemePreferenceAsync(string themeName);
    }
}