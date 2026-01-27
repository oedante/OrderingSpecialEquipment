using OrderingSpecialEquipment.Commands;
using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    /// <summary>
    /// ViewModel для окна редактирования отделов.
    /// </summary>
    public class EditDepartmentsViewModel : ViewModelBase
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        private ObservableCollection<Department> _departments;
        private Department? _selectedDepartment;
        private bool _isEditing = false;
        private string _editId = string.Empty;
        private string _editName = string.Empty;
        private bool _editIsActive = true;

        public EditDepartmentsViewModel(
            IDepartmentRepository departmentRepository,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _departmentRepository = departmentRepository;
            _messageService = messageService;
            _authorizationService = authorizationService;

            _departments = new ObservableCollection<Department>();
            LoadDepartmentsCommand = new RelayCommand(async _ => await LoadDepartmentsAsync(), _ => _authorizationService.CanReadTable("Departments"));
            SaveDepartmentCommand = new RelayCommand(async _ => await SaveDepartmentAsync(), _ => CanSaveDepartment());
            DeleteDepartmentCommand = new RelayCommand(async _ => await DeleteDepartmentAsync(), _ => CanDeleteDepartment());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());

            // Загрузка данных при создании ViewModel
            Task.Run(async () => await LoadDepartmentsAsync());
        }

        // Свойства для привязки к UI
        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                {
                    if (value != null)
                    {
                        _editId = value.Id;
                        _editName = value.Name;
                        _editIsActive = value.IsActive;
                        _isEditing = true;
                    }
                    else
                    {
                        ResetEditFields();
                    }
                    OnPropertyChanged(nameof(EditId));
                    OnPropertyChanged(nameof(EditName));
                    OnPropertyChanged(nameof(EditIsActive));
                    // Вызов RaiseCanExecuteChanged для команд
                    ((RelayCommand)SaveDepartmentCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteDepartmentCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditId
        {
            get => _editId;
            set
            {
                if (SetProperty(ref _editId, value))
                {
                    // Вызов RaiseCanExecuteChanged для команд
                    ((RelayCommand)SaveDepartmentCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditName
        {
            get => _editName;
            set => SetProperty(ref _editName, value);
        }

        public bool EditIsActive
        {
            get => _editIsActive;
            set => SetProperty(ref _editIsActive, value);
        }

        // Команды
        public ICommand LoadDepartmentsCommand { get; }
        public ICommand SaveDepartmentCommand { get; }
        public ICommand DeleteDepartmentCommand { get; }
        public ICommand CancelEditCommand { get; }

        // Методы команд
        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var dbDepartments = await _departmentRepository.GetAllAsync(); // Или GetActiveAsync()
                Departments.Clear();
                foreach (var dept in dbDepartments)
                {
                    if (dept.IsActive) // Фильтруем неактивные при отображении, если нужно
                        Departments.Add(dept);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки отделов: {ex.Message}", "Ошибка");
            }
        }

        private bool CanSaveDepartment()
        {
            return _authorizationService.CanWriteTable("Departments") &&
                   !string.IsNullOrWhiteSpace(EditId) &&
                   !string.IsNullOrWhiteSpace(EditName) &&
                   (!_isEditing || SelectedDepartment?.Id == EditId); // Если редактируем, ID должен совпадать
        }

        private async Task SaveDepartmentAsync()
        {
            if (!CanSaveDepartment()) return;

            try
            {
                Department department;
                bool isNew = !_isEditing;

                if (isNew)
                {
                    // Проверка на уникальность ID
                    if (await _departmentRepository.ExistsAsync(EditId))
                    {
                        _messageService.ShowErrorMessage($"Отдел с ID '{EditId}' уже существует.", "Ошибка");
                        return;
                    }
                    department = new Department { Id = EditId, Name = EditName, IsActive = EditIsActive };
                    await _departmentRepository.AddAsync(department);
                }
                else
                {
                    department = SelectedDepartment!;
                    department.Name = EditName;
                    department.IsActive = EditIsActive;
                    _departmentRepository.Update(department);
                }

                await _departmentRepository.SaveChangesAsync();

                if (isNew)
                {
                    if (department.IsActive) // Только если активен
                        Departments.Add(department);
                }
                else
                {
                    // Обновление в ObservableCollection может не сработать автоматически, если Name изменилось.
                    // Лучше обновить весь список или реализовать INotifyPropertyChanged в Department.
                    // Для простоты, перезагрузим список.
                    await LoadDepartmentsAsync();
                }

                ResetEditFields();
                _messageService.ShowInfoMessage(isNew ? "Отдел добавлен." : "Отдел обновлен.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка сохранения отдела: {ex.Message}", "Ошибка");
            }
        }

        private bool CanDeleteDepartment()
        {
            return _authorizationService.CanWriteTable("Departments") &&
                   SelectedDepartment != null && SelectedDepartment.Key != 0; // Предполагаем, что Key != 0 для существующих записей
        }

        private async Task DeleteDepartmentAsync()
        {
            if (!CanDeleteDepartment() || SelectedDepartment == null) return;

            if (!_messageService.ShowConfirmationDialog($"Вы действительно хотите удалить отдел '{SelectedDepartment.Name}'?")) return;

            try
            {
                // Удаление может быть логическим (установка IsActive = false) или физическим.
                // Пока делаем физическое удаление.
                _departmentRepository.Delete(SelectedDepartment);
                await _departmentRepository.SaveChangesAsync();
                Departments.Remove(SelectedDepartment);
                ResetEditFields();
                _messageService.ShowInfoMessage("Отдел удален.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка удаления отдела: {ex.Message}", "Ошибка");
            }
        }

        private void CancelEdit()
        {
            ResetEditFields();
        }

        private void ResetEditFields()
        {
            _isEditing = false;
            _editId = string.Empty;
            _editName = string.Empty;
            _editIsActive = true;
            SelectedDepartment = null; // Это должно сбросить UI
            OnPropertyChanged(nameof(EditId));
            OnPropertyChanged(nameof(EditName));
            OnPropertyChanged(nameof(EditIsActive));
            ((RelayCommand)SaveDepartmentCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteDepartmentCommand).RaiseCanExecuteChanged();
        }
    }
}