using System;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// Команда для привязки к событиям в XAML
    /// Реализует ICommand для выполнения действий без параметров
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Конструктор команды
        /// </summary>
        /// <param name="execute">Действие для выполнения</param>
        /// <param name="canExecute">Функция проверки возможности выполнения (опционально)</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
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
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// Выполняет команду
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute();
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