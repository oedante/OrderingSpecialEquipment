using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
        private bool _isAnimating = false;

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

            // Подписываемся на изменение видимости панели для запуска анимации
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.IsLeftPanelVisible) && !_isAnimating)
                {
                    StartPanelAnimation(_viewModel.IsLeftPanelVisible);
                }
            };

            // Загружаем данные при загрузке окна
            Loaded += async (s, e) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("MainWindow: Загрузка окна");
                    await _viewModel.InitializeAsync();
                    System.Diagnostics.Debug.WriteLine("MainWindow: Загрузка завершена");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке окна: {ex.Message}");
                }
            };
        }

        #endregion

        #region Анимация панели

        /// <summary>
        /// Запуск анимации панели
        /// </summary>
        private void StartPanelAnimation(bool show)
        {
            try
            {
                _isAnimating = true;

                // Создаем анимацию для ширины колонки
                var animation = new GridLengthAnimation
                {
                    From = new GridLength(LeftPanelColumn.Width.Value),
                    To = show ? new GridLength(250) : new GridLength(0),
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                // Подписываемся на завершение анимации
                animation.Completed += (s, e) =>
                {
                    _isAnimating = false;
                    _viewModel.UpdateDisplayedShiftsAsync();

                    // Устанавливаем финальное значение в ViewModel
                    _viewModel.LeftPanelWidth = show ? 250 : 0;
                };

                // Запускаем анимацию
                LeftPanelColumn.BeginAnimation(ColumnDefinitionWidthAnimation.WidthProperty, animation);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при анимации панели: {ex.Message}");
                // Если анимация не сработала, просто устанавливаем ширину
                LeftPanelColumn.Width = show ? new GridLength(250) : new GridLength(0);
                _viewModel.LeftPanelWidth = show ? 250 : 0;
                _viewModel.UpdateDisplayedShiftsAsync();
                _isAnimating = false;
            }
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
            try
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при переключении избранного: {ex.Message}");
            }
        }

        #endregion

        #region Обработчики событий таблицы заявок

        /// <summary>
        /// Двойной клик по заявке
        /// </summary>
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var dataGrid = (DataGrid)sender;
                if (dataGrid.SelectedItem is ShiftRequestViewModel request)
                {
                    System.Diagnostics.Debug.WriteLine($"Двойной клик по заявке: {request.Key}");
                    if (_viewModel.EditRequestCommand.CanExecute(request))
                    {
                        _viewModel.EditRequestCommand.Execute(request);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка двойного клика: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Класс для анимации GridLength
    /// </summary>
    public class GridLengthAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(GridLength);

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public GridLength From
        {
            get { return (GridLength)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

        public GridLength To
        {
            get { return (GridLength)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));

        public IEasingFunction EasingFunction
        {
            get { return (IEasingFunction)GetValue(EasingFunctionProperty); }
            set { SetValue(EasingFunctionProperty, value); }
        }

        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(GridLengthAnimation));

        public event EventHandler Completed;

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null)
                return From;

            double progress = animationClock.CurrentProgress.Value;

            if (EasingFunction != null)
                progress = EasingFunction.Ease(progress);

            if (animationClock.CurrentState == ClockState.Filling)
            {
                if (animationClock.CurrentProgress == 1.0)
                {
                    Completed?.Invoke(this, EventArgs.Empty);
                }
            }

            double fromValue = From.Value;
            double toValue = To.Value;
            double newValue = fromValue + (toValue - fromValue) * progress;

            return new GridLength(newValue, From.IsStar ? GridUnitType.Star : GridUnitType.Pixel);
        }
    }

    /// <summary>
    /// Класс для анимации Width свойства ColumnDefinition
    /// </summary>
    public static class ColumnDefinitionWidthAnimation
    {
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.RegisterAttached(
                "Width",
                typeof(double),
                typeof(ColumnDefinitionWidthAnimation),
                new PropertyMetadata(OnWidthChanged));

        public static void SetWidth(ColumnDefinition element, double value)
        {
            element.SetValue(WidthProperty, value);
        }

        public static double GetWidth(ColumnDefinition element)
        {
            return (double)element.GetValue(WidthProperty);
        }

        private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColumnDefinition column)
            {
                column.Width = new GridLength((double)e.NewValue);
            }
        }
    }
}