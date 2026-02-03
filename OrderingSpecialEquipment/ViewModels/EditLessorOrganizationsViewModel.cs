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
    /// ViewModel для редактирования организаций-арендодателей
    /// </summary>
    public class EditLessorOrganizationsViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILessorOrganizationRepository _organizationRepository;

        private ObservableCollection<LessorOrganization> _organizations;
        private LessorOrganization? _selectedOrganization;
        private bool _isEditMode;
        private string _searchText;

        public EditLessorOrganizationsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _organizationRepository = serviceProvider.GetRequiredService<ILessorOrganizationRepository>();

            Organizations = new ObservableCollection<LessorOrganization>();
            SearchText = string.Empty;

            // Команды
            LoadOrganizationsCommand = new RelayCommand(LoadOrganizations);
            AddOrganizationCommand = new RelayCommand(AddOrganization);
            EditOrganizationCommand = new RelayCommand(EditOrganization, CanEditOrganization);
            DeleteOrganizationCommand = new RelayCommand(DeleteOrganization, CanDeleteOrganization);
            SaveOrganizationCommand = new RelayCommand(SaveOrganization, CanSaveOrganization);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SearchCommand = new RelayCommand(SearchOrganizations);

            // Загрузка данных
            _ = LoadOrganizationsAsync();
        }

        // ========== Свойства ==========

        public ObservableCollection<LessorOrganization> Organizations
        {
            get => _organizations;
            set => SetProperty(ref _organizations, value);
        }

        public LessorOrganization? SelectedOrganization
        {
            get => _selectedOrganization;
            set => SetProperty(ref _selectedOrganization, value);
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

        // ========== Команды ==========

        public RelayCommand LoadOrganizationsCommand { get; }
        public RelayCommand AddOrganizationCommand { get; }
        public RelayCommand EditOrganizationCommand { get; }
        public RelayCommand DeleteOrganizationCommand { get; }
        public RelayCommand SaveOrganizationCommand { get; }
        public RelayCommand CancelEditCommand { get; }
        public RelayCommand SearchCommand { get; }

        // ========== Методы ==========

        /// <summary>
        /// Загружает организации-арендодатели
        /// </summary>
        private async Task LoadOrganizationsAsync()
        {
            try
            {
                var organizations = await _organizationRepository.GetAllAsync();
                Organizations = new ObservableCollection<LessorOrganization>(organizations);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки организаций: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrganizations()
        {
            _ = LoadOrganizationsAsync();
        }

        /// <summary>
        /// Добавляет новую организацию
        /// </summary>
        private void AddOrganization()
        {
            SelectedOrganization = new LessorOrganization
            {
                Id = GenerateNewId("LO"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            IsEditMode = true;
        }

        /// <summary>
        /// Редактирует выбранную организацию
        /// </summary>
        private void EditOrganization()
        {
            if (SelectedOrganization != null)
            {
                IsEditMode = true;
            }
        }

        private bool CanEditOrganization()
        {
            return SelectedOrganization != null && !IsEditMode;
        }

        /// <summary>
        /// Удаляет выбранную организацию
        /// </summary>
        private async void DeleteOrganization()
        {
            if (SelectedOrganization == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить организацию '{SelectedOrganization.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _organizationRepository.RemoveAsync(SelectedOrganization);
                    await LoadOrganizationsAsync();
                    SelectedOrganization = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления организации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteOrganization()
        {
            return SelectedOrganization != null && !IsEditMode;
        }

        /// <summary>
        /// Сохраняет организацию
        /// </summary>
        private async void SaveOrganization()
        {
            if (SelectedOrganization == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedOrganization.Name))
            {
                MessageBox.Show("Введите наименование организации", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (Organizations.Any(o => o.Id != SelectedOrganization.Id && o.Name == SelectedOrganization.Name))
                {
                    MessageBox.Show("Организация с таким наименованием уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!string.IsNullOrEmpty(SelectedOrganization.INN) &&
                    Organizations.Any(o => o.Id != SelectedOrganization.Id && o.INN == SelectedOrganization.INN))
                {
                    MessageBox.Show("Организация с таким ИНН уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedOrganization.Key == 0)
                {
                    // Новая организация
                    await _organizationRepository.AddAsync(SelectedOrganization);
                }
                else
                {
                    // Обновление существующей
                    _organizationRepository.Update(SelectedOrganization);
                }

                await LoadOrganizationsAsync();
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения организации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveOrganization()
        {
            return SelectedOrganization != null && IsEditMode;
        }

        /// <summary>
        /// Отменяет редактирование
        /// </summary>
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedOrganization = null;
        }

        /// <summary>
        /// Поиск организаций
        /// </summary>
        private async void SearchOrganizations()
        {
            try
            {
                var allOrganizations = await _organizationRepository.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var filtered = allOrganizations.Where(o =>
                        o.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        o.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (o.INN != null && o.INN.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

                    Organizations = new ObservableCollection<LessorOrganization>(filtered);
                }
                else
                {
                    Organizations = new ObservableCollection<LessorOrganization>(allOrganizations);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Генерирует новый идентификатор в формате префикс + номер
        /// </summary>
        private string GenerateNewId(string prefix)
        {
            try
            {
                var existingIds = Organizations.Select(o => o.Id).Where(id => id.StartsWith(prefix));

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