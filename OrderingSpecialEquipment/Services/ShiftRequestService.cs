using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IShiftRequestService.
    /// </summary>
    public class ShiftRequestService : IShiftRequestService
    {
        private readonly IShiftRequestRepository _requestRepo;
        private readonly IShiftRequestValidationService _validationService;
        private readonly IDataValidationService _dataValidationService; // Для валидации полей

        public ShiftRequestService(
            IShiftRequestRepository requestRepo,
            IShiftRequestValidationService validationService,
            IDataValidationService dataValidationService)
        {
            _requestRepo = requestRepo;
            _validationService = validationService;
            _dataValidationService = dataValidationService;
        }

        public async Task<bool> CreateRequestAsync(ShiftRequest request, string userId)
        {
            // Валидация на уровне данных
            if (!_dataValidationService.IsValidId(request.EquipmentId) ||
                !string.IsNullOrEmpty(request.LicensePlateId) && !_dataValidationService.IsValidId(request.LicensePlateId) ||
                !_dataValidationService.IsValidId(request.WarehouseId) ||
                !string.IsNullOrEmpty(request.AreaId) && !_dataValidationService.IsValidId(request.AreaId) ||
                !string.IsNullOrEmpty(request.LessorOrganizationId) && !_dataValidationService.IsValidId(request.LessorOrganizationId) ||
                !string.IsNullOrEmpty(request.DepartmentId) && !_dataValidationService.IsValidId(request.DepartmentId))
            {
                // Логировать ошибку валидации
                Console.WriteLine("Ошибка валидации ID в заявке.");
                return false;
            }

            // Проверка зависимостей
            var existingRequests = (await _requestRepo.GetByDateAndShiftAsync(request.Date, request.Shift)).ToList(); // Преобразуем в List
            var validationErrors = await _validationService.ValidateDependenciesAsync(request, existingRequests);

            if (validationErrors.Count > 0)
            {
                // Логировать ошибки валидации зависимостей
                Console.WriteLine($"Ошибки зависимостей: {string.Join(", ", validationErrors)}");
                return false;
            }

            // Установка системных полей
            request.CreatedByUserId = userId;
            request.CreatedAt = DateTime.UtcNow;
            request.IsBlocked = false; // По умолчанию не заблокирована

            try
            {
                await _requestRepo.AddAsync(request);
                await _requestRepo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения заявки: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateRequestAsync(ShiftRequest request)
        {
            // Получаем старую версию заявки для проверки зависимостей
            var oldRequest = await _requestRepo.GetByIdAsync(request.Key);
            if (oldRequest == null)
            {
                Console.WriteLine("Заявка для обновления не найдена.");
                return false;
            }

            // Валидация на уровне данных
            if (!_dataValidationService.IsValidId(request.EquipmentId) ||
                !string.IsNullOrEmpty(request.LicensePlateId) && !_dataValidationService.IsValidId(request.LicensePlateId) ||
                !_dataValidationService.IsValidId(request.WarehouseId) ||
                !string.IsNullOrEmpty(request.AreaId) && !_dataValidationService.IsValidId(request.AreaId) ||
                !string.IsNullOrEmpty(request.LessorOrganizationId) && !_dataValidationService.IsValidId(request.LessorOrganizationId) ||
                !string.IsNullOrEmpty(request.DepartmentId) && !_dataValidationService.IsValidId(request.DepartmentId))
            {
                Console.WriteLine("Ошибка валидации ID в заявке.");
                return false;
            }

            // Проверка зависимостей
            // Нужно получить все заявки на *ту же дату и смену*, кроме *текущей* (request.Key)
            var existingRequests = (await _requestRepo.GetByDateAndShiftAsync(request.Date, request.Shift)).ToList(); // Преобразуем в List
            existingRequests = existingRequests.Where(r => r.Key != request.Key).ToList(); // Используем Where, а не RemoveAll

            // Для проверки зависимостей, нужно учесть, что количество текущей техники изменилось
            // Т.е. если в старой заявке было 2, а в новой 1, то при проверке нужно "добавить обратно" 2 и "вычесть" 1
            // Это делает сервис валидации, передавая ему *новую* заявку и *все старые* (кроме неё самой)
            var validationErrors = await _validationService.ValidateDependenciesAsync(request, existingRequests);

            if (validationErrors.Count > 0)
            {
                Console.WriteLine($"Ошибки зависимостей при обновлении: {string.Join(", ", validationErrors)}");
                return false;
            }

            // Копируем разрешённые к изменению поля
            oldRequest.Date = request.Date;
            oldRequest.Shift = request.Shift;
            oldRequest.EquipmentId = request.EquipmentId;
            oldRequest.LicensePlateId = request.LicensePlateId;
            oldRequest.WarehouseId = request.WarehouseId;
            oldRequest.AreaId = request.AreaId;
            oldRequest.VehicleNumber = request.VehicleNumber;
            oldRequest.VehicleBrand = request.VehicleBrand;
            oldRequest.LessorOrganizationId = request.LessorOrganizationId;
            oldRequest.RequestedCount = request.RequestedCount;
            oldRequest.WorkedHours = request.WorkedHours;
            oldRequest.ActualCost = request.ActualCost;
            oldRequest.IsWorked = request.IsWorked;
            // IsBlocked не изменяется через Update, только через BlockRequest
            oldRequest.Comment = request.Comment;
            // CreatedByUserId и CreatedAt не изменяются
            oldRequest.DepartmentId = request.DepartmentId;
            oldRequest.ProgramYear = request.ProgramYear;
            oldRequest.ProgramMonth = request.ProgramMonth;

            try
            {
                _requestRepo.Update(oldRequest);
                await _requestRepo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления заявки: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> BlockRequestAsync(int key)
        {
            var request = await _requestRepo.GetByIdAsync(key);
            if (request == null || request.IsBlocked)
            {
                Console.WriteLine("Заявка не найдена или уже заблокирована.");
                return false;
            }

            request.IsBlocked = true;
            try
            {
                _requestRepo.Update(request);
                await _requestRepo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка блокировки заявки: {ex.Message}");
                return false;
            }
        }

        // --- ОБНОВЛЁННЫЙ МЕТОД ---
        public async Task<List<ShiftRequest>> FindRequestsAsync(DateTime? date = null, int? shift = null, string? equipmentId = null, string? warehouseId = null, string? departmentId = null)
        {
            // Используем конкретный метод репозитория
            var results = await _requestRepo.FindRequestsAsync(date, shift, equipmentId, warehouseId, departmentId);
            return results.ToList(); // Преобразуем IList в List
        }
    }
    // --- КЛАССЫ ExpressionExtensions и ExpressionReplacer БОЛЬШЕ НЕ НУЖНЫ ---
    // Удаляем их из этого файла.
}