using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс сервиса заявок на смены
    /// Управляет созданием, обновлением и удалением заявок
    /// </summary>
    public interface IShiftRequestService
    {
        /// <summary>
        /// Находит заявки по дате и смене
        /// </summary>
        Task<IEnumerable<ShiftRequest>> FindRequestsAsync(DateTime date, int shift);

        /// <summary>
        /// Находит заявки по двум датам и сменам
        /// </summary>
        Task<IEnumerable<ShiftRequest>> FindRequestsForTwoShiftsAsync(
            DateTime date1, int shift1,
            DateTime date2, int shift2);

        /// <summary>
        /// Создает новую заявку
        /// </summary>
        Task CreateRequestAsync(ShiftRequest request, User currentUser);

        /// <summary>
        /// Обновляет существующую заявку
        /// </summary>
        Task UpdateRequestAsync(ShiftRequest request, User currentUser);

        /// <summary>
        /// Удаляет заявку
        /// </summary>
        Task DeleteRequestAsync(int key, User currentUser);

        /// <summary>
        /// Обрабатывает зависимости техники при создании/обновлении заявки
        /// </summary>
        Task ProcessDependenciesAsync(ShiftRequest request, User currentUser);

        /// <summary>
        /// Проверяет, заблокирована ли заявка
        /// </summary>
        Task<bool> IsRequestBlockedAsync(int key);
    }
}