using OrderingSpecialEquipment.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OrderingSpecialEquipment
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel mainViewModel) // DI через конструктор
        {
            DataContext = mainViewModel; // Устанавливаем ViewModel до InitializeComponent
            InitializeComponent();
        }

        // Обработчик двойного клика по элементу доступной техники (для добавления заявки)
        private void AvailableEquipment_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                if (sender is ListBox listBox)
                {
                    var selectedEquipment = listBox.SelectedItem as Models.Equipment; // Обновите пространство имён, если нужно
                    if (selectedEquipment != null)
                    {
                        // Вызываем команду в MainViewModel для добавления заявки на выбранную технику
                        vm.AddRequestFromEquipmentCommand?.Execute(selectedEquipment.Id);
                    }
                }
            }
        }
    }
}