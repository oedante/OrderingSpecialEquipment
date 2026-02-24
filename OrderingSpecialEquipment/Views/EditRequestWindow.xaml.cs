using System.ComponentModel;
using System.Windows;
using OrderingSpecialEquipment.ViewModels;

namespace OrderingSpecialEquipment.Views
{
    public partial class EditRequestWindow : Window
    {
        private static EditRequestWindow _openInstance;
        private MainWindowViewModel _viewModel;

        public EditRequestWindow()
        {
            InitializeComponent();
            this.DataContextChanged += EditRequestWindow_DataContextChanged;

            // Проверяем, нет ли уже открытого окна
            if (_openInstance != null)
            {
                _openInstance.Activate();
                this.Close();
                return;
            }

            _openInstance = this;
        }

        private void EditRequestWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MainWindowViewModel oldVm)
            {
                oldVm.PropertyChanged -= Vm_PropertyChanged;
            }
            if (e.NewValue is MainWindowViewModel newVm)
            {
                _viewModel = newVm;
                newVm.PropertyChanged += Vm_PropertyChanged;
            }
        }

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Закрываем окно, когда ViewModel сбросит флаг IsPopupOpen
            if (e.PropertyName == nameof(MainWindowViewModel.IsPopupOpen))
            {
                if (sender is MainWindowViewModel vm && !vm.IsPopupOpen)
                {
                    Dispatcher.Invoke(() => this.Close());
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Opacity = 0;
                var anim = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new System.TimeSpan(0, 0, 0, 0, 220));
                this.BeginAnimation(Window.OpacityProperty, anim);

                // Устанавливаем фокус на первый элемент формы
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    var dp = this.FindName("FirstDatePicker") as System.Windows.Controls.DatePicker;
                    dp?.Focus();
                }));
            }
            catch
            {
                // Игнорируем ошибки анимации
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Сбрасываем флаг в ViewModel, если окно закрывается без сохранения
            if (_viewModel != null && _viewModel.IsPopupOpen)
            {
                _viewModel.IsPopupOpen = false;
            }

            // Освобождаем статическую ссылку
            if (_openInstance == this)
            {
                _openInstance = null;
            }
        }
    }
}