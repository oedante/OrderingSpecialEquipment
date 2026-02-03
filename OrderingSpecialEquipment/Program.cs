using System;
using System.Threading.Tasks;
using System.Windows;
using OrderingSpecialEquipment.Views;

namespace OrderingSpecialEquipment
{
    /// <summary>
    /// Точка входа в приложение
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Главный метод приложения
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // Создаем приложение WPF
            var app = new App();

            // Показываем сплеш-скрин
            ShowSplashScreen();

            // Запускаем приложение
            app.InitializeComponent();
            app.Run();
        }

        /// <summary>
        /// Показывает сплеш-скрин с логотипом при запуске
        /// </summary>
        private static void ShowSplashScreen()
        {
            var splash = new Views.SplashScreen
            {
                Owner = null,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = true
            };

            splash.Show();
            System.Threading.Thread.Sleep(2000); // Показываем 2 секунды
            splash.Close();
        }
    }
}