using OrderingSpecialEquipment.Models;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// ViewModel для элемента техники в левой панели
    /// </summary>
    public class EquipmentItemViewModel : ViewModelBase
    {
        private Equipment _equipment;
        private bool _isFavorite;
        private int _nightCount;
        private int _dayCount;
        private decimal _monthlyHoursLeft;

        public Equipment Equipment
        {
            get => _equipment;
            set => SetProperty(ref _equipment, value);
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set => SetProperty(ref _isFavorite, value);
        }

        public int NightCount
        {
            get => _nightCount;
            set => SetProperty(ref _nightCount, value);
        }

        public int DayCount
        {
            get => _dayCount;
            set => SetProperty(ref _dayCount, value);
        }

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

        public string DisplayName => Equipment?.Name ?? "";
        public string DisplayCounts => $"Н:{NightCount} Д:{DayCount}";
        public string HoursLeftDisplay => $"{MonthlyHoursLeft:F1} ч";
        public bool IsHoursLeftCritical => MonthlyHoursLeft <= 0;
        public bool IsHoursLeftWarning => MonthlyHoursLeft > 0 && MonthlyHoursLeft < 10;

        public string HoursLeftColor
        {
            get
            {
                if (MonthlyHoursLeft <= 0) return "#FFFFE0E0";
                if (MonthlyHoursLeft < 10) return "#FFFFF0E0";
                return "Transparent";
            }
        }
    }
}