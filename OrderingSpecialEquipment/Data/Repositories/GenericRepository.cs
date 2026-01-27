using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    /// <summary>
    /// Обобщенный репозиторий, предоставляющий базовые CRUD-операции.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    public abstract class GenericRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext Context;

        protected GenericRepository(ApplicationDbContext context)
        {
            Context = context;
        }

        public virtual async Task<IList<TEntity>> GetAllAsync()
        {
            return await Context.Set<TEntity>().ToListAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(object id)
        {
            return await Context.Set<TEntity>().FindAsync(id);
        }

        /// <summary>
        /// Метод для поиска сущности по строковому идентификатору "Id".
        /// Может быть переопределен в наследниках, если у сущности нет поля Id типа string.
        /// </summary>
        /// <param name="id">Значение поля Id.</param>
        /// <returns>Сущность или null, если не найдена.</returns>
        public virtual async Task<TEntity?> GetByIdStringAsync(string id)
        {
            // По умолчанию возвращает null, если сущность не имеет Id типа string.
            // В наследниках нужно переопределить с конкретным свойством.
            return null;
        }

        public virtual async Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Context.Set<TEntity>().Where(predicate).ToListAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await Context.Set<TEntity>().AddAsync(entity);
        }

        public virtual void Update(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }
    }
}