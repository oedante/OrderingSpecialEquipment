using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация сервиса заявок на смены
    /// Управляет созданием, обновлением и удалением заявок
    /// Создает новый сервис скоуп для каждой операции
    /// </summary>
    public class ShiftRequestService : IShiftRequestService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEquipmentDependencyService _dependencyService;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public ShiftRequestService(IServiceProvider serviceProvider, IEquipmentDependencyService dependencyService)
        {
            _serviceProvider = serviceProvider;
            _dependencyService = dependencyService;
        }

        /// <summary>
        /// Находит заявки по дате и смене
        /// </summary>
        public async Task<IEnumerable<ShiftRequest>> FindRequestsAsync(DateTime date, int shift)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IShiftRequestRepository>();
                    return await repo.GetByDateAndShiftAsync(date, shift);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске заявок: {ex.Message}");
                return new List<ShiftRequest>();
            }
        }

        /// <summary>
        /// Находит заявки по двум датам и сменам
        /// </summary>
        public async Task<IEnumerable<ShiftRequest>> FindRequestsForTwoShiftsAsync(
            DateTime date1, int shift1,
            DateTime date2, int shift2)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IShiftRequestRepository>();
                    return await repo.GetByDatesAndShiftsAsync(date1, shift1, date2, shift2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске заявок для двух смен: {ex.Message}");
                return new List<ShiftRequest>();
            }
        }

        /// <summary>
        /// Создает новую заявку
        /// </summary>
        public async Task CreateRequestAsync(ShiftRequest request, User currentUser)
        {
            if (request == null || currentUser == null)
                throw new ArgumentNullException("Некорректные параметры запроса");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IShiftRequestRepository>();
                    var equipmentRepo = scope.ServiceProvider.GetRequiredService<IEquipmentRepository>();

                    // Проверяем, заблокирована ли техника для этой даты и смены
                    var existingRequests = await FindRequestsAsync(request.Date, request.Shift);
                    var conflictingRequest = existingRequests.FirstOrDefault(r =>
                        r.EquipmentId == request.EquipmentId &&
                        r.WarehouseId == request.WarehouseId &&
                        r.IsBlocked);

                    if (conflictingRequest != null)
                    {
                        throw new InvalidOperationException($"Заявка на эту технику уже заблокирована для {request.Date.ToShortDateString()}, смена {request.Shift}");
                    }

                    // Получаем технику для проверки CanOrderMultiple
                    var equipment = await equipmentRepo.GetByIdAsync(request.EquipmentId);
                    if (equipment != null && !equipment.CanOrderMultiple)
                    {
                        // Для техники, которую нельзя заказывать несколько раз, проверяем существование
                        var existingSameEquipment = existingRequests.FirstOrDefault(r =>
                            r.EquipmentId == request.EquipmentId &&
                            r.WarehouseId == request.WarehouseId &&
                            !r.IsBlocked);

                        if (existingSameEquipment != null)
                        {
                            throw new InvalidOperationException($"Техника '{equipment.Name}' уже заказана на эту дату и смену. Создайте отдельную заявку.");
                        }
                    }

                    // Устанавливаем пользователя-создателя
                    request.CreatedByUserId = currentUser.Id;
                    request.CreatedAt = DateTime.UtcNow;

                    // Сохраняем заявку
                    await repo.AddAsync(request);

                    // Обрабатываем зависимости
                    await _dependencyService.ProcessDependenciesAsync(request, currentUser);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании заявки: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Обновляет существующую заявку
        /// </summary>
        public async Task UpdateRequestAsync(ShiftRequest request, User currentUser)
        {
            if (request == null || currentUser == null)
                throw new ArgumentNullException("Некорректные параметры запроса");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IShiftRequestRepository>();

                    // Проверяем, заблокирована ли заявка
                    if (request.IsBlocked)
                    {
                        throw new InvalidOperationException("Заблокированную заявку нельзя редактировать");
                    }

                    // Обновляем время изменения
                    request.CreatedAt = DateTime.UtcNow;

                    // Обновляем заявку
                    repo.Update(request);

                    // Обрабатываем зависимости
                    await _dependencyService.ProcessDependenciesAsync(request, currentUser);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении заявки: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Удаляет заявку
        /// </summary>
        public async Task DeleteRequestAsync(int key, User currentUser)
        {
            if (currentUser == null)
                throw new ArgumentNullException(nameof(currentUser));

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IShiftRequestRepository>();

                    // Получаем заявку
                    var request = await repo.GetByIdAsync(key);
                    if (request == null)
                    {
                        throw new InvalidOperationException("Заявка не найдена");
                    }

                    // Проверяем, заблокирована ли заявка
                    if (request.IsBlocked)
                    {
                        throw new InvalidOperationException("Заблокированную заявку нельзя удалить");
                    }

                    // Удаляем заявку
                    await repo.RemoveAsync(request);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении заявки: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Обрабатывает зависимости техники при создании/обновлении заявки
        /// </summary>
        public async Task ProcessDependenciesAsync(ShiftRequest request, User currentUser)
        {
            await _dependencyService.ProcessDependenciesAsync(request, currentUser);
        }

        /// <summary>
        /// Проверяет, заблокирована ли заявка
        /// </summary>
        public async Task<bool> IsRequestBlockedAsync(int key)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IShiftRequestRepository>();
                    var request = await repo.GetByIdAsync(key);
                    return request?.IsBlocked == true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке блокировки заявки: {ex.Message}");
                return false;
            }
        }
    }
}