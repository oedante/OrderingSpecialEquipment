using System;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// Реализация команды для привязки в XAML
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Поля

        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор команды с параметром object?
        /// </summary>
        /// <param name="execute">Действие при выполнении</param>
        /// <param name="canExecute">Функция проверки возможности выполнения</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Конструктор команды без параметра
        /// </summary>
        /// <param name="execute">Действие при выполнении</param>
        /// <param name="canExecute">Функция проверки возможности выполнения</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(p => execute(), canExecute == null ? null : (p) => canExecute())
        {
        }

        #endregion

        #region События

        /// <summary>
        /// Событие изменения возможности выполнения команды
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion

        #region Методы ICommand

        /// <summary>
        /// Проверка возможности выполнения команды
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Принудительное обновление состояния команды
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion
    }

    /// <summary>
    /// Типизированная версия RelayCommand
    /// </summary>
    /// <typeparam name="T">Тип параметра</typeparam>
    public class RelayCommand<T> : ICommand
    {
        #region Поля

        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор команды
        /// </summary>
        /// <param name="execute">Действие при выполнении</param>
        /// <param name="canExecute">Функция проверки возможности выполнения</param>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        #endregion

        #region События

        /// <summary>
        /// Событие изменения возможности выполнения команды
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion

        #region Методы ICommand

        /// <summary>
        /// Проверка возможности выполнения команды
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null)
                return true;

            if (parameter == null && typeof(T).IsValueType)
                return _canExecute(default);

            if (parameter is T typedParameter)
                return _canExecute(typedParameter);

            return false;
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        public void Execute(object? parameter)
        {
            T? typedParameter = parameter is T t ? t : default;
            _execute(typedParameter);
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Принудительное обновление состояния команды
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion
    }
}