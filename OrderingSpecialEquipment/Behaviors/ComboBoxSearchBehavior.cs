using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace OrderingSpecialEquipment.Behaviors
{
    /// <summary>
    /// Поведение для добавления живого поиска в ComboBox
    /// Автоматически фильтрует элементы по мере ввода текста
    /// </summary>
    public class ComboBoxSearchBehavior : Behavior<ComboBox>
    {
        private CancellationTokenSource _searchCancellationToken;
        private string _originalText;
        private bool _isUpdating;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.IsEditable = true;
            AssociatedObject.IsTextSearchEnabled = false;
            AssociatedObject.StaysOpenOnEdit = true;

            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            AssociatedObject.TextChanged += OnTextChanged;
            AssociatedObject.DropDownClosed += OnDropDownClosed;
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
            AssociatedObject.TextChanged -= OnTextChanged;
            AssociatedObject.DropDownClosed -= OnDropDownClosed;
            AssociatedObject.SelectionChanged -= OnSelectionChanged;

            _searchCancellationToken?.Cancel();
            base.OnDetaching();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || AssociatedObject.SelectedItem == null)
                return;

            // Сохраняем оригинальный текст для восстановления при закрытии
            if (AssociatedObject.SelectedItem is IComboBoxItem item)
            {
                _originalText = item.DisplayMember;
                AssociatedObject.Text = _originalText;
            }
        }

        private void OnDropDownClosed(object sender, EventArgs e)
        {
            // Восстанавливаем оригинальный текст при закрытии выпадающего списка
            if (!string.IsNullOrEmpty(_originalText) && !_isUpdating)
            {
                AssociatedObject.Text = _originalText;
            }
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Разрешаем навигацию клавишами
            if (e.Key == System.Windows.Input.Key.Down ||
                e.Key == System.Windows.Input.Key.Up ||
                e.Key == System.Windows.Input.Key.Enter)
            {
                return;
            }

            // Открываем выпадающий список при вводе текста
            if (!AssociatedObject.IsDropDownOpen &&
                e.Key != System.Windows.Input.Key.Escape &&
                e.Key != System.Windows.Input.Key.Tab)
            {
                AssociatedObject.IsDropDownOpen = true;
            }
        }

        private async void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating || string.IsNullOrEmpty(AssociatedObject.Text))
                return;

            // Отменяем предыдущий запрос поиска
            _searchCancellationToken?.Cancel();
            _searchCancellationToken = new CancellationTokenSource();

            try
            {
                // Задержка 300мс для уменьшения количества запросов
                await Task.Delay(300, _searchCancellationToken.Token);

                // Фильтруем элементы
                await FilterItemsAsync(_searchCancellationToken.Token);
            }
            catch (TaskCanceledException)
            {
                // Игнорируем отмененные задачи
            }
        }

        private async Task FilterItemsAsync(CancellationToken cancellationToken)
        {
            if (AssociatedObject.ItemsSource == null)
                return;

            _isUpdating = true;

            try
            {
                string searchText = AssociatedObject.Text?.ToLower().Trim() ?? "";

                // Получаем все элементы
                var items = AssociatedObject.ItemsSource.Cast<object>().ToList();

                // Фильтруем асинхронно (для больших коллекций)
                var filteredItems = await Task.Run(() =>
                {
                    if (string.IsNullOrEmpty(searchText))
                        return items;

                    return items.Where(item =>
                    {
                        if (item is IComboBoxItem comboBoxItem)
                        {
                            return comboBoxItem.DisplayMember?.ToLower().Contains(searchText) == true;
                        }
                        return item.ToString()?.ToLower().Contains(searchText) == true;
                    }).ToList();
                }, cancellationToken);

                // Обновляем источник данных
                if (AssociatedObject.ItemsSource is ObservableCollection<object> observableCollection)
                {
                    observableCollection.Clear();
                    foreach (var item in filteredItems)
                    {
                        observableCollection.Add(item);
                    }
                }
                else
                {
                    // Создаем новую коллекцию
                    var newCollection = new ObservableCollection<object>(filteredItems);
                    AssociatedObject.ItemsSource = newCollection;
                }

                // Открываем выпадающий список, если есть результаты
                AssociatedObject.IsDropDownOpen = filteredItems.Any();

                cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }

    /// <summary>
    /// Интерфейс для элементов ComboBox с поддержкой отображения
    /// </summary>
    public interface IComboBoxItem
    {
        string DisplayMember { get; }
        object ValueMember { get; }
    }
}