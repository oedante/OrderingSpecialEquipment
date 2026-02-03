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
    /// ViewModel для редактирования зависимостей техники
    /// </summary>
    public class EditEquipmentDependenciesViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEquipmentDependencyRepository _dependencyRepository;
        private readonly IEquipmentRepository _equipmentRepository;

        private ObservableCollection<EquipmentDependency> _dependencies;
        private EquipmentDependency? _selectedDependency;
        private bool _isEditMode;
        private string _searchText;
        private ObservableCollection<Equipment> _equipmentList;

        public EditEquipmentDependenciesViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dependencyRepository = serviceProvider.GetRequiredService<IEquipmentDependencyRepository>();
            _equipmentRepository = serviceProvider.GetRequiredService<IEquipmentRepository>();

            Dependencies = new ObservableCollection<EquipmentDependency>();
            EquipmentList = new ObservableCollection<Equipment>();
            SearchText = string.Empty;

            // Команды
            LoadDependenciesCommand = new RelayCommand(LoadDependencies);
            AddDependencyCommand = new RelayCommand(AddDependency);
            EditDependencyCommand = new RelayCommand(EditDependency, CanEditDependency);
            DeleteDependencyCommand = new RelayCommand(DeleteDependency, CanDeleteDependency);
            SaveDependencyCommand = new RelayCommand(SaveDependency, CanSaveDependency);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SearchCommand = new RelayCommand(SearchDependencies);

            // Загрузка данных
            _ = LoadDataAsync();
        }

        // ========== Свойства ==========

        public ObservableCollection<EquipmentDependency> Dependencies
        {
            get => _dependencies;
            set => SetProperty(ref _dependencies, value);
        }

        public EquipmentDependency? SelectedDependency
        {
            get => _selectedDependency;
            set => SetProperty(ref _selectedDependency, value);
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

        public ObservableCollection<Equipment> EquipmentList
        {
            get => _equipmentList;
            set => SetProperty(ref _equipmentList, value);
        }

        // ========== Команды ==========

        public RelayCommand LoadDependenciesCommand { get; }
        public RelayCommand AddDependencyCommand { get; }
        public RelayCommand EditDependencyCommand { get; }
        public RelayCommand DeleteDependencyCommand { get; }
        public RelayCommand SaveDependencyCommand { get; }
        public RelayCommand CancelEditCommand { get; }
        public RelayCommand SearchCommand { get; }

        // ========== Методы ==========

        /// <summary>
        /// Загружает все данные (зависимости, техника)
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                await LoadDependenciesAsync();
                await LoadEquipmentAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает зависимости техники
        /// </summary>
        private async Task LoadDependenciesAsync()
        {
            try
            {
                var dependencies = await _dependencyRepository.GetAllAsync();
                Dependencies = new ObservableCollection<EquipmentDependency>(dependencies);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки зависимостей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDependencies()
        {
            _ = LoadDependenciesAsync();
        }

        /// <summary>
        /// Загружает технику для выпадающих списков
        /// </summary>
        private async Task LoadEquipmentAsync()
        {
            try
            {
                var equipment = await _equipmentRepository.GetActiveEquipmentAsync();
                EquipmentList = new ObservableCollection<Equipment>(equipment);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки техники: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавляет новую зависимость
        /// </summary>
        private void AddDependency()
        {
            SelectedDependency = new EquipmentDependency
            {
                RequiredCount = 1,
                IsMandatory = true,
                CreatedAt = DateTime.UtcNow
            };
            IsEditMode = true;
        }

        /// <summary>
        /// Редактирует выбранную зависимость
        /// </summary>
        private void EditDependency()
        {
            if (SelectedDependency != null)
            {
                IsEditMode = true;
            }
        }

        private bool CanEditDependency()
        {
            return SelectedDependency != null && !IsEditMode;
        }

        /// <summary>
        /// Удаляет выбранную зависимость
        /// </summary>
        private async void DeleteDependency()
        {
            if (SelectedDependency == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить зависимость '{SelectedDependency.MainEquipment?.Name} → {SelectedDependency.DependentEquipment?.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _dependencyRepository.RemoveAsync(SelectedDependency);
                    await LoadDependenciesAsync();
                    SelectedDependency = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления зависимости: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteDependency()
        {
            return SelectedDependency != null && !IsEditMode;
        }

        /// <summary>
        /// Сохраняет зависимость
        /// </summary>
        private async void SaveDependency()
        {
            if (SelectedDependency == null)
                return;

            // Валидация
            if (SelectedDependency.MainEquipmentId == null)
            {
                MessageBox.Show("Выберите основную технику", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDependency.DependentEquipmentId == null)
            {
                MessageBox.Show("Выберите зависимую технику", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDependency.MainEquipmentId == SelectedDependency.DependentEquipmentId)
            {
                MessageBox.Show("Основная и зависимая техника не могут быть одинаковыми", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDependency.RequiredCount <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Проверяем, не существует ли уже такой зависимости
                var existing = await _dependencyRepository.FindAsync(d =>
                    d.MainEquipmentId == SelectedDependency.MainEquipmentId &&
                    d.DependentEquipmentId == SelectedDependency.DependentEquipmentId);

                if (existing.Any() && SelectedDependency.Key == 0)
                {
                    MessageBox.Show("Зависимость для этих техник уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedDependency.Key == 0)
                {
                    // Новая зависимость
                    await _dependencyRepository.AddAsync(SelectedDependency);
                }
                else
                {
                    // Обновление существующей
                    _dependencyRepository.Update(SelectedDependency);
                }

                await LoadDependenciesAsync();
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения зависимости: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveDependency()
        {
            return SelectedDependency != null && IsEditMode;
        }

        /// <summary>
        /// Отменяет редактирование
        /// </summary>
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedDependency = null;
        }

        /// <summary>
        /// Поиск зависимостей
        /// </summary>
        private async void SearchDependencies()
        {
            try
            {
                var allDependencies = await _dependencyRepository.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var filtered = allDependencies.Where(d =>
                        (d.MainEquipment?.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (d.DependentEquipment?.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (d.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));

                    Dependencies = new ObservableCollection<EquipmentDependency>(filtered);
                }
                else
                {
                    Dependencies = new ObservableCollection<EquipmentDependency>(allDependencies);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}