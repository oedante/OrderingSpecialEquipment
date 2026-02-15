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
        public EquipmentService(IDatabaseService databaseService, IAuthorizationService authorizationService)
            : base(databaseService, authorizationService)
        {
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Получение зависимостей для техники
        /// </summary>
        public async Task<List<EquipmentDependency>> GetDependenciesAsync(string equipmentId)
        {
            return await _databaseService.Context.EquipmentDependencies
                .Include(ed => ed.DependentEquipment)
                .Where(ed => ed.MainEquipmentId == equipmentId)
                .ToListAsync();
        }

        /// <summary>
        /// Получение техники с учетом избранного пользователя
        /// </summary>
        public async Task<List<Equipment>> GetEquipmentsWithFavoritesAsync(string userId, bool onlyFavorites = false)
        {
            var query = _databaseService.Context.Equipments
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    Equipment = e,
                    IsFavorite = _databaseService.Context.UserFavorites
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
            return await _databaseService.Context.Equipments
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Добавление в избранное
        /// </summary>
        public async Task AddToFavoritesAsync(string userId, string equipmentId)
        {
            var exists = await _databaseService.Context.UserFavorites
                .AnyAsync(uf => uf.UserId == userId && uf.EquipmentId == equipmentId);

            if (!exists)
            {
                var maxOrder = await _databaseService.Context.UserFavorites
                    .Where(uf => uf.UserId == userId)
                    .MaxAsync(uf => (int?)uf.SortOrder) ?? 0;

                var favorite = new UserFavorite
                {
                    UserId = userId,
                    EquipmentId = equipmentId,
                    SortOrder = maxOrder + 1
                };

                await _databaseService.Context.UserFavorites.AddAsync(favorite);
                await _databaseService.Context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Удаление из избранного
        /// </summary>
        public async Task RemoveFromFavoritesAsync(string userId, string equipmentId)
        {
            var favorite = await _databaseService.Context.UserFavorites
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.EquipmentId == equipmentId);

            if (favorite != null)
            {
                _databaseService.Context.UserFavorites.Remove(favorite);
                await _databaseService.Context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Проверка, находится ли техника в избранном
        /// </summary>
        public async Task<bool> IsFavoriteAsync(string userId, string equipmentId)
        {
            return await _databaseService.Context.UserFavorites
                .AnyAsync(uf => uf.UserId == userId && uf.EquipmentId == equipmentId);
        }

        #endregion
    }
}