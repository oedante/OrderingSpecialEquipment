using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.Utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OrderingSpecialEquipment.Views
{
    /// <summary>
    /// Логика взаимодействия для ConnectionSettingsWindow.xaml
    /// </summary>
    public partial class ConnectionSettingsWindow : Window
    {
        #region Поля

        private readonly IDatabaseService _databaseService;
        // Используем поле для отслеживания успешного теста
        private bool _testSuccessful = false;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор окна настроек подключения
        /// </summary>
        public ConnectionSettingsWindow()
        {
            InitializeComponent();

            // Получаем сервисы
            _databaseService = App.Services.GetRequiredService<IDatabaseService>();

            // Загружаем сохраненные настройки
            LoadSavedSettings();

            // Подписываемся на изменения
            txtServer.TextChanged += UpdateConnectionString;
            txtPort.TextChanged += UpdateConnectionString;
            txtDatabase.TextChanged += UpdateConnectionString;
            txtUsername.TextChanged += UpdateConnectionString;
            txtPassword.PasswordChanged += (s, e) => UpdateConnectionString(s, e);
            rbPostgreSQL.Checked += (s, e) => UpdateConnectionString(s, e);
            rbSqlServer.Checked += (s, e) => UpdateConnectionString(s, e);

            // Формируем начальную строку
            UpdateConnectionString(null, null);
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обновление строки подключения при изменении параметров
        /// </summary>
        private void UpdateConnectionString(object sender, RoutedEventArgs e)
        {
            try
            {
                string connectionString = BuildConnectionString();
                txtConnectionString.Text = connectionString;

                // Сбрасываем статус тестирования
                _testSuccessful = false;
                btnSave.IsEnabled = false;
                borderSuccess.Visibility = Visibility.Collapsed;
                borderError.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                txtConnectionString.Text = $"Ошибка формирования строки: {ex.Message}";
            }
        }

        /// <summary>
        /// Проверка подключения
        /// </summary>
        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnTest.IsEnabled = false;
                btnTest.Content = "Проверка...";

                string connectionString = BuildConnectionString();
                var dbType = rbPostgreSQL.IsChecked == true
                    ? DbConnectionFactory.DatabaseType.PostgreSQL
                    : DbConnectionFactory.DatabaseType.SqlServer;

                // Проверяем подключение
                bool success = await Task.Run(() =>
                    DbConnectionFactory.TestConnection(connectionString, dbType));

                if (success)
                {
                    borderSuccess.Visibility = Visibility.Visible;
                    borderError.Visibility = Visibility.Collapsed;
                    _testSuccessful = true;
                    btnSave.IsEnabled = true;
                }
                else
                {
                    borderSuccess.Visibility = Visibility.Collapsed;
                    borderError.Visibility = Visibility.Visible;
                    txtError.Text = "Не удалось подключиться к базе данных. Проверьте параметры.";
                    _testSuccessful = false;
                    btnSave.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                borderSuccess.Visibility = Visibility.Collapsed;
                borderError.Visibility = Visibility.Visible;
                txtError.Text = $"Ошибка: {ex.Message}";
                _testSuccessful = false;
                btnSave.IsEnabled = false;
            }
            finally
            {
                btnTest.IsEnabled = true;
                btnTest.Content = "Проверить подключение";
            }
        }

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string connectionString = BuildConnectionString();
                var dbType = rbPostgreSQL.IsChecked == true
                    ? DbConnectionFactory.DatabaseType.PostgreSQL
                    : DbConnectionFactory.DatabaseType.SqlServer;

                // Сохраняем строку подключения
                ConnectionStringHelper.SaveConnectionString(connectionString);

                // Переинициализируем сервис БД
                await _databaseService.InitializeAsync(connectionString);

                MessageBox.Show(
                    "Настройки подключения успешно сохранены.",
                    "Успешно",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при сохранении настроек: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отмена
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Загрузка сохраненных настроек
        /// </summary>
        private void LoadSavedSettings()
        {
            try
            {
                string savedConnection = ConnectionStringHelper.LoadConnectionString();
                if (!string.IsNullOrEmpty(savedConnection))
                {
                    // Пытаемся распарсить сохраненную строку
                    if (savedConnection.Contains("Host="))
                    {
                        rbPostgreSQL.IsChecked = true;

                        // Простой парсинг строки PostgreSQL
                        var parts = savedConnection.Split(';');
                        foreach (var part in parts)
                        {
                            if (part.StartsWith("Host="))
                                txtServer.Text = part.Substring(5);
                            else if (part.StartsWith("Port="))
                                txtPort.Text = part.Substring(5);
                            else if (part.StartsWith("Database="))
                                txtDatabase.Text = part.Substring(9);
                            else if (part.StartsWith("Username="))
                                txtUsername.Text = part.Substring(9);
                            else if (part.StartsWith("Password="))
                                txtPassword.Password = part.Substring(9);
                        }
                    }
                    else if (savedConnection.Contains("Server="))
                    {
                        rbSqlServer.IsChecked = true;

                        // Простой парсинг строки SQL Server
                        var parts = savedConnection.Split(';');
                        foreach (var part in parts)
                        {
                            if (part.StartsWith("Server="))
                                txtServer.Text = part.Substring(7);
                            else if (part.StartsWith("Database="))
                                txtDatabase.Text = part.Substring(9);
                            else if (part.StartsWith("User ID="))
                                txtUsername.Text = part.Substring(8);
                            else if (part.StartsWith("Password="))
                                txtPassword.Password = part.Substring(9);
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки
            }
        }

        /// <summary>
        /// Формирование строки подключения
        /// </summary>
        private string BuildConnectionString()
        {
            if (rbPostgreSQL.IsChecked == true)
            {
                return $"Host={txtServer.Text};" +
                       $"Port={txtPort.Text};" +
                       $"Database={txtDatabase.Text};" +
                       $"Username={txtUsername.Text};" +
                       $"Password={txtPassword.Password}";
            }
            else
            {
                return $"Server={txtServer.Text};" +
                       $"Database={txtDatabase.Text};" +
                       $"User ID={txtUsername.Text};" +
                       $"Password={txtPassword.Password};" +
                       $"TrustServerCertificate=True;";
            }
        }

        #endregion
    }
}