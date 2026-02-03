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
    /// ViewModel для редактирования отделов
    /// </summary>
    public class EditDepartmentsViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDepartmentRepository _departmentRepository;

        private ObservableCollection<Department> _departments;
        private Department? _selectedDepartment;
        private bool _isEditMode;
        private string _searchText;

        public EditDepartmentsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _departmentRepository = serviceProvider.GetRequiredService<IDepartmentRepository>();

            Departments = new ObservableCollection<Department>();
            SearchText = string.Empty;

            // Команды
            LoadDepartmentsCommand = new RelayCommand(LoadDepartments);
            AddDepartmentCommand = new RelayCommand(AddDepartment);
            EditDepartmentCommand = new RelayCommand(EditDepartment, CanEditDepartment);
            DeleteDepartmentCommand = new RelayCommand(DeleteDepartment, CanDeleteDepartment);
            SaveDepartmentCommand = new RelayCommand(SaveDepartment, CanSaveDepartment);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SearchCommand = new RelayCommand(SearchDepartments);

            // Загрузка данных
            _ = LoadDepartmentsAsync();
        }

        // ========== Свойства ==========

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
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

        public ICommand LoadDepartmentsCommand { get; }
        public ICommand AddDepartmentCommand { get; }
        public ICommand EditDepartmentCommand { get; }
        public ICommand DeleteDepartmentCommand { get; }
        public ICommand SaveDepartmentCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SearchCommand { get; }

        // ========== Методы ==========

        /// <summary>
        /// Загружает все отделы
        /// </summary>
        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _departmentRepository.GetAllAsync();
                Departments = new ObservableCollection<Department>(departments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDepartments()
        {
            _ = LoadDepartmentsAsync();
        }

        /// <summary>
        /// Добавляет новый отдел
        /// </summary>
        private void AddDepartment()
        {
            SelectedDepartment = new Department
            {
                Id = GenerateNewId("DE"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            IsEditMode = true;
        }

        /// <summary>
        /// Редактирует выбранный отдел
        /// </summary>
        private void EditDepartment()
        {
            if (SelectedDepartment != null)
            {
                IsEditMode = true;
            }
        }

        private bool CanEditDepartment()
        {
            return SelectedDepartment != null && !IsEditMode;
        }

        /// <summary>
        /// Удаляет выбранный отдел
        /// </summary>
        private async void DeleteDepartment()
        {
            if (SelectedDepartment == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить отдел '{SelectedDepartment.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _departmentRepository.RemoveAsync(SelectedDepartment);
                    await LoadDepartmentsAsync();
                    SelectedDepartment = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления отдела: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteDepartment()
        {
            return SelectedDepartment != null && !IsEditMode;
        }

        /// <summary>
        /// Сохраняет отдел
        /// </summary>
        private async void SaveDepartment()
        {
            if (SelectedDepartment == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedDepartment.Name))
            {
                MessageBox.Show("Введите наименование отдела", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (Departments.Any(d => d.Id != SelectedDepartment.Id && d.Name == SelectedDepartment.Name))
                {
                    MessageBox.Show("Отдел с таким наименованием уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedDepartment.Key == 0)
                {
                    // Новый отдел
                    await _departmentRepository.AddAsync(SelectedDepartment);
                }
                else
                {
                    // Обновление существующего
                    _departmentRepository.Update(SelectedDepartment);
                }

                await LoadDepartmentsAsync();
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения отдела: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveDepartment()
        {
            return SelectedDepartment != null && IsEditMode;
        }

        /// <summary>
        /// Отменяет редактирование
        /// </summary>
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedDepartment = null;
        }

        /// <summary>
        /// Поиск отделов
        /// </summary>
        private async void SearchDepartments()
        {
            try
            {
                var allDepartments = await _departmentRepository.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var filtered = allDepartments.Where(d =>
                        d.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        d.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

                    Departments = new ObservableCollection<Department>(filtered);
                }
                else
                {
                    Departments = new ObservableCollection<Department>(allDepartments);
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
                var existingIds = Departments.Select(d => d.Id).Where(id => id.StartsWith(prefix));

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