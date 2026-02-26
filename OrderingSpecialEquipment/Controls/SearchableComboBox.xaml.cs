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
        private string _lastFilterByOrganization = string.Empty;
        private string _lastFilterByEquipment = string.Empty;

        #endregion

        #region Конструктор

        public SearchableComboBox()
        {
            InitializeComponent();

            _viewSource.Filter += OnFilterItem;

            Loaded += OnLoaded;
        }

        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register("PlaceholderText", typeof(string), typeof(SearchableComboBox), new PropertyMetadata("Поиск..."));

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

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        public object SelectedValue
        {
            get { return (object)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public string FilterByOrganization
        {
            get { return (string)GetValue(FilterByOrganizationProperty); }
            set { SetValue(FilterByOrganizationProperty, value); }
        }

        public string FilterByEquipment
        {
            get { return (string)GetValue(FilterByEquipmentProperty); }
            set { SetValue(FilterByEquipmentProperty, value); }
        }

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

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

            if (item != null && !string.IsNullOrEmpty(control.SelectedValuePath))
            {
                var prop = item.GetType().GetProperty(control.SelectedValuePath);
                if (prop != null)
                {
                    control.SelectedValue = prop.GetValue(item);
                }
            }

            if (item != null)
            {
                control._isUpdatingText = true;
                control.MainComboBox.Text = control.GetDisplayText(item);
                control._isUpdatingText = false;
            }
            else
            {
                control._isUpdatingText = true;
                control.MainComboBox.Text = string.Empty;
                control._isUpdatingText = false;
            }
        }

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;

            // Проверяем, действительно ли изменился фильтр
            if (e.Property == FilterByOrganizationProperty &&
                control._lastFilterByOrganization != (string)e.NewValue)
            {
                control._lastFilterByOrganization = (string)e.NewValue;
                control.UpdateItemsSource();
            }
            else if (e.Property == FilterByEquipmentProperty &&
                     control._lastFilterByEquipment != (string)e.NewValue)
            {
                control._lastFilterByEquipment = (string)e.NewValue;
                control.UpdateItemsSource();
            }
        }

        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;
            control.UpdateItemsSource();
        }

        #endregion

        #region Обработчики событий

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (MainComboBox.Template.FindName("PART_EditableTextBox", MainComboBox) is TextBox textBox)
            {
                textBox.TextChanged += OnTextChanged;
                textBox.VerticalContentAlignment = VerticalAlignment.Center;
            }

            MainComboBox.SelectionChanged += OnSelectionChanged;
            MainComboBox.DropDownOpened += (s, e) => IsDropDownOpen = true;
            MainComboBox.DropDownClosed += (s, e) => IsDropDownOpen = false;
            MainComboBox.PreviewKeyDown += OnPreviewKeyDown;
            MainComboBox.LostFocus += OnLostFocus;

            UpdateItemsSource();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUpdatingText)
            {
                SearchText = ((TextBox)sender).Text;

                if (!string.IsNullOrEmpty(SearchText) && !IsDropDownOpen && HasItems)
                {
                    MainComboBox.IsDropDownOpen = true;
                }
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SelectedItem = e.AddedItems[0];
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && HasItems)
            {
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
                MainComboBox.IsDropDownOpen = false;
                e.Handled = true;
            }
        }

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
                SearchText = string.Empty;
                MainComboBox.Text = string.Empty;
            }
        }

        private void OnFilterItem(object sender, FilterEventArgs e)
        {
            if (e.Item == null)
            {
                e.Accepted = false;
                return;
            }

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

        private void UpdateItemsSource()
        {
            if (ItemsSource == null)
            {
                _viewSource.Source = null;
                FilteredItems = new List<object>();
                return;
            }

            var filteredByProps = ItemsSource.Cast<object>();

            // Фильтр по организации
            if (!string.IsNullOrEmpty(FilterByOrganization))
            {
                filteredByProps = filteredByProps.Where(i =>
                {
                    var prop = i.GetType().GetProperty("LessorOrganizationId");
                    return prop != null && prop.GetValue(i)?.ToString() == FilterByOrganization;
                });
            }

            // Фильтр по технике
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

        private void UpdateFilter()
        {
            _viewSource.View.Refresh();
            FilteredItems = _viewSource.View.Cast<object>().ToList();
        }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}