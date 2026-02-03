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
    /// ViewModel для редактирования госномеров
    /// </summary>
    public class EditLicensePlatesViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILicensePlateRepository _licensePlateRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ILessorOrganizationRepository _lessorOrgRepository;

        private ObservableCollection<LicensePlate> _licensePlates;
        private LicensePlate? _selectedLicensePlate;
        private bool _isEditMode;
        private string _searchText;
        private ObservableCollection<Equipment> _equipments;
        private ObservableCollection<LessorOrganization> _lessorOrganizations;

        public EditLicensePlatesViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _licensePlateRepository = serviceProvider.GetRequiredService<ILicensePlateRepository>();
            _equipmentRepository = serviceProvider.GetRequiredService<IEquipmentRepository>();
            _lessorOrgRepository = serviceProvider.GetRequiredService<ILessorOrganizationRepository>();

            LicensePlates = new ObservableCollection<LicensePlate>();
            Equipments = new ObservableCollection<Equipment>();
            LessorOrganizations = new ObservableCollection<LessorOrganization>();
            SearchText = string.Empty;

            // Команды
            LoadLicensePlatesCommand = new RelayCommand(LoadLicensePlates);
            AddLicensePlateCommand = new RelayCommand(AddLicensePlate);
            EditLicensePlateCommand = new RelayCommand(EditLicensePlate, CanEditLicensePlate);
            DeleteLicensePlateCommand = new RelayCommand(DeleteLicensePlate, CanDeleteLicensePlate);
            SaveLicensePlateCommand = new RelayCommand(SaveLicensePlate, CanSaveLicensePlate);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SearchCommand = new RelayCommand(SearchLicensePlates);

            // Загрузка данных
            _ = LoadDataAsync();
        }

        // ========== Свойства ==========

        public ObservableCollection<LicensePlate> LicensePlates
        {
            get => _licensePlates;
            set => SetProperty(ref _licensePlates, value);
        }

        public LicensePlate? SelectedLicensePlate
        {
            get => _selectedLicensePlate;
            set => SetProperty(ref _selectedLicensePlate, value);
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

        public ObservableCollection<Equipment> Equipments
        {
            get => _equipments;
            set => SetProperty(ref _equipments, value);
        }

        public ObservableCollection<LessorOrganization> LessorOrganizations
        {
            get => _lessorOrganizations;
            set => SetProperty(ref _lessorOrganizations, value);
        }

        // ========== Команды ==========

        public RelayCommand LoadLicensePlatesCommand { get; }
        public RelayCommand AddLicensePlateCommand { get; }
        public RelayCommand EditLicensePlateCommand { get; }
        public RelayCommand DeleteLicensePlateCommand { get; }
        public RelayCommand SaveLicensePlateCommand { get; }
        public RelayCommand CancelEditCommand { get; }
        public RelayCommand SearchCommand { get; }

        // ========== Методы ==========

        /// <summary>
        /// Загружает все данные (госномера, техника, организации)
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                await LoadLicensePlatesAsync();
                await LoadEquipmentsAsync();
                await LoadLessorOrganizationsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает госномера
        /// </summary>
        private async Task LoadLicensePlatesAsync()
        {
            try
            {
                var plates = await _licensePlateRepository.GetAllAsync();
                LicensePlates = new ObservableCollection<LicensePlate>(plates);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки госномеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLicensePlates()
        {
            _ = LoadLicensePlatesAsync();
        }

        /// <summary>
        /// Загружает технику для выпадающего списка
        /// </summary>
        private async Task LoadEquipmentsAsync()
        {
            try
            {
                var equipments = await _equipmentRepository.GetActiveEquipmentAsync();
                Equipments = new ObservableCollection<Equipment>(equipments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки техники: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает организации для выпадающего списка
        /// </summary>
        private async Task LoadLessorOrganizationsAsync()
        {
            try
            {
                var orgs = await _lessorOrgRepository.GetActiveLessorOrganizationsAsync();
                LessorOrganizations = new ObservableCollection<LessorOrganization>(orgs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки организаций: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавляет новый госномер
        /// </summary>
        private void AddLicensePlate()
        {
            SelectedLicensePlate = new LicensePlate
            {
                Id = GenerateNewId("LP"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            IsEditMode = true;
        }

        /// <summary>
        /// Редактирует выбранный госномер
        /// </summary>
        private void EditLicensePlate()
        {
            if (SelectedLicensePlate != null)
            {
                IsEditMode = true;
            }
        }

        private bool CanEditLicensePlate()
        {
            return SelectedLicensePlate != null && !IsEditMode;
        }

        /// <summary>
        /// Удаляет выбранный госномер
        /// </summary>
        private async void DeleteLicensePlate()
        {
            if (SelectedLicensePlate == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить госномер '{SelectedLicensePlate.PlateNumber}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _licensePlateRepository.RemoveAsync(SelectedLicensePlate);
                    await LoadLicensePlatesAsync();
                    SelectedLicensePlate = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления госномера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteLicensePlate()
        {
            return SelectedLicensePlate != null && !IsEditMode;
        }

        /// <summary>
        /// Сохраняет госномер
        /// </summary>
        private async void SaveLicensePlate()
        {
            if (SelectedLicensePlate == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedLicensePlate.PlateNumber))
            {
                MessageBox.Show("Введите госномер", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedLicensePlate.EquipmentId))
            {
                MessageBox.Show("Выберите технику", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedLicensePlate.LessorOrganizationId))
            {
                MessageBox.Show("Выберите организацию-арендодателя", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (LicensePlates.Any(lp => lp.Id != SelectedLicensePlate.Id && lp.PlateNumber == SelectedLicensePlate.PlateNumber))
                {
                    MessageBox.Show("Госномер с таким номером уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedLicensePlate.Key == 0)
                {
                    // Новый госномер - ID будет сгенерирован триггером в БД
                    SelectedLicensePlate.Id = null; // Устанавливаем в null для генерации триггером
                    await _licensePlateRepository.AddAsync(SelectedLicensePlate);
                }
                else
                {
                    // Обновление существующего
                    _licensePlateRepository.Update(SelectedLicensePlate);
                }

                await LoadLicensePlatesAsync();
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения госномера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveLicensePlate()
        {
            return SelectedLicensePlate != null && IsEditMode;
        }

        /// <summary>
        /// Отменяет редактирование
        /// </summary>
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedLicensePlate = null;
        }

        /// <summary>
        /// Поиск госномеров с защитой от SQL инъекций
        /// </summary>
        private async void SearchLicensePlates()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadLicensePlatesAsync();
                    return;
                }

                var results = await _licensePlateRepository.SearchByPlateNumberAsync(SearchText);
                LicensePlates = new ObservableCollection<LicensePlate>(results);
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
                var existingIds = LicensePlates.Select(lp => lp.Id).Where(id => id.StartsWith(prefix));

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