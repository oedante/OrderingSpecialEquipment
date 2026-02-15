using System.Linq.Expressions;

namespace OrderingSpecialEquipment.Services.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса работы с данными (CRUD)
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    public interface IDataService<T> where T : class
    {
        /// <summary>
        /// Получение всех записей
        /// </summary>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Получение записей с фильтром
        /// </summary>
        Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Получение записи по ID
        /// </summary>
        Task<T> GetByIdAsync(object id);

        /// <summary>
        /// Добавление записи
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Добавление нескольких записей
        /// </summary>
        Task<List<T>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Обновление записи
        /// </summary>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Удаление записи
        /// </summary>
        Task<bool> DeleteAsync(object id);

        /// <summary>
        /// Удаление записи
        /// </summary>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Удаление нескольких записей
        /// </summary>
        Task<bool> DeleteRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Проверка существования записи
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Количество записей
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);

        /// <summary>
        /// Сохранение изменений
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}