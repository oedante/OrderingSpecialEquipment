using OrderingSpecialEquipment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Сервис для валидации заявок на технику (ShiftRequest) перед сохранением.
    /// </summary>
    public interface IShiftRequestValidationService
    {
        /// <summary>
        /// Проверяет зависимости техники для новой или изменённой заявки.
        /// </summary>
        /// <param name="request">Заявка для проверки.</param>
        /// <param name="existingRequests">Существующие заявки на ту же дату и смену (опционально, если известны заранее).</param>
        /// <returns>Список сообщений об ошибках, если зависимости не соблюдены. Пустой список, если всё в порядке.</returns>
        Task<List<string>> ValidateDependenciesAsync(ShiftRequest request, List<ShiftRequest>? existingRequests = null);
    }
}