using System;
using System.Threading.Tasks;
using System.Windows;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Логика взаимодействия для сплеш-скрина
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Показывает сплеш-скрин на заданное время
        /// </summary>
        public async Task ShowForDuration(TimeSpan duration)
        {
            Show();
            await Task.Delay(duration);
            Close();
        }
    }
}