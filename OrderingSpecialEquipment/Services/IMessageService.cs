using System.Windows;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс для сервиса показа сообщений пользователю.
    /// Инкапсулирует вызов MessageBox.Show и другие UI-зависимые операции.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Показывает информационное сообщение.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="title">Заголовок окна сообщения. По умолчанию "Информация".</param>
        void ShowInfoMessage(string message, string title = "Информация");

        /// <summary>
        /// Показывает сообщение об ошибке.
        /// </summary>
        /// <param name="message">Текст сообщения об ошибке.</param>
        /// <param name="title">Заголовок окна сообщения. По умолчанию "Ошибка".</param>
        void ShowErrorMessage(string message, string title = "Ошибка");

        /// <summary>
        /// Показывает предупреждение.
        /// </summary>
        /// <param name="message">Текст предупреждения.</param>
        /// <param name="title">Заголовок окна сообщения. По умолчанию "Предупреждение".</param>
        void ShowWarningMessage(string message, string title = "Предупреждение");

        /// <summary>
        /// Показывает диалог подтверждения.
        /// </summary>
        /// <param name="message">Текст вопроса подтверждения.</param>
        /// <param name="title">Заголовок окна сообщения. По умолчанию "Подтверждение".</param>
        /// <returns>True, если пользователь нажал "Да", иначе false.</returns>
        bool ShowConfirmationDialog(string message, string title = "Подтверждение");
    }
}