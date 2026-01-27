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
    /// ViewModel для окна редактирования техники.
    /// </summary>
    public class EditEquipmentViewModel : ViewModelBase
    {
        private readonly IEquipmentRepositoryBase _equipmentRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        private ObservableCollection<Equipment> _equipments;
        private Equipment? _selectedEquipment;
        private bool _isEditing = false;
        private string _editId = string.Empty;
        private string _editName = string.Empty;
        private string? _editCategory;
        private bool _editCanOrderMultiple = false;
        private decimal? _editHourlyCost;
        private bool _editRequiresOperator = false;
        private string? _editDescription;
        private bool _editIsActive = true;

        public EditEquipmentViewModel(
            IEquipmentRepositoryBase equipmentRepository,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _equipmentRepository = equipmentRepository;
            _messageService = messageService;
            _authorizationService = authorizationService;

            _equipments = new ObservableCollection<Equipment>();
            LoadEquipmentsCommand = new RelayCommand(async _ => await LoadEquipmentsAsync(), _ => _authorizationService.CanReadTable("Equipments"));
            SaveEquipmentCommand = new RelayCommand(async _ => await SaveEquipmentAsync(), _ => CanSaveEquipment());
            DeleteEquipmentCommand = new RelayCommand(async _ => await DeleteEquipmentAsync(), _ => CanDeleteEquipment());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());

            Task.Run(async () => await LoadEquipmentsAsync());
        }

        public ObservableCollection<Equipment> Equipments
        {
            get => _equipments;
            set => SetProperty(ref _equipments, value);
        }

        public Equipment? SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                if (SetProperty(ref _selectedEquipment, value))
                {
                    if (value != null)
                    {
                        _editId = value.Id;
                        _editName = value.Name;
                        _editCategory = value.Category;
                        _editCanOrderMultiple = value.CanOrderMultiple;
                        _editHourlyCost = value.HourlyCost;
                        _editRequiresOperator = value.RequiresOperator;
                        _editDescription = value.Description;
                        _editIsActive = value.IsActive;
                        _isEditing = true;
                    }
                    else
                    {
                        ResetEditFields();
                    }
                    OnPropertyChanged(nameof(EditId));
                    OnPropertyChanged(nameof(EditName));
                    OnPropertyChanged(nameof(EditCategory));
                    OnPropertyChanged(nameof(EditCanOrderMultiple));
                    OnPropertyChanged(nameof(EditHourlyCost));
                    OnPropertyChanged(nameof(EditRequiresOperator));
                    OnPropertyChanged(nameof(EditDescription));
                    OnPropertyChanged(nameof(EditIsActive));
                    ((RelayCommand)SaveEquipmentCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteEquipmentCommand).RaiseCanExecuteChanged();
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
                    ((RelayCommand)SaveEquipmentCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditName
        {
            get => _editName;
            set => SetProperty(ref _editName, value);
        }

        public string? EditCategory
        {
            get => _editCategory;
            set => SetProperty(ref _editCategory, value);
        }

        public bool EditCanOrderMultiple
        {
            get => _editCanOrderMultiple;
            set => SetProperty(ref _editCanOrderMultiple, value);
        }

        public decimal? EditHourlyCost
        {
            get => _editHourlyCost;
            set => SetProperty(ref _editHourlyCost, value);
        }

        public bool EditRequiresOperator
        {
            get => _editRequiresOperator;
            set => SetProperty(ref _editRequiresOperator, value);
        }

        public string? EditDescription
        {
            get => _editDescription;
            set => SetProperty(ref _editDescription, value);
        }

        public bool EditIsActive
        {
            get => _editIsActive;
            set => SetProperty(ref _editIsActive, value);
        }

        public ICommand LoadEquipmentsCommand { get; }
        public ICommand SaveEquipmentCommand { get; }
        public ICommand DeleteEquipmentCommand { get; }
        public ICommand CancelEditCommand { get; }

        private async Task LoadEquipmentsAsync()
        {
            try
            {
                // Загружаем *все* записи, включая неактивные, если у пользователя есть право на запись
                // или если нужно показать все для выбора.
                // Для отображения в основном списке можно фильтровать.
                var dbEquipments = await _equipmentRepository.GetAllAsync();
                Equipments.Clear();
                foreach (var eq in dbEquipments)
                {
                    // Фильтруем неактивные при отображении в основном списке, если нужно
                    // В данном случае, оставим все, чтобы пользователь мог редактировать IsActive
                    Equipments.Add(eq);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки техники: {ex.Message}", "Ошибка");
            }
        }

        private bool CanSaveEquipment()
        {
            return _authorizationService.CanWriteTable("Equipments") &&
                   !string.IsNullOrWhiteSpace(EditId) &&
                   !string.IsNullOrWhiteSpace(EditName) &&
                   (!_isEditing || SelectedEquipment?.Id == EditId);
        }

        private async Task SaveEquipmentAsync()
        {
            if (!CanSaveEquipment()) return;

            try
            {
                Equipment equipment;
                bool isNew = !_isEditing;

                if (isNew)
                {
                    if (await _equipmentRepository.ExistsAsync(EditId))
                    {
                        _messageService.ShowErrorMessage($"Техника с ID '{EditId}' уже существует.", "Ошибка");
                        return;
                    }
                    equipment = new Equipment
                    {
                        Id = EditId,
                        Name = EditName,
                        Category = EditCategory,
                        CanOrderMultiple = EditCanOrderMultiple,
                        HourlyCost = EditHourlyCost,
                        RequiresOperator = EditRequiresOperator,
                        Description = EditDescription,
                        IsActive = EditIsActive
                    };
                    await _equipmentRepository.AddAsync(equipment);
                }
                else
                {
                    equipment = SelectedEquipment!;
                    equipment.Name = EditName;
                    equipment.Category = EditCategory;
                    equipment.CanOrderMultiple = EditCanOrderMultiple;
                    equipment.HourlyCost = EditHourlyCost;
                    equipment.RequiresOperator = EditRequiresOperator;
                    equipment.Description = EditDescription;
                    equipment.IsActive = EditIsActive;
                    _equipmentRepository.Update(equipment);
                }

                await _equipmentRepository.SaveChangesAsync();

                if (isNew)
                {
                    // Добавляем в список, даже если IsActive = false, для видимости
                    Equipments.Add(equipment);
                }
                else
                {
                    await LoadEquipmentsAsync(); // Обновление списка, чтобы отразить изменения
                }

                ResetEditFields();
                _messageService.ShowInfoMessage(isNew ? "Техника добавлена." : "Техника обновлена.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка сохранения техники: {ex.Message}", "Ошибка");
            }
        }

        private bool CanDeleteEquipment()
        {
            return _authorizationService.CanWriteTable("Equipments") &&
                   SelectedEquipment != null && SelectedEquipment.Key != 0;
        }

        private async Task DeleteEquipmentAsync()
        {
            if (!CanDeleteEquipment() || SelectedEquipment == null) return;

            if (!_messageService.ShowConfirmationDialog($"Вы действительно хотите удалить технику '{SelectedEquipment.Name}'?")) return;

            try
            {
                _equipmentRepository.Delete(SelectedEquipment);
                await _equipmentRepository.SaveChangesAsync();
                Equipments.Remove(SelectedEquipment);
                ResetEditFields();
                _messageService.ShowInfoMessage("Техника удалена.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка удаления техники: {ex.Message}", "Ошибка");
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
            _editCategory = null;
            _editCanOrderMultiple = false;
            _editHourlyCost = null;
            _editRequiresOperator = false;
            _editDescription = null;
            _editIsActive = true;
            SelectedEquipment = null;
            OnPropertyChanged(nameof(EditId));
            OnPropertyChanged(nameof(EditName));
            OnPropertyChanged(nameof(EditCategory));
            OnPropertyChanged(nameof(EditCanOrderMultiple));
            OnPropertyChanged(nameof(EditHourlyCost));
            OnPropertyChanged(nameof(EditRequiresOperator));
            OnPropertyChanged(nameof(EditDescription));
            OnPropertyChanged(nameof(EditIsActive));
            ((RelayCommand)SaveEquipmentCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteEquipmentCommand).RaiseCanExecuteChanged();
        }
    }
}