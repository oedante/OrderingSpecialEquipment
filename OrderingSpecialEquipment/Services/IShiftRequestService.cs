using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Сервис для бизнес-логики, связанной с заявками на технику (ShiftRequest).
    /// </summary>
    public interface IShiftRequestService
    {
        /// <summary>
        /// Создаёт новую заявку, предварительно проверив зависимости.
        /// </summary>
        /// <param name="request">Новая заявка.</param>
        /// <param name="userId">ID пользователя, создавшего заявку.</param>
        /// <returns>True, если заявка создана успешно.</returns>
        Task<bool> CreateRequestAsync(ShiftRequest request, string userId);

        /// <summary>
        /// Обновляет существующую заявку, предварительно проверив зависимости.
        /// </summary>
        /// <param name="request">Заявка с новыми данными. Поле Key должно быть заполнено.</param>
        /// <returns>True, если заявка обновлена успешно.</returns>
        Task<bool> UpdateRequestAsync(ShiftRequest request);

        /// <summary>
        /// Блокирует заявку (устанавливает IsBlocked = true).
        /// </summary>
        /// <param name="key">Ключ заявки.</param>
        /// <returns>True, если успешно заблокирована.</returns>
        Task<bool> BlockRequestAsync(int key);

        /// <summary>
        /// Находит заявки по критериям.
        /// </summary>
        /// <param name="date">Дата (необязательно).</param>
        /// <param name="shift">Смена (необязательно).</param>
        /// <param name="equipmentId">ID техники (необязательно).</param>
        /// <param name="warehouseId">ID склада (необязательно).</param>
        /// <param name="departmentId">ID отдела (необязательно).</param>
        /// <returns>Список найденных заявок.</returns>
        Task<List<ShiftRequest>> FindRequestsAsync(DateTime? date = null, int? shift = null, string? equipmentId = null, string? warehouseId = null, string? departmentId = null);

        // ... другие методы ...
    }
}