using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IShiftRequestValidationService.
    /// Проверяет зависимости техники перед сохранением заявки.
    /// </summary>
    public class ShiftRequestValidationService : IShiftRequestValidationService
    {
        private readonly IShiftRequestRepository _shiftRequestRepo;
        private readonly IEquipmentDependencyRepository _dependencyRepo;

        public ShiftRequestValidationService(
            IShiftRequestRepository shiftRequestRepo,
            IEquipmentDependencyRepository dependencyRepo)
        {
            _shiftRequestRepo = shiftRequestRepo;
            _dependencyRepo = dependencyRepo;
        }

        public async Task<List<string>> ValidateDependenciesAsync(ShiftRequest request, List<ShiftRequest>? existingRequests = null)
        {
            var errors = new List<string>();

            // Получаем зависимости для основной техники в заявке
            var dependencies = await _dependencyRepo.GetByMainEquipmentAsync(request.EquipmentId);
            if (!dependencies.Any()) return errors; // Нет зависимостей - всё в порядке

            // Получаем существующие заявки на ту же дату и смену, если не переданы
            if (existingRequests == null)
            {
                existingRequests = new List<ShiftRequest>();
                // Загружаем заявки на ту же дату и смену, кроме текущей (если редактируется)
                // Предполагаем, что Key != 0 для существующих записей
                if (request.Key != 0)
                {
                    var allRequests = (await _shiftRequestRepo.GetByDateAndShiftAsync(request.Date, request.Shift)).ToList(); // Преобразуем в List
                    existingRequests = allRequests.Where(r => r.Key != request.Key).ToList(); // Преобразуем в List
                }
                else
                {
                    // Для новой заявки
                    var fetchedRequests = await _shiftRequestRepo.GetByDateAndShiftAsync(request.Date, request.Shift);
                    existingRequests = fetchedRequests.ToList(); // Преобразуем в List
                }
            }

            // Группируем существующие заявки по ID техники для подсчёта
            var equipmentCounts = existingRequests
                                    .GroupBy(r => r.EquipmentId)
                                    .ToDictionary(g => g.Key, g => g.Sum(r => r.RequestedCount));

            // Добавляем текущую заявку к подсчёту, если она изменяется
            if (request.Key != 0)
            {
                if (equipmentCounts.ContainsKey(request.EquipmentId))
                {
                    equipmentCounts[request.EquipmentId] += request.RequestedCount;
                }
                else
                {
                    equipmentCounts[request.EquipmentId] = request.RequestedCount;
                }
            }

            // Проверяем зависимости
            foreach (var dep in dependencies)
            {
                int requestedCountForDependent = equipmentCounts.ContainsKey(dep.DependentEquipmentId) ? equipmentCounts[dep.DependentEquipmentId] : 0;

                if (dep.IsMandatory && requestedCountForDependent < dep.RequiredCount)
                {
                    // Техника в заявке требует, чтобы была заказана определённая зависимая техника в нужном количестве
                    int needed = dep.RequiredCount - requestedCountForDependent;
                    errors.Add($"Для техники '{request.EquipmentId}' требуется минимум {dep.RequiredCount} единиц техники '{dep.DependentEquipmentId}'. Заказано: {requestedCountForDependent}. Не хватает: {needed}.");
                }
            }

            return errors;
        }
    }
}