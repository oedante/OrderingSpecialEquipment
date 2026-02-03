using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация сервиса зависимостей техники
    /// Автоматически создает заявки на зависимую технику при создании основной заявки
    /// </summary>
    public class EquipmentDependencyService : IEquipmentDependencyService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public EquipmentDependencyService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Обрабатывает зависимости техники при создании/обновлении заявки
        /// Автоматически создает заявки на зависимую технику
        /// </summary>
        public async Task ProcessDependenciesAsync(ShiftRequest mainRequest, User currentUser)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dependencyRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<EquipmentDependency>>();
                    var shiftRequestRepo = scope.ServiceProvider.GetRequiredService<IShiftRequestRepository>();

                    // Получаем все зависимости для основной техники
                    var dependencies = await dependencyRepo.FindAsync(d =>
                        d.MainEquipmentId == mainRequest.EquipmentId && d.IsMandatory);

                    foreach (var dependency in dependencies)
                    {
                        // Проверяем, существует ли уже заявка на зависимую технику
                        var existingRequests = await shiftRequestRepo.GetByDateAndShiftAsync(
                            mainRequest.Date, mainRequest.Shift);

                        var existingDependency = existingRequests.FirstOrDefault(r =>
                            r.EquipmentId == dependency.DependentEquipmentId &&
                            r.WarehouseId == mainRequest.WarehouseId);

                        if (existingDependency == null)
                        {
                            // Создаем новую заявку на зависимую технику
                            var dependentRequest = new ShiftRequest
                            {
                                Date = mainRequest.Date,
                                Shift = mainRequest.Shift,
                                EquipmentId = dependency.DependentEquipmentId,
                                WarehouseId = mainRequest.WarehouseId,
                                AreaId = mainRequest.AreaId,
                                DepartmentId = mainRequest.DepartmentId,
                                RequestedCount = dependency.RequiredCount,
                                CreatedByUserId = currentUser.Id,
                                CreatedAt = DateTime.UtcNow,
                                Comment = $"Автоматически создана как зависимость для {mainRequest.Equipment?.Name}"
                            };

                            await shiftRequestRepo.AddAsync(dependentRequest);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке зависимостей техники: {ex.Message}");
                // Не выбрасываем исключение, чтобы не прервать основную операцию
            }
        }
    }
}