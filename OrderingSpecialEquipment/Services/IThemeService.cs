namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс для сервиса управления темами оформления (светлая/темная).
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Применяет выбранную тему.
        /// </summary>
        /// <param name="themeName">Название темы (например, "Light", "Dark").</param>
        void ApplyTheme(string themeName);

        /// <summary>
        /// Сохраняет выбранную тему в настройках приложения.
        /// </summary>
        /// <param name="themeName">Название темы.</param>
        void SaveThemePreference(string themeName);

        /// <summary>
        /// Загружает сохраненную тему из настроек приложения.
        /// </summary>
        /// <returns>Название сохраненной темы.</returns>
        string LoadThemePreference();

        /// <summary>
        /// Получает список доступных тем.
        /// </summary>
        /// <returns>Массив названий тем.</returns>
        string[] GetAvailableThemes();
    }
}