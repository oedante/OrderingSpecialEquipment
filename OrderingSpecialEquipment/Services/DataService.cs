using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Базовый сервис для работы с данными (CRUD операции)
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    public class DataService<T> : IDataService<T> where T : class
    {
        #region Поля

        protected readonly IDbContextFactory _contextFactory;
        protected readonly IAuthorizationService _authorizationService;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор сервиса данных
        /// </summary>
        /// <param name="contextFactory">Фабрика контекстов БД</param>
        /// <param name="authorizationService">Сервис авторизации</param>
        public DataService(IDbContextFactory contextFactory, IAuthorizationService authorizationService)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Получение всех записей
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().ToListAsync();
        }

        /// <summary>
        /// Получение записей с фильтром
        /// </summary>
        public virtual async Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Получение записи по ID
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(object id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().FindAsync(id);
        }

        /// <summary>
        /// Добавление записи
        /// </summary>
        public virtual async Task<T> AddAsync(T entity)
        {
            using var context = _contextFactory.CreateDbContext();
            var entry = await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        /// <summary>
        /// Добавление нескольких записей
        /// </summary>
        public virtual async Task<List<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            using var context = _contextFactory.CreateDbContext();
            var list = entities.ToList();
            await context.Set<T>().AddRangeAsync(list);
            await context.SaveChangesAsync();
            return list;
        }

        /// <summary>
        /// Обновление записи
        /// </summary>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            using var context = _contextFactory.CreateDbContext();
            var entry = context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        /// <summary>
        /// Удаление записи по ID
        /// </summary>
        public virtual async Task<bool> DeleteAsync(object id)
        {
            using var context = _contextFactory.CreateDbContext();
            var entity = await context.Set<T>().FindAsync(id);
            if (entity == null)
                return false;

            context.Set<T>().Remove(entity);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Удаление записи
        /// </summary>
        public virtual async Task<bool> DeleteAsync(T entity)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Set<T>().Remove(entity);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Удаление нескольких записей
        /// </summary>
        public virtual async Task<bool> DeleteRangeAsync(IEnumerable<T> entities)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Set<T>().RemoveRange(entities);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Проверка существования записи
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().AnyAsync(predicate);
        }

        /// <summary>
        /// Количество записей
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            using var context = _contextFactory.CreateDbContext();
            if (predicate == null)
                return await context.Set<T>().CountAsync();

            return await context.Set<T>().CountAsync(predicate);
        }

        #endregion
    }
}