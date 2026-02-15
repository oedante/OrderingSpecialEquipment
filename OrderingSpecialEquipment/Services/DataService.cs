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

        protected readonly IDatabaseService _databaseService;
        protected readonly IAuthorizationService _authorizationService;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор сервиса данных
        /// </summary>
        /// <param name="databaseService">Сервис БД</param>
        /// <param name="authorizationService">Сервис авторизации</param>
        public DataService(IDatabaseService databaseService, IAuthorizationService authorizationService)
        {
            _databaseService = databaseService;
            _authorizationService = authorizationService;
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Получение всех записей
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _databaseService.Context.Set<T>().ToListAsync();
        }

        /// <summary>
        /// Получение записей с фильтром
        /// </summary>
        public virtual async Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _databaseService.Context.Set<T>().Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Получение записи по ID
        /// </summary>
        public virtual async Task<T> GetByIdAsync(object id)
        {
            return await _databaseService.Context.Set<T>().FindAsync(id);
        }

        /// <summary>
        /// Добавление записи
        /// </summary>
        public virtual async Task<T> AddAsync(T entity)
        {
            var entry = await _databaseService.Context.Set<T>().AddAsync(entity);
            await _databaseService.Context.SaveChangesAsync();
            return entry.Entity;
        }

        /// <summary>
        /// Добавление нескольких записей
        /// </summary>
        public virtual async Task<List<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            var list = entities.ToList();
            await _databaseService.Context.Set<T>().AddRangeAsync(list);
            await _databaseService.Context.SaveChangesAsync();
            return list;
        }

        /// <summary>
        /// Обновление записи
        /// </summary>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            var entry = _databaseService.Context.Set<T>().Update(entity);
            await _databaseService.Context.SaveChangesAsync();
            return entry.Entity;
        }

        /// <summary>
        /// Удаление записи по ID
        /// </summary>
        public virtual async Task<bool> DeleteAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            return await DeleteAsync(entity);
        }

        /// <summary>
        /// Удаление записи
        /// </summary>
        public virtual async Task<bool> DeleteAsync(T entity)
        {
            _databaseService.Context.Set<T>().Remove(entity);
            var result = await _databaseService.Context.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Удаление нескольких записей
        /// </summary>
        public virtual async Task<bool> DeleteRangeAsync(IEnumerable<T> entities)
        {
            _databaseService.Context.Set<T>().RemoveRange(entities);
            var result = await _databaseService.Context.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Проверка существования записи
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _databaseService.Context.Set<T>().AnyAsync(predicate);
        }

        /// <summary>
        /// Количество записей
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await _databaseService.Context.Set<T>().CountAsync();

            return await _databaseService.Context.Set<T>().CountAsync(predicate);
        }

        /// <summary>
        /// Сохранение изменений
        /// </summary>
        public virtual async Task<int> SaveChangesAsync()
        {
            return await _databaseService.Context.SaveChangesAsync();
        }

        #endregion
    }
}