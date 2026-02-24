using OrderingSpecialEquipment.Models;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// ViewModel для элемента техники в левой панели
    /// </summary>
    public class EquipmentItemViewModel : ViewModelBase
    {
        #region Поля

        private Equipment _equipment;
        private bool _isFavorite;
        private int _nightCount;
        private int _dayCount;
        private decimal _monthlyHoursLeft;

        #endregion

        #region Свойства

        /// <summary>
        /// Модель техники
        /// </summary>
        public Equipment Equipment
        {
            get => _equipment;
            set => SetProperty(ref _equipment, value);
        }

        /// <summary>
        /// Находится ли в избранном
        /// </summary>
        public bool IsFavorite
        {
            get => _isFavorite;
            set => SetProperty(ref _isFavorite, value);
        }

        /// <summary>
        /// Количество заявок на ночную смену
        /// </summary>
        public int NightCount
        {
            get => _nightCount;
            set => SetProperty(ref _nightCount, value);
        }

        /// <summary>
        /// Количество заявок на дневную смену
        /// </summary>
        public int DayCount
        {
            get => _dayCount;
            set => SetProperty(ref _dayCount, value);
        }

        /// <summary>
        /// Оставшиеся часы по транспортной программе на текущий месяц
        /// </summary>
        public decimal MonthlyHoursLeft
        {
            get => _monthlyHoursLeft;
            set
            {
                if (SetProperty(ref _monthlyHoursLeft, value))
                {
                    OnPropertyChanged(nameof(HoursLeftDisplay));
                    OnPropertyChanged(nameof(IsHoursLeftCritical));
                    OnPropertyChanged(nameof(IsHoursLeftWarning));
                    OnPropertyChanged(nameof(HoursLeftColor));
                }
            }
        }

        /// <summary>
        /// Отображаемое название техники
        /// </summary>
        public string DisplayName => Equipment?.Name ?? "";

        /// <summary>
        /// Отображение количества заявок по сменам
        /// </summary>
        public string DisplayCounts => $"Н:{NightCount} Д:{DayCount}";

        /// <summary>
        /// Отображение оставшихся часов
        /// </summary>
        public string HoursLeftDisplay => $"{MonthlyHoursLeft:F1} ч";

        /// <summary>
        /// Критический остаток часов (<= 0)
        /// </summary>
        public bool IsHoursLeftCritical => MonthlyHoursLeft <= 0;

        /// <summary>
        /// Предупреждение об остатке часов (< 10)
        /// </summary>
        public bool IsHoursLeftWarning => MonthlyHoursLeft > 0 && MonthlyHoursLeft < 10;

        /// <summary>
        /// Цвет фона в зависимости от остатка часов
        /// </summary>
        public string HoursLeftColor
        {
            get
            {
                if (MonthlyHoursLeft <= 0) return "#FFFFE0E0"; // Красноватый
                if (MonthlyHoursLeft < 10) return "#FFFFF0E0"; // Желтоватый
                return "Transparent";
            }
        }

        #endregion
    }
}