using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace OrderingSpecialEquipment.Controls
{
    /// <summary>
    /// Логика взаимодействия для SearchableComboBox.xaml
    /// </summary>
    public partial class SearchableComboBox : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SearchableComboBox()
        {
            InitializeComponent();
            MainComboBox.Loaded += (s, e) => UpdateFilteredItems();
            MainComboBox.TextInput += (s, e) => UpdateFilteredItems();
            MainComboBox.SelectionChanged += (s, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    SelectedItem = e.AddedItems[0];
                }
            };
            MainComboBox.LostFocus += (s, e) => UpdateFilteredItems();
        }

        // Зависимые свойства
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SearchableComboBox),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(SearchableComboBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(SearchableComboBox),
                new PropertyMetadata(""));

        public static readonly DependencyProperty FilterMemberPathProperty =
            DependencyProperty.Register("FilterMemberPath", typeof(string), typeof(SearchableComboBox),
                new PropertyMetadata("PlateDisplay"));

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

        // Свойства
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

        public string FilterMemberPath
        {
            get { return (string)GetValue(FilterMemberPathProperty); }
            set { SetValue(FilterMemberPathProperty, value); }
        }

        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
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

        // Приватные свойства
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    UpdateFilteredItems();
                }
            }
        }

        private IEnumerable _filteredItems = new List<object>();
        public IEnumerable FilteredItems
        {
            get => _filteredItems;
            set
            {
                _filteredItems = value;
                OnPropertyChanged();
            }
        }

        // Обработчики изменений
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;
            control.UpdateFilteredItems();
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
        }

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;
            control.UpdateFilteredItems();
        }

        // Обновление отфильтрованных элементов
        private void UpdateFilteredItems()
        {
            if (ItemsSource == null)
            {
                FilteredItems = new List<object>();
                return;
            }

            var items = ItemsSource.Cast<object>();

            // Фильтр по организации
            if (!string.IsNullOrEmpty(FilterByOrganization))
            {
                items = items.Where(i =>
                {
                    var prop = i.GetType().GetProperty("LessorOrganizationId");
                    return prop != null && prop.GetValue(i)?.ToString() == FilterByOrganization;
                });
            }

            // Фильтр по технике
            if (!string.IsNullOrEmpty(FilterByEquipment))
            {
                items = items.Where(i =>
                {
                    var prop = i.GetType().GetProperty("EquipmentId");
                    return prop != null && prop.GetValue(i)?.ToString() == FilterByEquipment;
                });
            }

            // Поиск по тексту
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                items = items.Where(i =>
                {
                    if (string.IsNullOrEmpty(FilterMemberPath))
                    {
                        return i.ToString()?.ToLower().Contains(searchLower) ?? false;
                    }

                    var prop = i.GetType().GetProperty(FilterMemberPath);
                    if (prop != null)
                    {
                        var value = prop.GetValue(i)?.ToString();
                        return value?.ToLower().Contains(searchLower) ?? false;
                    }

                    return false;
                });
            }

            FilteredItems = items.ToList();

            // Обновляем текст в ComboBox
            if (SelectedItem != null)
            {
                MainComboBox.Text = GetDisplayText(SelectedItem);
            }
        }

        private string GetDisplayText(object item)
        {
            if (item == null) return string.Empty;

            if (!string.IsNullOrEmpty(DisplayMemberPath))
            {
                var prop = item.GetType().GetProperty(DisplayMemberPath);
                if (prop != null)
                {
                    return prop.GetValue(item)?.ToString() ?? string.Empty;
                }
            }

            return item.ToString() ?? string.Empty;
        }
    }
}