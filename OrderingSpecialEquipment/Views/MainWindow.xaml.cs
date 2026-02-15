using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Поля

        private readonly MainWindowViewModel _viewModel;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор главного окна
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Получаем ViewModel из DI контейнера
            _viewModel = App.Services.GetRequiredService<MainWindowViewModel>();
            DataContext = _viewModel;

            // Загружаем дополнительные данные для выпадающих списков
            Loaded += async (s, e) =>
            {
                await _viewModel.InitializeAsync();

                // Проверяем статус подключения
                var dbService = App.Services.GetRequiredService<IDatabaseService>();
                if (!dbService.IsConnected)
                {
                    // Если нет подключения, показываем уведомление в статусбаре
                    System.Diagnostics.Debug.WriteLine("Нет подключения к БД при загрузке главного окна");
                }

                await LoadComboBoxDataAsync();
            };
        }

        #endregion

        #region Обработчики событий меню

        /// <summary>
        /// Выход из приложения
        /// </summary>
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// О программе
        /// </summary>
        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Система управления заявками на специальную технику\nВерсия 2.0\n\nРазработано в учебных целях",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #endregion

        #region Обработчики событий левой панели

        /// <summary>
        /// Клик по звездочке избранного
        /// </summary>
        private void FavoriteStar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is EquipmentItemViewModel equipment)
            {
                if (_viewModel.ToggleFavoriteCommand.CanExecute(equipment))
                {
                    _viewModel.ToggleFavoriteCommand.Execute(equipment);
                }
                e.Handled = true;
            }
        }

        #endregion

        #region Обработчики событий таблицы заявок

        /// <summary>
        /// Двойной клик по заявке
        /// </summary>
        private void RequestBorder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is ShiftRequestViewModel request)
            {
                if (_viewModel.EditRequestCommand.CanExecute(request))
                {
                    _viewModel.EditRequestCommand.Execute(request);
                }
                e.Handled = true;
            }
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Загрузка данных для выпадающих списков в popup
        /// </summary>
        private async System.Threading.Tasks.Task LoadComboBoxDataAsync()
        {
            try
            {
                var authService = App.Services.GetRequiredService<IAuthorizationService>();
                var dbService = App.Services.GetRequiredService<IDatabaseService>();

                if (dbService.IsConnected)
                {
                    System.Diagnostics.Debug.WriteLine("Загрузка данных для выпадающих списков...");

                    // Загружаем отделы
                    var departments = await authService.GetAccessibleDepartmentsAsync();
                    _viewModel.GetType().GetProperty("AccessibleDepartments")?.SetValue(_viewModel, departments);

                    // Загружаем склады
                    var warehouses = await authService.GetAccessibleWarehousesAsync();
                    _viewModel.GetType().GetProperty("AccessibleWarehouses")?.SetValue(_viewModel, warehouses);

                    // Загружаем технику
                    var equipments = await dbService.Context.Equipments.ToListAsync();
                    _viewModel.GetType().GetProperty("Equipments")?.SetValue(_viewModel, equipments);

                    // Загружаем госномера
                    var plates = await dbService.Context.LicensePlates.ToListAsync();
                    _viewModel.GetType().GetProperty("LicensePlates")?.SetValue(_viewModel, plates);

                    // Загружаем арендодателей
                    var lessors = await dbService.Context.LessorOrganizations.ToListAsync();
                    _viewModel.GetType().GetProperty("LessorOrganizations")?.SetValue(_viewModel, lessors);

                    System.Diagnostics.Debug.WriteLine("Данные для выпадающих списков загружены");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Пропуск загрузки данных для выпадающих списков - нет подключения к БД");
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не показываем пользователю
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных для combo: {ex.Message}");
            }
        }

        #endregion
    }
}