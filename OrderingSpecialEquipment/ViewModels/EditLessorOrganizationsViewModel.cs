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
    /// ViewModel для окна редактирования организаций-арендодателей.
    /// </summary>
    public class EditLessorOrganizationsViewModel : ViewModelBase
    {
        private readonly ILessorOrganizationRepository _lessorOrgRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        private ObservableCollection<LessorOrganization> _lessorOrganizations;
        private LessorOrganization? _selectedLessorOrg;
        private bool _isEditing = false;
        private string _editId = string.Empty;
        private string _editName = string.Empty;
        private string? _editINN;
        private string? _editContactPerson;
        private string? _editPhone;
        private string? _editEmail;
        private string? _editAddress;
        private bool _editIsActive = true;

        public EditLessorOrganizationsViewModel(
            ILessorOrganizationRepository lessorOrgRepository,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _lessorOrgRepository = lessorOrgRepository;
            _messageService = messageService;
            _authorizationService = authorizationService;

            _lessorOrganizations = new ObservableCollection<LessorOrganization>();
            LoadLessorOrganizationsCommand = new RelayCommand(async _ => await LoadLessorOrganizationsAsync(), _ => _authorizationService.CanReadTable("LessorOrganizations"));
            SaveLessorOrganizationCommand = new RelayCommand(async _ => await SaveLessorOrganizationAsync(), _ => CanSaveLessorOrganization());
            DeleteLessorOrganizationCommand = new RelayCommand(async _ => await DeleteLessorOrganizationAsync(), _ => CanDeleteLessorOrganization());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());

            Task.Run(async () => await LoadLessorOrganizationsAsync());
        }

        public ObservableCollection<LessorOrganization> LessorOrganizations
        {
            get => _lessorOrganizations;
            set => SetProperty(ref _lessorOrganizations, value);
        }

        public LessorOrganization? SelectedLessorOrg
        {
            get => _selectedLessorOrg;
            set
            {
                if (SetProperty(ref _selectedLessorOrg, value))
                {
                    if (value != null)
                    {
                        _editId = value.Id;
                        _editName = value.Name;
                        _editINN = value.INN;
                        _editContactPerson = value.ContactPerson;
                        _editPhone = value.Phone;
                        _editEmail = value.Email;
                        _editAddress = value.Address;
                        _editIsActive = value.IsActive;
                        _isEditing = true;
                    }
                    else
                    {
                        ResetEditFields();
                    }
                    OnPropertyChanged(nameof(EditId));
                    OnPropertyChanged(nameof(EditName));
                    OnPropertyChanged(nameof(EditINN));
                    OnPropertyChanged(nameof(EditContactPerson));
                    OnPropertyChanged(nameof(EditPhone));
                    OnPropertyChanged(nameof(EditEmail));
                    OnPropertyChanged(nameof(EditAddress));
                    OnPropertyChanged(nameof(EditIsActive));
                    ((RelayCommand)SaveLessorOrganizationCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteLessorOrganizationCommand).RaiseCanExecuteChanged();
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
                    ((RelayCommand)SaveLessorOrganizationCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string EditName
        {
            get => _editName;
            set => SetProperty(ref _editName, value);
        }

        public string? EditINN
        {
            get => _editINN;
            set => SetProperty(ref _editINN, value);
        }

        public string? EditContactPerson
        {
            get => _editContactPerson;
            set => SetProperty(ref _editContactPerson, value);
        }

        public string? EditPhone
        {
            get => _editPhone;
            set => SetProperty(ref _editPhone, value);
        }

        public string? EditEmail
        {
            get => _editEmail;
            set => SetProperty(ref _editEmail, value);
        }

        public string? EditAddress
        {
            get => _editAddress;
            set => SetProperty(ref _editAddress, value);
        }

        public bool EditIsActive
        {
            get => _editIsActive;
            set => SetProperty(ref _editIsActive, value);
        }

        public ICommand LoadLessorOrganizationsCommand { get; }
        public ICommand SaveLessorOrganizationCommand { get; }
        public ICommand DeleteLessorOrganizationCommand { get; }
        public ICommand CancelEditCommand { get; }

        private async Task LoadLessorOrganizationsAsync()
        {
            try
            {
                var dbOrgs = await _lessorOrgRepository.GetAllAsync();
                LessorOrganizations.Clear();
                foreach (var org in dbOrgs)
                {
                    // Фильтруем неактивные при отображении в основном списке, если нужно
                    // В данном случае, оставим все, чтобы пользователь мог редактировать IsActive
                    LessorOrganizations.Add(org);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка загрузки арендодателей: {ex.Message}", "Ошибка");
            }
        }

        private bool CanSaveLessorOrganization()
        {
            return _authorizationService.CanWriteTable("LessorOrganizations") &&
                   !string.IsNullOrWhiteSpace(EditId) &&
                   !string.IsNullOrWhiteSpace(EditName) &&
                   (!_isEditing || SelectedLessorOrg?.Id == EditId);
        }

        private async Task SaveLessorOrganizationAsync()
        {
            if (!CanSaveLessorOrganization()) return;

            try
            {
                LessorOrganization organization;
                bool isNew = !_isEditing;

                if (isNew)
                {
                    if (await _lessorOrgRepository.ExistsAsync(EditId))
                    {
                        _messageService.ShowErrorMessage($"Организация-арендодатель с ID '{EditId}' уже существует.", "Ошибка");
                        return;
                    }
                    organization = new LessorOrganization
                    {
                        Id = EditId,
                        Name = EditName,
                        INN = EditINN,
                        ContactPerson = EditContactPerson,
                        Phone = EditPhone,
                        Email = EditEmail,
                        Address = EditAddress,
                        IsActive = EditIsActive
                    };
                    await _lessorOrgRepository.AddAsync(organization);
                }
                else
                {
                    organization = SelectedLessorOrg!;
                    organization.Name = EditName;
                    organization.INN = EditINN;
                    organization.ContactPerson = EditContactPerson;
                    organization.Phone = EditPhone;
                    organization.Email = EditEmail;
                    organization.Address = EditAddress;
                    organization.IsActive = EditIsActive;
                    _lessorOrgRepository.Update(organization);
                }

                await _lessorOrgRepository.SaveChangesAsync();

                if (isNew)
                {
                    // Добавляем в список, даже если IsActive = false, для видимости
                    LessorOrganizations.Add(organization);
                }
                else
                {
                    await LoadLessorOrganizationsAsync(); // Обновление списка
                }

                ResetEditFields();
                _messageService.ShowInfoMessage(isNew ? "Арендодатель добавлен." : "Арендодатель обновлен.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка сохранения арендодателя: {ex.Message}", "Ошибка");
            }
        }

        private bool CanDeleteLessorOrganization()
        {
            return _authorizationService.CanWriteTable("LessorOrganizations") &&
                   SelectedLessorOrg != null && SelectedLessorOrg.Key != 0;
        }

        private async Task DeleteLessorOrganizationAsync()
        {
            if (!CanDeleteLessorOrganization() || SelectedLessorOrg == null) return;

            if (!_messageService.ShowConfirmationDialog($"Вы действительно хотите удалить арендодателя '{SelectedLessorOrg.Name}'?")) return;

            try
            {
                _lessorOrgRepository.Delete(SelectedLessorOrg);
                await _lessorOrgRepository.SaveChangesAsync();
                LessorOrganizations.Remove(SelectedLessorOrg);
                ResetEditFields();
                _messageService.ShowInfoMessage("Арендодатель удален.", "Успех");
            }
            catch (Exception ex)
            {
                _messageService.ShowErrorMessage($"Ошибка удаления арендодателя: {ex.Message}", "Ошибка");
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
            _editINN = null;
            _editContactPerson = null;
            _editPhone = null;
            _editEmail = null;
            _editAddress = null;
            _editIsActive = true;
            SelectedLessorOrg = null;
            OnPropertyChanged(nameof(EditId));
            OnPropertyChanged(nameof(EditName));
            OnPropertyChanged(nameof(EditINN));
            OnPropertyChanged(nameof(EditContactPerson));
            OnPropertyChanged(nameof(EditPhone));
            OnPropertyChanged(nameof(EditEmail));
            OnPropertyChanged(nameof(EditAddress));
            OnPropertyChanged(nameof(EditIsActive));
            ((RelayCommand)SaveLessorOrganizationCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteLessorOrganizationCommand).RaiseCanExecuteChanged();
        }
    }
}