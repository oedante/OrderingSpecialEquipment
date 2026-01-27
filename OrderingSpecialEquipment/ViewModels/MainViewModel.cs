using OrderingSpecialEquipment.Commands;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services;
using System;
using System.Windows.Input;

namespace OrderingSpecialEquipment.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        // Убираем IThemeService из полей, так как она используется при инициализации
        // private readonly IThemeService _themeService;

        private ViewModelBase? _currentView;
        private User? _currentUser;
        private string _currentUserInfo = "Пользователь не аутентифицирован";

        // Добавляем сервисы для навигации
        private readonly IServiceProvider _serviceProvider;

        public MainViewModel(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            IThemeService themeService, // Принимаем IThemeService через DI
            IServiceProvider serviceProvider) // Принимаем IServiceProvider для получения других ViewModel
        {
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _serviceProvider = serviceProvider; // Сохраняем для навигации

            InitializeApplication(themeService); // Передаем themeService в инициализацию
        }

        private void InitializeApplication(IThemeService themeService)
        {
            _currentUser = _authenticationService.AuthenticateCurrentUser();
            if (_currentUser != null)
            {
                _currentUserInfo = $"Пользователь: {_currentUser.FullName} ({_currentUser.WindowsLogin})";
                _authorizationService.InitializeForUser(_currentUser);
            }
            else
            {
                _messageService.ShowErrorMessage("Не удалось аутентифицировать пользователя Windows. Приложение будет закрыто.", "Ошибка аутентификации");
                // Application.Current.Shutdown(); // Пример вызова, зависит от реализации
                return;
            }

            var savedTheme = themeService.LoadThemePreference();
            themeService.ApplyTheme(savedTheme);

            // Инициализация команд
            NavigateToEditDepartmentsCommand = new RelayCommand(_ => NavigateToView<EditDepartmentsViewModel>(), _ => _authorizationService.CanReadTable("Departments") || _authorizationService.CanWriteTable("Departments"));
            NavigateToEditEquipmentCommand = new RelayCommand(_ => NavigateToView<EditEquipmentViewModel>(), _ => _authorizationService.CanReadTable("Equipments") || _authorizationService.CanWriteTable("Equipments"));
            NavigateToEditLessorOrganizationsCommand = new RelayCommand(_ => NavigateToView<EditLessorOrganizationsViewModel>(), _ => _authorizationService.CanReadTable("LessorOrganizations") || _authorizationService.CanWriteTable("LessorOrganizations"));
            NavigateToEditLicensePlatesCommand = new RelayCommand(_ => NavigateToView<EditLicensePlatesViewModel>(), _ => _authorizationService.CanReadTable("LicensePlates") || _authorizationService.CanWriteTable("LicensePlates"));
            NavigateToEditRolesCommand = new RelayCommand(_ => NavigateToView<EditRolesViewModel>(), _ => _authorizationService.CanReadTable("Roles") || _authorizationService.CanWriteTable("Roles"));
            NavigateToEditUsersCommand = new RelayCommand(_ => NavigateToView<EditUsersViewModel>(), _ => _authorizationService.CanReadTable("Users") || _authorizationService.CanWriteTable("Users"));
            NavigateToEditWarehousesCommand = new RelayCommand(_ => NavigateToView<EditWarehousesAndAreasViewModel>(), _ => _authorizationService.CanReadTable("Warehouses") || _authorizationService.CanWriteTable("Warehouses"));
            NavigateToEditUserAccessCommand = new RelayCommand(_ => NavigateToView<EditUserAccessViewModel>(), _ => _authorizationService.HasSpecialPermission("SPEC_ManageUsers"));
            NavigateToReportsCommand = new RelayCommand(_ => NavigateToView<ReportsViewModel>(), _ => _authorizationService.HasSpecialPermission("SPEC_ViewReports"));
            NavigateToSettingsCommand = new RelayCommand(_ => NavigateToView<SettingsViewModel>(), _ => _authorizationService.IsCurrentUserSystemAdmin()); // Используем новый метод

            NavigateToView<ReportsViewModel>(); // Или другое начальное представление
        }

        // Свойства
        public ViewModelBase? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string CurrentUserInfo
        {
            get => _currentUserInfo;
            set => SetProperty(ref _currentUserInfo, value);
        }

        // Команды навигации
        public ICommand NavigateToEditDepartmentsCommand { get; set; }
        public ICommand NavigateToEditEquipmentCommand { get; set; }
        public ICommand NavigateToEditLessorOrganizationsCommand { get; set; }
        public ICommand NavigateToEditLicensePlatesCommand { get; set; }
        public ICommand NavigateToEditRolesCommand { get; set; }
        public ICommand NavigateToEditUsersCommand { get; set; }
        public ICommand NavigateToEditWarehousesCommand { get; set; }
        public ICommand NavigateToEditUserAccessCommand { get; set; }
        public ICommand NavigateToReportsCommand { get; set; }
        public ICommand NavigateToSettingsCommand { get; set; }

        // Метод навигации, использующий DI
        private void NavigateToView<T>() where T : ViewModelBase
        {
            var viewModel = _serviceProvider.GetService(typeof(T)) as T;
            if (viewModel != null)
            {
                CurrentView = viewModel;
            }
            else
            {
                _messageService.ShowErrorMessage($"Не удалось создать ViewModel: {typeof(T).Name}", "Ошибка навигации");
            }
        }
    }
}