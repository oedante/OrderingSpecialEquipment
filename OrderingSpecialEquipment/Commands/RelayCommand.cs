using System;
using System.Windows.Input;

namespace OrderingSpecialEquipment.Commands
{
    /// <summary>
    /// Необобщённая реализация ICommand для действий без параметра или с object-параметром.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Конструктор для команд без параметра
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = _ => execute?.Invoke(); // Игнорируем параметр
            _canExecute = _ => canExecute?.Invoke() ?? true; // Игнорируем параметр
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Вызывает событие CanExecuteChanged.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Обобщённая реализация ICommand, позволяющая передавать параметр строго типизированного типа T.
    /// </summary>
    /// <typeparam name="T">Тип параметра команды.</typeparam>
    public class RelayCommandOfT<T> : ICommand // Изменим имя на RelayCommandOfT
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommandOfT(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            if (parameter != null && !(parameter is T || (parameter is null && typeof(T).IsClass)))
            {
                return false;
            }
            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            if (parameter != null && !(parameter is T || (parameter is null && typeof(T).IsClass)))
            {
                throw new ArgumentException($"Неправильный тип параметра. Ожидался '{typeof(T)?.Name}', получен '{parameter.GetType()?.Name}'.");
            }
            _execute((T?)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}