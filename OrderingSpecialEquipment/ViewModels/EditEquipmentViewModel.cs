using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// ViewModel для редактирования техники
    /// </summary>
    public class EditEquipmentsViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEquipmentRepository _equipmentRepository;

        private ObservableCollection<Equipment> _equipments;
        private Equipment? _selectedEquipment;
        private bool _isEditMode;
        private string _searchText;
        private ObservableCollection<string> _categories;

        public EditEquipmentsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _equipmentRepository = serviceProvider.GetRequiredService<IEquipmentRepository>();

            Equipments = new ObservableCollection<Equipment>();
            Categories = new ObservableCollection<string> { "Спецтехника", "Рабочий", "Оборудование" };
            SearchText = string.Empty;

            // Команды
            LoadEquipmentsCommand = new RelayCommand(LoadEquipments);
            AddEquipmentCommand = new RelayCommand(AddEquipment);
            EditEquipmentCommand = new RelayCommand(EditEquipment, CanEditEquipment);
            DeleteEquipmentCommand = new RelayCommand(DeleteEquipment, CanDeleteEquipment);
            SaveEquipmentCommand = new RelayCommand(SaveEquipment, CanSaveEquipment);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SearchCommand = new RelayCommand(SearchEquipments);

            // Загрузка данных
            _ = LoadEquipmentsAsync();
        }

        // ========== Свойства ==========

        public ObservableCollection<Equipment> Equipments
        {
            get => _equipments;
            set => SetProperty(ref _equipments, value);
        }

        public Equipment? SelectedEquipment
        {
            get => _selectedEquipment;
            set => SetProperty(ref _selectedEquipment, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        // ========== Команды ==========

        public ICommand LoadEquipmentsCommand { get; }
        public ICommand AddEquipmentCommand { get; }
        public ICommand EditEquipmentCommand { get; }
        public ICommand DeleteEquipmentCommand { get; }
        public ICommand SaveEquipmentCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SearchCommand { get; }

        // ========== Методы ==========

        /// <summary>
        /// Загружает всю технику
        /// </summary>
        private async Task LoadEquipmentsAsync()
        {
            try
            {
                var equipments = await _equipmentRepository.GetAllAsync();
                Equipments = new ObservableCollection<Equipment>(equipments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки техники: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadEquipments()
        {
            _ = LoadEquipmentsAsync();
        }

        /// <summary>
        /// Добавляет новую технику
        /// </summary>
        private void AddEquipment()
        {
            SelectedEquipment = new Equipment
            {
                Id = GenerateNewId("EQ"),
                IsActive = true,
                CanOrderMultiple = false,
                RequiresOperator = false,
                CreatedAt = DateTime.UtcNow
            };
            IsEditMode = true;
        }

        /// <summary>
        /// Редактирует выбранную технику
        /// </summary>
        private void EditEquipment()
        {
            if (SelectedEquipment != null)
            {
                IsEditMode = true;
            }
        }

        private bool CanEditEquipment()
        {
            return SelectedEquipment != null && !IsEditMode;
        }

        /// <summary>
        /// Удаляет выбранную технику
        /// </summary>
        private async void DeleteEquipment()
        {
            if (SelectedEquipment == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить технику '{SelectedEquipment.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _equipmentRepository.RemoveAsync(SelectedEquipment);
                    await LoadEquipmentsAsync();
                    SelectedEquipment = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления техники: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteEquipment()
        {
            return SelectedEquipment != null && !IsEditMode;
        }

        /// <summary>
        /// Сохраняет технику
        /// </summary>
        private async void SaveEquipment()
        {
            if (SelectedEquipment == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedEquipment.Name))
            {
                MessageBox.Show("Введите наименование техники", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (Equipments.Any(e => e.Id != SelectedEquipment.Id && e.Name == SelectedEquipment.Name))
                {
                    MessageBox.Show("Техника с таким наименованием уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedEquipment.Key == 0)
                {
                    // Новая техника
                    await _equipmentRepository.AddAsync(SelectedEquipment);
                }
                else
                {
                    // Обновление существующей
                    _equipmentRepository.Update(SelectedEquipment);
                }

                await LoadEquipmentsAsync();
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения техники: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveEquipment()
        {
            return SelectedEquipment != null && IsEditMode;
        }

        /// <summary>
        /// Отменяет редактирование
        /// </summary>
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedEquipment = null;
        }

        /// <summary>
        /// Поиск техники
        /// </summary>
        private async void SearchEquipments()
        {
            try
            {
                var allEquipments = await _equipmentRepository.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var filtered = allEquipments.Where(e =>
                        e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        e.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (e.Category != null && e.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

                    Equipments = new ObservableCollection<Equipment>(filtered);
                }
                else
                {
                    Equipments = new ObservableCollection<Equipment>(allEquipments);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Генерирует новый идентификатор
        /// </summary>
        private string GenerateNewId(string prefix)
        {
            try
            {
                var existingIds = Equipments.Select(e => e.Id).Where(id => id.StartsWith(prefix));

                if (!existingIds.Any())
                {
                    return $"{prefix}000001";
                }

                var maxNumber = existingIds
                    .Select(id => int.TryParse(id.Substring(prefix.Length), out int num) ? num : 0)
                    .Max();

                return $"{prefix}{(maxNumber + 1):D6}";
            }
            catch
            {
                return $"{prefix}000001";
            }
        }
    }
}