using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.ViewModels;
using System;
using System.Windows;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Логика взаимодействия для UserSettingsView.xaml
    /// </summary>
    public partial class UserSettingsView : Window
    {
        private readonly UserSettingsViewModel _viewModel;

        public UserSettingsView()
        {
            InitializeComponent();

            _viewModel = App.Services.GetRequiredService<UserSettingsViewModel>();
            DataContext = _viewModel;

            Loaded += async (s, e) =>
            {
                try
                {
                    await _viewModel.LoadUsersAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }
    }
}