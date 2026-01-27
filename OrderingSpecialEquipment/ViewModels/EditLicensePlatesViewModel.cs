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
    /// ViewModel для окна редактирования государственных номеров техники.
    /// </summary>
    public class EditLicensePlatesViewModel : ViewModelBase
    {
        private readonly ILicensePlateRepository _licensePlateRepository;
        private readonly IEquipmentRepositoryBase _equipmentRepository;
        private readonly ILessorOrganizationRepository _lessorOrgRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        private ObservableCollection<LicensePlate> _licensePlates;
        private LicensePlate? _selectedLicensePlate;
        private bool _isEditing = false;
        private string _editId = string.Empty;
        private string _editPlateNumber = string.Empty;
        private string _editEquipmentId = string.Empty; // Привязка к технике
        private string _editLessorOrganizationId = string.Empty; // Привязка к арендодателю
        private string? _editBrand;
        private int? _editYear;
        private string? _editCapacity;
        private string? _editVIN;
        private bool _editIsActive = true;

        // Для выбора в ComboBox
        private ObservableCollection<Equipment> _equipmentsForSelection;
        private ObservableCollection<LessorOrganization> _lessorOrgsForSelection;

        public EditLicensePlatesViewModel(
            ILicensePlateRepository licensePlateRepository,
            IEquipmentRepositoryBase equipmentRepository,
            ILessorOrganizationRepository lessorOrgRepository,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _licensePlateRepository = licensePlateRepository;
            _equipmentRepository = equipmentRepository;
            _lessorOrgRepository = lessorOrgRepository;
            _messageService = messageService;
            _authorizationService = authorizationService;

            _licensePlates = new ObservableCollection<LicensePlate>();
            _equipmentsForSelection = new ObservableCollection<Equipment>();
            _lessorOrgsForSelection = new ObservableCollection<LessorOrganization>();

            LoadLicensePlatesCommand = new RelayCommand(async _ => await LoadLicensePlatesAsync(), _ => _authorizationService.CanReadTable("LicensePlates"));
            SaveLicensePlateCommand = new RelayCommand(async _ => await SaveLicensePlateAsync(), _ => CanSaveLicensePlate());
            DeleteLicensePlateCommand = new RelayCommand(async _ => await DeleteLicensePlateAsync(), _ => CanDeleteLicensePlate());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());

            // Загрузка зависимых данных
            Task.Run(async () =>
            {
                await LoadEquipmentsForSelectionAsync();
                await LoadLessorOrgsForSelectionAsync();
            });

            Task.Run(async () => await LoadLicensePlatesAsync());
        }

        public ObservableCollection<LicensePlate> LicensePlates
        {
            get => _licensePlates;
            set => SetProperty(ref _licensePlates, value);
        }

        public LicensePlate? SelectedLicensePlate
        {
            get => _selectedLicensePlate;
            set
            {
                if (SetProperty(ref _selectedLicensePlate, value))
                {
                    if (value != null)
                    {
                        _editId = value.Id;
                        _editPlateNumber = value.PlateNumber;
                        _editEquipmentId = value.EquipmentId;
                        _editLessorOrganizationId = value.LessorOrganizationId;
                        _editBrand = value.Brand;
                        _editYear = value.Year;
                        _editCapacity = value.Capacity;
                        _editVIN = value.VIN;
                        _editIsActive = value.IsActive;
                        _isEditing = true;
                    }
                    else
                    {
                        ResetEditFields();
                    }
                    OnPropertyChanged(nameof(EditId));
                    OnPropertyChanged(nameof(EditPlateNumber));
                    OnPropertyChanged(nameof(EditEquipmentId));
                    OnPropertyChanged(nameof(EditLessorOrganizationId));
                    OnPropertyChanged(nameof(EditBrand));
                    OnPropertyChanged(nameof(EditYear));
                    OnPropertyChanged(nameof(EditCapacity));
                    OnPropertyChanged(nameof(EditVIN));
                    OnPropertyChanged(nameof(EditIsActive));
                    ((RelayCommand)SaveLicensePlateCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteLicensePlateCommand).RaiseCanExecuteChanged();
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
                    ((RelayCommand)SaveLicensePlateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditPlateNumber
        {
            get => _editPlateNumber;
            set => SetProperty(ref _editPlateNumber, value);
        }

        public string EditEquipmentId
        {
            get => _editEquipmentId;
            set => SetProperty(ref _editEquipmentId, value);
        }

        public string EditLessorOrganizationId
        {
            get => _editLessorOrganizationId;
            set => SetProperty(ref _editLessorOrganizationId, value);
        }

        public string? EditBrand
        {
            get => _editBrand;
            set => SetProperty(ref _editBrand, value);
        }

        public int? EditYear
        {
            get => _editYear;
            set => SetProperty(ref _editYear, value);
        }

        public string? EditCapacity
        {
            get => _editCapacity;
            set => SetProperty(ref _editCapacity, value);
        }

        public string? EditVIN
        {
            get => _editVIN;
            set => SetProperty(ref _editVIN, value);
        }

        public bool EditIsActive
        {
            get => _editIsActive;
            set => SetProperty(ref _editIsActive, value);
        }

        // --- Свойства для выбора ---
        public ObservableCollection<Equipment> EquipmentsForSelection
        {
            get => _equipmentsForSelection;
            set => SetProperty(ref _equipmentsForSelection, value);
        }

        public ObservableCollection<LessorOrganization> LessorOrgsForSelection
        {
            get => _lessorOrgsForSelection;
            set => SetProperty(ref _lessorOrgsForSelection, value);
        }

        public ICommand LoadLicensePlatesCommand { get; }
        public ICommand SaveLicensePlateCommand { get; }
        public ICommand DeleteLicensePlateCommand { get; }
        public ICommand CancelEditCommand { get; }

        private async Task LoadLicensePlatesAsync()
        {
            try
            {
                var dbPlates = await _licensePlateRepository.GetAllAsync();
                LicensePlates.Clear();
                foreach (var plate in dbPlates)
                {
                    // Фильтруем неактивные при отображении в основном списке, если нужно
                    // В данном случае, оставим все, чтобы пользователь мог редактировать IsActive
                    LicensePlates.Add(plate);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки госномеров: {ex.Message}", "Ошибка");
            }
        }

        private bool CanSaveLicensePlate()
        {
            return _authorizationService.CanWriteTable("LicensePlates") &&
                   !string.IsNullOrWhiteSpace(EditId) &&
                   !string.IsNullOrWhiteSpace(EditPlateNumber) &&
                   !string.IsNullOrWhiteSpace(EditEquipmentId) &&
                   !string.IsNullOrWhiteSpace(EditLessorOrganizationId) &&
                   (!_isEditing || SelectedLicensePlate?.Id == EditId);
        }

        private async Task SaveLicensePlateAsync()
        {
            if (!CanSaveLicensePlate()) return;

            try
            {
                LicensePlate plate;
                bool isNew = !_isEditing;

                if (isNew)
                {
                    if (await _licensePlateRepository.ExistsAsync(EditId))
                    {
                        _messageService.ShowErrorMessage($"Госномер с ID '{EditId}' уже существует.", "Ошибка");
                        return;
                    }
                    plate = new LicensePlate
                    {
                        Id = EditId,
                        PlateNumber = EditPlateNumber,
                        EquipmentId = EditEquipmentId,
                        LessorOrganizationId = EditLessorOrganizationId,
                        Brand = EditBrand,
                        Year = EditYear,
                        Capacity = EditCapacity,
                        VIN = EditVIN,
                        IsActive = EditIsActive
                    };
                    await _licensePlateRepository.AddAsync(plate);
                }
                else
                {
                    plate = SelectedLicensePlate!;
                    plate.PlateNumber = EditPlateNumber;
                    plate.EquipmentId = EditEquipmentId;
                    plate.LessorOrganizationId = EditLessorOrganizationId;
                    plate.Brand = EditBrand;
                    plate.Year = EditYear;
                    plate.Capacity = EditCapacity;
                    plate.VIN = EditVIN;
                    plate.IsActive = EditIsActive;
                    _licensePlateRepository.Update(plate);
                }

                await _licensePlateRepository.SaveChangesAsync();

                if (isNew)
                {
                    // Добавляем в список, даже если IsActive = false, для видимости
                    LicensePlates.Add(plate);
                }
                else
                {
                    await LoadLicensePlatesAsync(); // Обновление списка
                }

                ResetEditFields();
                _messageService.ShowInfoMessage(isNew ? "Госномер добавлен." : "Госномер обновлен.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка сохранения госномера: {ex.Message}", "Ошибка");
            }
        }

        private bool CanDeleteLicensePlate()
        {
            return _authorizationService.CanWriteTable("LicensePlates") &&
                   SelectedLicensePlate != null && SelectedLicensePlate.Key != 0;
        }

        private async Task DeleteLicensePlateAsync()
        {
            if (!CanDeleteLicensePlate() || SelectedLicensePlate == null) return;

            if (!_messageService.ShowConfirmationDialog($"Вы действительно хотите удалить госномер '{SelectedLicensePlate.PlateNumber}'?")) return;

            try
            {
                _licensePlateRepository.Delete(SelectedLicensePlate);
                await _licensePlateRepository.SaveChangesAsync();
                LicensePlates.Remove(SelectedLicensePlate);
                ResetEditFields();
                _messageService.ShowInfoMessage("Госномер удален.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка удаления госномера: {ex.Message}", "Ошибка");
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
            _editPlateNumber = string.Empty;
            _editEquipmentId = string.Empty; // Сбросить на значение по умолчанию или оставить как есть?
            _editLessorOrganizationId = string.Empty; // Сбросить на значение по умолчанию или оставить как есть?
            _editBrand = null;
            _editYear = null;
            _editCapacity = null;
            _editVIN = null;
            _editIsActive = true;
            SelectedLicensePlate = null;
            OnPropertyChanged(nameof(EditId));
            OnPropertyChanged(nameof(EditPlateNumber));
            OnPropertyChanged(nameof(EditEquipmentId));
            OnPropertyChanged(nameof(EditLessorOrganizationId));
            OnPropertyChanged(nameof(EditBrand));
            OnPropertyChanged(nameof(EditYear));
            OnPropertyChanged(nameof(EditCapacity));
            OnPropertyChanged(nameof(EditVIN));
            OnPropertyChanged(nameof(EditIsActive));
            ((RelayCommand)SaveLicensePlateCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteLicensePlateCommand).RaiseCanExecuteChanged();
        }

        // --- Вспомогательные методы ---
        private async Task LoadEquipmentsForSelectionAsync()
        {
            try
            {
                var dbEq = await _equipmentRepository.GetAllAsync();
                EquipmentsForSelection.Clear();
                foreach (var eq in dbEq)
                {
                    // Фильтрация по IsActive
                    if (eq.IsActive) // <-- Добавлено
                    {
                        EquipmentsForSelection.Add(eq);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки техники для выбора: {ex.Message}", "Ошибка");
            }
        }

        private async Task LoadLessorOrgsForSelectionAsync()
        {
            try
            {
                var dbOrgs = await _lessorOrgRepository.GetAllAsync();
                LessorOrgsForSelection.Clear();
                foreach (var org in dbOrgs)
                {
                    // Фильтрация по IsActive
                    if (org.IsActive) // <-- Добавлено
                    {
                        LessorOrgsForSelection.Add(org);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки арендодателей для выбора: {ex.Message}", "Ошибка");
            }
        }
    }
}