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
    /// ViewModel для редактирования транспортной программы
    /// </summary>
    public class EditTransportProgramViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITransportProgramRepository _programRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEquipmentRepository _equipmentRepository;

        private ObservableCollection<TransportProgram> _transportProgram;
        private TransportProgram? _selectedProgram;
        private bool _isEditMode;
        private string _searchText;
        private ObservableCollection<Department> _departments;
        private ObservableCollection<Equipment> _equipments;
        private ObservableCollection<int> _years;
        private int? _selectedYear;
        private string? _selectedDepartmentId;

        public EditTransportProgramViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _programRepository = serviceProvider.GetRequiredService<ITransportProgramRepository>();
            _departmentRepository = serviceProvider.GetRequiredService<IDepartmentRepository>();
            _equipmentRepository = serviceProvider.GetRequiredService<IEquipmentRepository>();

            TransportProgram = new ObservableCollection<TransportProgram>();
            Departments = new ObservableCollection<Department>();
            Equipments = new ObservableCollection<Equipment>();
            Years = new ObservableCollection<int>();
            SearchText = string.Empty;

            // Заполнение списка лет (текущий и 5 предыдущих)
            for (int i = 0; i <= 5; i++)
            {
                Years.Add(DateTime.Today.Year - i);
            }
            SelectedYear = DateTime.Today.Year;

            // Команды
            LoadProgramCommand = new RelayCommand(LoadProgram);
            AddProgramCommand = new RelayCommand(AddProgram);
            EditProgramCommand = new RelayCommand(EditProgram, CanEditProgram);
            DeleteProgramCommand = new RelayCommand(DeleteProgram, CanDeleteProgram);
            SaveProgramCommand = new RelayCommand(SaveProgram, CanSaveProgram);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SearchCommand = new RelayCommand(SearchProgram);

            // Загрузка данных
            _ = LoadDataAsync();
        }

        // ========== Свойства ==========

        public ObservableCollection<TransportProgram> TransportProgram
        {
            get => _transportProgram;
            set => SetProperty(ref _transportProgram, value);
        }

        public TransportProgram? SelectedProgram
        {
            get => _selectedProgram;
            set => SetProperty(ref _selectedProgram, value);
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

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public ObservableCollection<Equipment> Equipments
        {
            get => _equipments;
            set => SetProperty(ref _equipments, value);
        }

        public ObservableCollection<int> Years
        {
            get => _years;
            set => SetProperty(ref _years, value);
        }

        public int? SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    _ = LoadProgramAsync();
                }
            }
        }

        public string? SelectedDepartmentId
        {
            get => _selectedDepartmentId;
            set
            {
                if (SetProperty(ref _selectedDepartmentId, value))
                {
                    _ = LoadProgramAsync();
                }
            }
        }

        // ========== Команды ==========

        public RelayCommand LoadProgramCommand { get; }
        public RelayCommand AddProgramCommand { get; }
        public RelayCommand EditProgramCommand { get; }
        public RelayCommand DeleteProgramCommand { get; }
        public RelayCommand SaveProgramCommand { get; }
        public RelayCommand CancelEditCommand { get; }
        public RelayCommand SearchCommand { get; }

        // ========== Методы ==========

        /// <summary>
        /// Загружает все данные (программы, отделы, техника)
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                await LoadDepartmentsAsync();
                await LoadEquipmentsAsync();
                await LoadProgramAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает транспортную программу
        /// </summary>
        private async Task LoadProgramAsync()
        {
            try
            {
                var programs = await _programRepository.GetAllAsync();

                if (SelectedYear.HasValue)
                {
                    programs = programs.Where(p => p.Year == SelectedYear.Value);
                }

                if (!string.IsNullOrEmpty(SelectedDepartmentId))
                {
                    programs = programs.Where(p => p.DepartmentId == SelectedDepartmentId);
                }

                TransportProgram = new ObservableCollection<TransportProgram>(programs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки транспортной программы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProgram()
        {
            _ = LoadProgramAsync();
        }

        /// <summary>
        /// Загружает отделы
        /// </summary>
        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _departmentRepository.GetActiveDepartmentsAsync();
                Departments = new ObservableCollection<Department>(departments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает технику
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
        /// Добавляет новую запись транспортной программы
        /// </summary>
        private void AddProgram()
        {
            SelectedProgram = new TransportProgram
            {
                Year = SelectedYear ?? DateTime.Today.Year,
                HourlyCost = 0,
                JanuaryHours = 0,
                FebruaryHours = 0,
                MarchHours = 0,
                AprilHours = 0,
                MayHours = 0,
                JuneHours = 0,
                JulyHours = 0,
                AugustHours = 0,
                SeptemberHours = 0,
                OctoberHours = 0,
                NovemberHours = 0,
                DecemberHours = 0,
                CreatedAt = DateTime.UtcNow
            };
            IsEditMode = true;
        }

        /// <summary>
        /// Редактирует выбранную запись
        /// </summary>
        private void EditProgram()
        {
            if (SelectedProgram != null)
            {
                IsEditMode = true;
            }
        }

        private bool CanEditProgram()
        {
            return SelectedProgram != null && !IsEditMode;
        }

        /// <summary>
        /// Удаляет выбранную запись
        /// </summary>
        private async void DeleteProgram()
        {
            if (SelectedProgram == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить запись транспортной программы для {SelectedProgram.Department?.Name} - {SelectedProgram.Equipment?.Name} ({SelectedProgram.Year} год)?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _programRepository.RemoveAsync(SelectedProgram);
                    await LoadProgramAsync();
                    SelectedProgram = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteProgram()
        {
            return SelectedProgram != null && !IsEditMode;
        }

        /// <summary>
        /// Сохраняет запись транспортной программы
        /// </summary>
        private async void SaveProgram()
        {
            if (SelectedProgram == null)
                return;

            // Валидация
            if (SelectedProgram.DepartmentId == null)
            {
                MessageBox.Show("Выберите отдел", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedProgram.EquipmentId == null)
            {
                MessageBox.Show("Выберите технику", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedProgram.Year == 0)
            {
                MessageBox.Show("Введите год", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Проверяем, не существует ли уже такой записи
                var existing = await _programRepository.FindAsync(p =>
                    p.DepartmentId == SelectedProgram.DepartmentId &&
                    p.EquipmentId == SelectedProgram.EquipmentId &&
                    p.Year == SelectedProgram.Year);

                if (existing.Any() && SelectedProgram.Key == 0)
                {
                    MessageBox.Show("Запись для этого отдела, техники и года уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedProgram.Key == 0)
                {
                    // Новая запись
                    await _programRepository.AddAsync(SelectedProgram);
                }
                else
                {
                    // Обновление существующей
                    _programRepository.Update(SelectedProgram);
                }

                await LoadProgramAsync();
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveProgram()
        {
            return SelectedProgram != null && IsEditMode;
        }

        /// <summary>
        /// Отменяет редактирование
        /// </summary>
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedProgram = null;
        }

        /// <summary>
        /// Поиск по транспортной программе
        /// </summary>
        private async void SearchProgram()
        {
            try
            {
                await LoadProgramAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}