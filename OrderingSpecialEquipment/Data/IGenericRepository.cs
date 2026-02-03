using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Обобщенный интерфейс репозитория для работы с сущностями
    /// Предоставляет базовые CRUD операции
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности</typeparam>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Получает все сущности
        /// </summary>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Получает сущность по идентификатору
        /// </summary>
        Task<TEntity?> GetByIdAsync(object id);

        /// <summary>
        /// Добавляет новую сущность
        /// </summary>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Обновляет существующую сущность
        /// </summary>
        void Update(TEntity entity);

        /// <summary>
        /// Удаляет сущность
        /// </summary>
        Task RemoveAsync(TEntity entity);

        /// <summary>
        /// Находит сущности по условию
        /// </summary>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Проверяет существование сущности по условию
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    }
}