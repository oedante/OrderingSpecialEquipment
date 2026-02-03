using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Обобщенная реализация репозитория
    /// Работает с любым типом сущности через DbContext
    /// DbContext получается через DI (не хранится как поле)
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности</typeparam>
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Получает все сущности
        /// </summary>
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            try
            {
                return await _context.Set<TEntity>().AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при получении всех сущностей {typeof(TEntity).Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получает сущность по идентификатору
        /// </summary>
        public async Task<TEntity?> GetByIdAsync(object id)
        {
            try
            {
                return await _context.Set<TEntity>().FindAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении сущности {typeof(TEntity).Name} с ID {id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Добавляет новую сущность
        /// </summary>
        public async Task AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                await _context.Set<TEntity>().AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении сущности {typeof(TEntity).Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Обновляет существующую сущность
        /// </summary>
        public void Update(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                _context.Set<TEntity>().Update(entity);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении сущности {typeof(TEntity).Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Удаляет сущность
        /// </summary>
        public async Task RemoveAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                _context.Set<TEntity>().Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении сущности {typeof(TEntity).Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Находит сущности по условию
        /// </summary>
        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            try
            {
                return await _context.Set<TEntity>().AsNoTracking().Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске сущностей {typeof(TEntity).Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование сущности по условию
        /// </summary>
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            try
            {
                return await _context.Set<TEntity>().AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке существования сущности {typeof(TEntity).Name}: {ex.Message}");
                throw;
            }
        }
    }
}