using OrderingSpecialEquipment.ViewModels;
using System.Windows;

namespace OrderingSpecialEquipment
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel mainViewModel) // DI через конструктор
        {
            InitializeComponent();
            DataContext = mainViewModel; // Устанавливаем ViewModel для окна
        }
    }
}