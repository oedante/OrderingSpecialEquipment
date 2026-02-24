using OrderingSpecialEquipment.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace OrderingSpecialEquipment.Controls
{
    /// <summary>
    /// Логика взаимодействия для SearchableComboBox.xaml
    /// Комбобокс с возможностью поиска по вводу текста
    /// </summary>
    public partial class SearchableComboBox : UserControl, INotifyPropertyChanged
    {
        #region События

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Поля

        private string _searchText = string.Empty;
        private IEnumerable _filteredItems = new List<object>();
        private readonly CollectionViewSource _viewSource = new CollectionViewSource();
        private bool _isUpdatingText = false;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор SearchableComboBox
        /// </summary>
        public SearchableComboBox()
        {
            InitializeComponent();

            // Настройка CollectionViewSource для фильтрации
            _viewSource.Filter += OnFilterItem;

            // Подписка на события после загрузки
            Loaded += OnLoaded;
        }

        // Placeholder dependency property
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register("PlaceholderText", typeof(string), typeof(SearchableComboBox), new PropertyMetadata(string.Empty));

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        #endregion

        #region Свойства зависимостей

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SearchableComboBox),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(SearchableComboBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(SearchableComboBox),
                new PropertyMetadata("", OnDisplayMemberPathChanged));

        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(SearchableComboBox),
                new PropertyMetadata("Id"));

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object), typeof(SearchableComboBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty FilterByOrganizationProperty =
            DependencyProperty.Register("FilterByOrganization", typeof(string), typeof(SearchableComboBox),
                new PropertyMetadata(null, OnFilterChanged));

        public static readonly DependencyProperty FilterByEquipmentProperty =
            DependencyProperty.Register("FilterByEquipment", typeof(string), typeof(SearchableComboBox),
                new PropertyMetadata(null, OnFilterChanged));

        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(SearchableComboBox),
                new PropertyMetadata(false));

        #endregion

        #region Свойства

        /// <summary>
        /// Источник данных
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Выбранный элемент
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Путь к отображаемому свойству
        /// </summary>
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>
        /// Путь к значению
        /// </summary>
        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        /// <summary>
        /// Выбранное значение
        /// </summary>
        public object SelectedValue
        {
            get { return (object)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        /// <summary>
        /// Фильтр по организации
        /// </summary>
        public string FilterByOrganization
        {
            get { return (string)GetValue(FilterByOrganizationProperty); }
            set { SetValue(FilterByOrganizationProperty, value); }
        }

        /// <summary>
        /// Фильтр по технике
        /// </summary>
        public string FilterByEquipment
        {
            get { return (string)GetValue(FilterByEquipmentProperty); }
            set { SetValue(FilterByEquipmentProperty, value); }
        }

        /// <summary>
        /// Открыт ли выпадающий список
        /// </summary>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        /// <summary>
        /// Текст поиска
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    UpdateFilter();
                }
            }
        }

        /// <summary>
        /// Отфильтрованные элементы
        /// </summary>
        public IEnumerable FilteredItems
        {
            get => _filteredItems;
            set
            {
                _filteredItems = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasItems));
            }
        }

        /// <summary>
        /// Есть ли элементы после фильтрации
        /// </summary>
        public bool HasItems => FilteredItems?.Cast<object>().Any() ?? false;

        #endregion

        #region Обработчики изменений свойств зависимостей

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;
            control.UpdateItemsSource();
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;
            var item = e.NewValue;

            // Обновляем SelectedValue
            if (item != null && !string.IsNullOrEmpty(control.SelectedValuePath))
            {
                var prop = item.GetType().GetProperty(control.SelectedValuePath);
                if (prop != null)
                {
                    control.SelectedValue = prop.GetValue(item);
                }
            }

            // Обновляем текст в ComboBox
            if (item != null)
            {
                control._isUpdatingText = true;
                control.MainComboBox.Text = control.GetDisplayText(item);
                control._isUpdatingText = false;
            }
        }

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;
            control.UpdateItemsSource();
        }

        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;
            control.UpdateItemsSource();
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Загрузка контрола
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Получаем текстовое поле внутри ComboBox
            if (MainComboBox.Template.FindName("PART_EditableTextBox", MainComboBox) is TextBox textBox)
            {
                textBox.TextChanged += OnTextChanged;
            }

            MainComboBox.SelectionChanged += OnSelectionChanged;
            MainComboBox.DropDownOpened += (s, e) => IsDropDownOpen = true;
            MainComboBox.DropDownClosed += (s, e) => IsDropDownOpen = false;
            MainComboBox.PreviewKeyDown += OnPreviewKeyDown;
            MainComboBox.LostFocus += OnLostFocus;

            UpdateItemsSource();
        }

        /// <summary>
        /// Изменение текста
        /// </summary>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUpdatingText)
            {
                SearchText = ((TextBox)sender).Text;

                // Автоматически открываем выпадающий список при вводе текста
                if (!string.IsNullOrEmpty(SearchText) && !IsDropDownOpen && HasItems)
                {
                    MainComboBox.IsDropDownOpen = true;
                }
            }
        }

        /// <summary>
        /// Изменение выбора
        /// </summary>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SelectedItem = e.AddedItems[0];
            }
        }

        /// <summary>
        /// Обработка нажатий клавиш
        /// </summary>
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && HasItems)
            {
                // При нажатии Enter выбираем первый элемент
                var firstItem = FilteredItems.Cast<object>().FirstOrDefault();
                if (firstItem != null)
                {
                    SelectedItem = firstItem;
                    MainComboBox.Text = GetDisplayText(firstItem);
                    MainComboBox.IsDropDownOpen = false;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // При Escape закрываем список
                MainComboBox.IsDropDownOpen = false;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Потеря фокуса
        /// </summary>
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null)
            {
                _isUpdatingText = true;
                MainComboBox.Text = GetDisplayText(SelectedItem);
                _isUpdatingText = false;
            }
            else if (!string.IsNullOrEmpty(SearchText))
            {
                // Если ничего не выбрано, очищаем текст
                SearchText = string.Empty;
                MainComboBox.Text = string.Empty;
            }
        }

        /// <summary>
        /// Фильтрация элементов
        /// </summary>
        private void OnFilterItem(object sender, FilterEventArgs e)
        {
            if (e.Item == null)
            {
                e.Accepted = false;
                return;
            }

            // Фильтр по тексту поиска
            if (!string.IsNullOrEmpty(SearchText))
            {
                var displayText = GetDisplayText(e.Item);
                if (!displayText.ToLower().Contains(SearchText.ToLower()))
                {
                    e.Accepted = false;
                    return;
                }
            }

            e.Accepted = true;
        }

        #endregion

        #region Методы

        /// <summary>
        /// Обновление источника данных
        /// </summary>
        private void UpdateItemsSource()
        {
            if (ItemsSource == null)
            {
                _viewSource.Source = null;
                FilteredItems = new List<object>();
                return;
            }

            // Применяем фильтры по организации и технике
            var filteredByProps = ItemsSource.Cast<object>();

            if (!string.IsNullOrEmpty(FilterByOrganization))
            {
                filteredByProps = filteredByProps.Where(i =>
                {
                    var prop = i.GetType().GetProperty("LessorOrganizationId");
                    return prop != null && prop.GetValue(i)?.ToString() == FilterByOrganization;
                });
            }

            if (!string.IsNullOrEmpty(FilterByEquipment))
            {
                filteredByProps = filteredByProps.Where(i =>
                {
                    var prop = i.GetType().GetProperty("EquipmentId");
                    return prop != null && prop.GetValue(i)?.ToString() == FilterByEquipment;
                });
            }

            _viewSource.Source = filteredByProps.ToList();
            UpdateFilter();
        }

        /// <summary>
        /// Обновление фильтрации
        /// </summary>
        private void UpdateFilter()
        {
            _viewSource.View.Refresh();
            FilteredItems = _viewSource.View.Cast<object>().ToList();
        }

        /// <summary>
        /// Получение отображаемого текста для элемента
        /// </summary>
        private string GetDisplayText(object item)
        {
            if (item == null) return string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(DisplayMemberPath))
                {
                    var prop = item.GetType().GetProperty(DisplayMemberPath);
                    if (prop != null)
                    {
                        return prop.GetValue(item)?.ToString() ?? string.Empty;
                    }
                }

                // Специальная обработка для LicensePlate
                if (item is LicensePlate plate)
                {
                    if (!string.IsNullOrEmpty(plate.Brand))
                        return $"{plate.PlateNumber} - {plate.Brand}";
                    return plate.PlateNumber;
                }

                return item.ToString() ?? string.Empty;
            }
            catch
            {
                return item.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Вызов PropertyChanged
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}