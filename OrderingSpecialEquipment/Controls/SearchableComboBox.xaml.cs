using OrderingSpecialEquipment.Models;
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

            // Подписываемся на события после инициализации
            Loaded += (s, e) =>
            {
                UpdateFilteredItems();

                // Подписываемся на изменения текста через TextBox внутри ComboBox
                if (MainComboBox.Template.FindName("PART_EditableTextBox", MainComboBox) is TextBox textBox)
                {
                    textBox.TextChanged += (_, _) =>
                    {
                        SearchText = textBox.Text;
                        UpdateFilteredItems();
                    };
                }
            };

            MainComboBox.SelectionChanged += (s, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    SelectedItem = e.AddedItems[0];
                    SearchText = GetDisplayText(e.AddedItems[0]);
                }
            };

            MainComboBox.LostFocus += (s, e) =>
            {
                if (SelectedItem != null)
                {
                    MainComboBox.Text = GetDisplayText(SelectedItem);
                }
            };
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
                new PropertyMetadata("", OnDisplayMemberPathChanged));

        public static readonly DependencyProperty FilterMemberPathProperty =
            DependencyProperty.Register("FilterMemberPath", typeof(string), typeof(SearchableComboBox),
                new PropertyMetadata("", OnFilterMemberPathChanged));

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

            if (item != null)
            {
                control.MainComboBox.Text = control.GetDisplayText(item);
            }
        }

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;
            control.UpdateFilteredItems();
        }

        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;
            control.UpdateFilteredItems();
        }

        private static void OnFilterMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableComboBox)d;
            control.UpdateFilteredItems();
        }

        // Обновление отфильтрованных элементов
        private void UpdateFilteredItems()
        {
            try
            {
                if (ItemsSource == null)
                {
                    FilteredItems = new List<object>();
                    return;
                }

                var items = ItemsSource.Cast<object>().ToList();

                // Фильтр по организации
                if (!string.IsNullOrEmpty(FilterByOrganization))
                {
                    items = items.Where(i =>
                    {
                        var prop = i.GetType().GetProperty("LessorOrganizationId");
                        return prop != null && prop.GetValue(i)?.ToString() == FilterByOrganization;
                    }).ToList();
                }

                // Фильтр по технике
                if (!string.IsNullOrEmpty(FilterByEquipment))
                {
                    items = items.Where(i =>
                    {
                        var prop = i.GetType().GetProperty("EquipmentId");
                        return prop != null && prop.GetValue(i)?.ToString() == FilterByEquipment;
                    }).ToList();
                }

                // Поиск по тексту
                if (!string.IsNullOrEmpty(SearchText))
                {
                    var searchLower = SearchText.ToLower();
                    items = items.Where(i =>
                    {
                        var displayText = GetDisplayText(i);
                        return displayText.ToLower().Contains(searchLower);
                    }).ToList();
                }

                FilteredItems = items;

                // Обновляем текст в ComboBox если выбран элемент
                if (SelectedItem != null && items.Contains(SelectedItem))
                {
                    MainComboBox.Text = GetDisplayText(SelectedItem);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в UpdateFilteredItems: {ex.Message}");
                FilteredItems = new List<object>();
            }
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

                // Если DisplayMemberPath не задан или не найден, используем стандартное отображение
                if (item is LicensePlate plate)
                {
                    return $"{plate.PlateNumber} - {plate.Brand}";
                }

                return item.ToString() ?? string.Empty;
            }
            catch
            {
                return item.ToString() ?? string.Empty;
            }
        }
    }
}