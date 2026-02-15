using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// Базовый класс для всех ViewModel с реализацией INotifyPropertyChanged
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region События

        /// <summary>
        /// Событие изменения свойства
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Защищенные методы

        /// <summary>
        /// Вызов события изменения свойства
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Установка значения поля с вызовом события
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="field">Ссылка на поле</param>
        /// <param name="value">Новое значение</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns>True, если значение изменилось</returns>
        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Обработка ошибок при выполнении действия
        /// </summary>
        /// <param name="action">Действие</param>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        protected virtual void HandleError(Action action, string errorMessage = "Произошла ошибка")
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{errorMessage}: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Асинхронная обработка ошибок при выполнении действия
        /// </summary>
        /// <param name="func">Асинхронное действие</param>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        protected virtual async Task HandleErrorAsync(Func<Task> func,
            string errorMessage = "Произошла ошибка")
        {
            try
            {
                await func();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{errorMessage}: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion
    }
}