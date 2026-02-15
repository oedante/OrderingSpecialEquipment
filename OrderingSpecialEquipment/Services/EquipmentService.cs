using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Сервис для работы с техникой
    /// </summary>
    public class EquipmentService : DataService<Equipment>, IEquipmentService
    {
        #region Конструктор

        /// <summary>
        /// Конструктор сервиса техники
        /// </summary>
        public EquipmentService(IDbContextFactory contextFactory, IAuthorizationService authorizationService)
            : base(contextFactory, authorizationService)
        {
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Получение зависимостей для техники
        /// </summary>
        public async Task<List<EquipmentDependency>> GetDependenciesAsync(string equipmentId)
        {
            using var context = _contextFactory.CreateDbContext();

            return await context.EquipmentDependencies
                .Include(ed => ed.DependentEquipment)
                .Where(ed => ed.MainEquipmentId == equipmentId)
                .ToListAsync();
        }

        /// <summary>
        /// Получение техники с учетом избранного пользователя
        /// </summary>
        public async Task<List<Equipment>> GetEquipmentsWithFavoritesAsync(string userId, bool onlyFavorites = false)
        {
            using var context = _contextFactory.CreateDbContext();

            var query = context.Equipments
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    Equipment = e,
                    IsFavorite = context.UserFavorites
                        .Any(uf => uf.UserId == userId && uf.EquipmentId == e.Id)
                });

            if (onlyFavorites)
            {
                query = query.Where(x => x.IsFavorite);
            }

            var result = await query
                .OrderByDescending(x => x.IsFavorite)
                .ThenBy(x => x.Equipment.Name)
                .ToListAsync();

            return result.Select(x => x.Equipment).ToList();
        }

        /// <summary>
        /// Получение активной техники
        /// </summary>
        public async Task<List<Equipment>> GetActiveEquipmentsAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            return await context.Equipments
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Добавление в избранное
        /// </summary>
        public async Task AddToFavoritesAsync(string userId, string equipmentId)
        {
            using var context = _contextFactory.CreateDbContext();

            var exists = await context.UserFavorites
                .AnyAsync(uf => uf.UserId == userId && uf.EquipmentId == equipmentId);

            if (!exists)
            {
                var maxOrder = await context.UserFavorites
                    .Where(uf => uf.UserId == userId)
                    .MaxAsync(uf => (int?)uf.SortOrder) ?? 0;

                var favorite = new UserFavorite
                {
                    UserId = userId,
                    EquipmentId = equipmentId,
                    SortOrder = maxOrder + 1
                };

                await context.UserFavorites.AddAsync(favorite);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Удаление из избранного
        /// </summary>
        public async Task RemoveFromFavoritesAsync(string userId, string equipmentId)
        {
            using var context = _contextFactory.CreateDbContext();

            var favorite = await context.UserFavorites
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.EquipmentId == equipmentId);

            if (favorite != null)
            {
                context.UserFavorites.Remove(favorite);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Проверка, находится ли техника в избранном
        /// </summary>
        public async Task<bool> IsFavoriteAsync(string userId, string equipmentId)
        {
            using var context = _contextFactory.CreateDbContext();

            return await context.UserFavorites
                .AnyAsync(uf => uf.UserId == userId && uf.EquipmentId == equipmentId);
        }

        #endregion

        #region Переопределенные методы

        /// <summary>
        /// Добавление техники
        /// </summary>
        public override async Task<Equipment> AddAsync(Equipment entity)
        {
            if (!_authorizationService.CanWriteTable("Equipments"))
                throw new UnauthorizedAccessException("Нет прав на добавление техники");

            entity.CreatedAt = DateTime.UtcNow;
            return await base.AddAsync(entity);
        }

        /// <summary>
        /// Обновление техники
        /// </summary>
        public override async Task<Equipment> UpdateAsync(Equipment entity)
        {
            if (!_authorizationService.CanWriteTable("Equipments"))
                throw new UnauthorizedAccessException("Нет прав на редактирование техники");

            return await base.UpdateAsync(entity);
        }

        /// <summary>
        /// Удаление техники
        /// </summary>
        public override async Task<bool> DeleteAsync(object id)
        {
            if (!_authorizationService.CanWriteTable("Equipments"))
                throw new UnauthorizedAccessException("Нет прав на удаление техники");

            return await base.DeleteAsync(id);
        }

        #endregion
    }
}