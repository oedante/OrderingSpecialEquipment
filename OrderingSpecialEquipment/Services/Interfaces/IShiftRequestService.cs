using OrderingSpecialEquipment.Models;
using System.Linq.Expressions;

namespace OrderingSpecialEquipment.Services.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с заявками
    /// </summary>
    public interface IShiftRequestService : IDataService<ShiftRequest>
    {
        /// <summary>
        /// Получение заявок по дате и смене
        /// </summary>
        Task<List<ShiftRequest>> GetByDateAndShiftAsync(DateTime date, int shift);

        /// <summary>
        /// Получение заявок с пагинацией
        /// </summary>
        Task<List<ShiftRequest>> GetPagedAsync(int page, int pageSize,
            Expression<Func<ShiftRequest, bool>>? predicate = null,
            Func<IQueryable<ShiftRequest>, IOrderedQueryable<ShiftRequest>>? orderBy = null);

        /// <summary>
        /// Получение количества часов по транспортной программе для техники
        /// </summary>
        Task<decimal> GetTransportProgramHoursAsync(string departmentId, string equipmentId, int year, int month);

        /// <summary>
        /// Блокировка заявки для редактирования
        /// </summary>
        Task<bool> LockRequestAsync(int requestKey, string userId);

        /// <summary>
        /// Разблокировка заявки
        /// </summary>
        Task<bool> UnlockRequestAsync(int requestKey);

        /// <summary>
        /// Очистка устаревших блокировок
        /// </summary>
        Task CleanupExpiredLocksAsync();
    }
}