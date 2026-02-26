using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using OrderingSpecialEquipment.ViewModels;

namespace OrderingSpecialEquipment.Views
{
    public partial class EditRequestWindow : Window
    {
        private static EditRequestWindow _openInstance;
        private MainWindowViewModel _viewModel;
        private bool _isClosing = false;

        public EditRequestWindow()
        {
            InitializeComponent();
            this.DataContextChanged += EditRequestWindow_DataContextChanged;

            // Убираем проверку из конструктора - переносим в Loaded
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
            if (e.PropertyName == nameof(MainWindowViewModel.IsPopupOpen))
            {
                if (sender is MainWindowViewModel vm && !vm.IsPopupOpen && !_isClosing)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        if (!_isClosing && this.IsLoaded)
                        {
                            this.Close();
                        }
                    });
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, не открыто ли уже другое окно
                if (_openInstance != null && _openInstance != this && _openInstance.IsLoaded)
                {
                    _openInstance.Activate();
                    _isClosing = true;
                    this.Close();
                    return;
                }

                this.Opacity = 0;
                var anim = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new System.TimeSpan(0, 0, 0, 0, 220));
                this.BeginAnimation(Window.OpacityProperty, anim);

                // Копируем отдел из главного окна, если это новая заявка
                if (_viewModel?.EditingRequest?.IsNew == true && _viewModel.SelectedDepartment != null)
                {
                    _viewModel.EditingRequest.Department = _viewModel.SelectedDepartment;
                }

                // Устанавливаем фокус на первый элемент
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (FirstDatePicker != null && FirstDatePicker.IsLoaded)
                    {
                        FirstDatePicker.Focus();
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в Window_Loaded: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _isClosing = true;

            if (_viewModel != null && _viewModel.IsPopupOpen)
            {
                _viewModel.IsPopupOpen = false;
            }

            if (_openInstance == this)
            {
                _openInstance = null;
            }
        }

        // Обработчик изменения смены
        private void ComboBox_ShiftChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.EditingRequest != null)
            {
                _viewModel.UpdateTimeBasedOnShift();
            }
        }

        // Обработчик изменения склада
        private void ComboBox_WarehouseChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.EditingRequest != null)
            {
                // Автоматически выбираем первую доступную территорию
                if (_viewModel.EditingRequest.AvailableAreas != null &&
                    _viewModel.EditingRequest.AvailableAreas.Any())
                {
                    _viewModel.EditingRequest.Area = _viewModel.EditingRequest.AvailableAreas.First();
                }
            }
        }

        // Обработчик изменения техники
        private void ComboBox_EquipmentChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.EditingRequest != null)
            {
                // Сбрасываем количество до 1
                _viewModel.EditingRequest.RequestedCount = 1;

                // Очищаем арендодателя и госномер
                _viewModel.EditingRequest.LessorOrganization = null;
                _viewModel.EditingRequest.LicensePlate = null;

                // Обновляем фильтрацию номеров
                _viewModel.NotifyPropertyChanged(nameof(MainWindowViewModel.FilteredLicensePlates));
            }
        }

        // Обработчик изменения арендодателя
        private void ComboBox_LessorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.EditingRequest != null)
            {
                // Очищаем госномер при смене арендодателя
                _viewModel.EditingRequest.LicensePlate = null;

                // Обновляем фильтрацию номеров
                _viewModel.NotifyPropertyChanged(nameof(MainWindowViewModel.FilteredLicensePlates));
            }
        }
    }
}