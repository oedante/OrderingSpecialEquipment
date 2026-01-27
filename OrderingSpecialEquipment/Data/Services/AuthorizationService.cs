using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IAuthorizationService.
    /// Проверяет права на основе роли пользователя и таблицы UserDepartmentAccess/UserWarehouseAccess.
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private User? _currentUser;
        private Role? _currentRole; // Теперь это поле доступно
        private IList<string>? _allowedDepartmentIds;
        private IList<string>? _allowedWarehouseIds;

        private readonly IUserDepartmentAccessRepository _userDepartmentAccessRepo;
        private readonly IUserWarehouseAccessRepository _userWarehouseAccessRepo;
        private readonly IRoleRepository _roleRepository; // Добавляем репозиторий ролей

        public AuthorizationService(
            IUserDepartmentAccessRepository userDepartmentAccessRepo,
            IUserWarehouseAccessRepository userWarehouseAccessRepo,
            IRoleRepository roleRepository) // Принимаем IRoleRepository через DI
        {
            _userDepartmentAccessRepo = userDepartmentAccessRepo;
            _userWarehouseAccessRepo = userWarehouseAccessRepo;
            _roleRepository = roleRepository;
        }

        public void InitializeForUser(User user)
        {
            _currentUser = user;
            _currentRole = null; // Сбрасываем роль
            _allowedDepartmentIds = null;
            _allowedWarehouseIds = null;

            if (user != null)
            {
                // Загружаем роль пользователя через репозиторий
                _currentRole = _roleRepository.GetByIdStringAsync(user.RoleId).Result; // Используем Result осторожно

                var deptAccesses = _userDepartmentAccessRepo.GetByUserIdAsync(user.Id).Result; // Используем Result
                _allowedDepartmentIds = deptAccesses.Select(da => da.DepartmentId).ToList();

                var warehouseAccessKeys = deptAccesses.Where(da => !da.HasAllWarehouses).Select(da => da.Key).ToList();
                if (warehouseAccessKeys.Any())
                {
                    var warehouseAccesses = _userWarehouseAccessRepo.GetByUserDepartmentAccessKeysAsync(warehouseAccessKeys).Result; // Используем Result
                    _allowedWarehouseIds = warehouseAccesses.Select(wa => wa.WarehouseId).ToList();
                }
                else
                {
                    if (user.HasAllDepartments || (_currentRole?.SPEC_ManageAllDepartments ?? false))
                    {
                        _allowedWarehouseIds = null;
                    }
                    else if (!_allowedDepartmentIds.Any())
                    {
                        _allowedWarehouseIds = new List<string>();
                    }
                }
            }
        }

        public bool CanReadTable(string tableName)
        {
            if (_currentRole == null) return false;
            var permissionProperty = _currentRole.GetType().GetProperty($"TAB_{tableName}");
            if (permissionProperty != null && permissionProperty.PropertyType == typeof(short))
            {
                var value = (short)permissionProperty.GetValue(_currentRole)!;
                return value >= 1;
            }
            return false;
        }

        public bool CanWriteTable(string tableName)
        {
            if (_currentRole == null) return false;
            var permissionProperty = _currentRole.GetType().GetProperty($"TAB_{tableName}");
            if (permissionProperty != null && permissionProperty.PropertyType == typeof(short))
            {
                var value = (short)permissionProperty.GetValue(_currentRole)!;
                return value >= 2;
            }
            return false;
        }

        public bool HasSpecialPermission(string permissionName)
        {
            if (_currentRole == null) return false;
            var permissionProperty = _currentRole.GetType().GetProperty(permissionName);
            if (permissionProperty != null && permissionProperty.PropertyType == typeof(bool))
            {
                return (bool)permissionProperty.GetValue(_currentRole)!;
            }
            return false;
        }

        public bool CanAccessDepartment(string departmentId)
        {
            if (_currentUser == null) return false;
            if (_currentUser.HasAllDepartments || (_currentRole?.SPEC_ManageAllDepartments ?? false))
                return true;

            return _allowedDepartmentIds?.Contains(departmentId) == true;
        }

        public bool CanAccessWarehouse(string warehouseId)
        {
            if (_currentUser == null) return false;
            if (_allowedWarehouseIds == null) return true;

            return _allowedWarehouseIds.Contains(warehouseId);
        }

        // --- НОВЫЕ МЕТОДЫ ---
        public Role? GetCurrentUserRole()
        {
            return _currentRole;
        }

        public bool IsCurrentUserSystemAdmin()
        {
            return _currentRole?.SPEC_SystemAdmin == true;
        }
    }
}