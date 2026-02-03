using System.Windows.Controls;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Логика взаимодействия для окна управления правами доступа пользователей к отделам и складам
    /// </summary>
    public partial class EditAccessRightsView : UserControl
    {
        public EditAccessRightsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик изменения выбора пользователя
        /// Вызывает команду для загрузки информации о доступах
        /// </summary>
        private void UserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModels.EditAccessRightsViewModel vm && vm.UserSelectionChangedCommand.CanExecute(null))
            {
                vm.UserSelectionChangedCommand.Execute(null);
            }
        }

        /// <summary>
        /// Обработчик изменения выбора отдела
        /// Вызывает команду для загрузки информации о доступах к складам
        /// </summary>
        private void DepartmentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModels.EditAccessRightsViewModel vm && vm.DepartmentSelectionChangedCommand.CanExecute(null))
            {
                vm.DepartmentSelectionChangedCommand.Execute(null);
            }
        }
    }
}