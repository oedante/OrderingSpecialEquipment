using System;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// Команда для привязки к событиям в XAML с параметром
    /// Реализует ICommand для выполнения действий с параметром
    /// </summary>
    /// <typeparam name="T">Тип параметра команды</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        /// <summary>
        /// Конструктор команды с параметром
        /// </summary>
        /// <param name="execute">Действие для выполнения с параметром</param>
        /// <param name="canExecute">Функция проверки возможности выполнения с параметром (опционально)</param>
        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Событие изменения возможности выполнения команды
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Проверяет, может ли команда быть выполнена
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || (parameter is T t && _canExecute(t));
        }

        /// <summary>
        /// Выполняет команду с параметром
        /// </summary>
        public void Execute(object? parameter)
        {
            if (parameter is T t)
            {
                _execute(t);
            }
        }

        /// <summary>
        /// Принудительно обновляет состояние команды
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}