using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// Базовый класс для ViewModel, реализующий INotifyPropertyChanged и INotifyDataErrorInfo.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        // Используем ConcurrentDictionary для потокобезопасности
        private readonly ConcurrentDictionary<string, List<string>> _errors = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>
        /// Вызывает событие PropertyChanged для указанного свойства.
        /// </summary>
        /// <param name="propertyName">Имя свойства. Передается автоматически компилятором.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Устанавливает значение поля и вызывает PropertyChanged, если значение изменилось.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="field">Поле, хранящее значение.</param>
        /// <param name="value">Новое значение.</param>
        /// <param name="propertyName">Имя свойства. Передается автоматически компилятором.</param>
        /// <returns>True, если значение изменилось.</returns>
        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // --- Реализация INotifyDataErrorInfo ---
        public bool HasErrors => !_errors.IsEmpty;

        public IEnumerable GetErrors(string? propertyName)
        {
            return _errors.TryGetValue(propertyName ?? string.Empty, out var propertyErrors)
                ? propertyErrors
                : Enumerable.Empty<string>();
        }

        /// <summary>
        /// Устанавливает ошибки для указанного свойства.
        /// </summary>
        /// <param name="propertyName">Имя свойства. Если null или пустая строка, ошибка считается общей.</param>
        /// <param name="errors">Список сообщений об ошибках.</param>
        protected virtual void SetErrors(string? propertyName, IEnumerable<string> errors)
        {
            var errorList = errors.ToList();
            if (errorList.Count > 0)
            {
                _errors[propertyName ?? string.Empty] = errorList;
            }
            else
            {
                _errors.TryRemove(propertyName ?? string.Empty, out _);
            }
            OnErrorsChanged(propertyName);
        }

        /// <summary>
        /// Удаляет ошибки для указанного свойства.
        /// </summary>
        /// <param name="propertyName">Имя свойства.</param>
        protected virtual void ClearErrors(string? propertyName = null)
        {
            _errors.TryRemove(propertyName ?? string.Empty, out _);
            OnErrorsChanged(propertyName);
        }

        /// <summary>
        /// Вызывает событие ErrorsChanged для указанного свойства.
        /// </summary>
        /// <param name="propertyName">Имя свойства.</param>
        protected virtual void OnErrorsChanged(string? propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}