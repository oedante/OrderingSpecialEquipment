using System;
using System.Windows.Input;

namespace OrderingSpecialEquipment.Commands
{
    /// <summary>
    /// Реализация ICommand, которая позволяет привязывать методы в ViewModel к элементам управления UI.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Конструктор команды с возможностью указать условие выполнения.
        /// </summary>
        /// <param name="execute">Метод, вызываемый при выполнении команды.</param>
        /// <param name="canExecute">Метод, определяющий, можно ли выполнить команду. Необязательный параметр.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Возникает, когда меняется состояние, влияющее на выполнение команды.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Определяет, может ли команда выполниться в данный момент.
        /// </summary>
        /// <param name="parameter">Параметр команды.</param>
        /// <returns>True, если команда может быть выполнена, иначе false.</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Выполняет логику команды.
        /// </summary>
        /// <param name="parameter">Параметр команды.</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Вызывает событие CanExecuteChanged вручную.
        /// Полезно, когда условия, влияющие на CanExecute, изменяются извне.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}