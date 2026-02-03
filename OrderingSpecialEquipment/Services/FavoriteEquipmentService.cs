using Microsoft.Extensions.DependencyInjection;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация сервиса избранной техники
    /// Управляет добавлением/удалением техники в избранное для пользователя
    /// Создает новый сервис скоуп для каждой операции
    /// </summary>
    public class FavoriteEquipmentService : IFavoriteEquipmentService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Конструктор с внедрением зависимости
        /// </summary>
        public FavoriteEquipmentService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Получает избранную технику для пользователя
        /// </summary>
        public async Task<IEnumerable<Equipment>> GetFavoriteEquipmentAsync(string userId)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userFavoriteRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<UserFavorite>>();
                    var equipmentRepo = scope.ServiceProvider.GetRequiredService<IEquipmentRepository>();

                    // Получаем записи избранного для пользователя
                    var favorites = await userFavoriteRepo.FindAsync(f => f.UserId == userId);

                    // Получаем технику для каждой записи
                    var equipmentList = new List<Equipment>();
                    foreach (var favorite in favorites)
                    {
                        var equipment = await equipmentRepo.GetByIdAsync(favorite.EquipmentId);
                        if (equipment != null && equipment.IsActive)
                        {
                            equipmentList.Add(equipment);
                        }
                    }

                    return equipmentList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении избранной техники: {ex.Message}");
                return new List<Equipment>();
            }
        }

        /// <summary>
        /// Добавляет технику в избранное
        /// </summary>
        public async Task AddToFavoriteAsync(string userId, string equipmentId)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userFavoriteRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<UserFavorite>>();
                    var equipmentRepo = scope.ServiceProvider.GetRequiredService<IEquipmentRepository>();

                    // Проверяем, существует ли уже такая запись
                    var existing = await userFavoriteRepo.FindAsync(f =>
                        f.UserId == userId && f.EquipmentId == equipmentId);

                    if (!existing.Any())
                    {
                        // Получаем следующий порядок сортировки
                        var allFavorites = await userFavoriteRepo.FindAsync(f => f.UserId == userId);
                        int sortOrder = allFavorites.Any() ? allFavorites.Max(f => f.SortOrder) + 1 : 0;

                        // Создаем новую запись
                        var favorite = new UserFavorite
                        {
                            UserId = userId,
                            EquipmentId = equipmentId,
                            SortOrder = sortOrder
                        };

                        await userFavoriteRepo.AddAsync(favorite);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении в избранное: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Удаляет технику из избранного
        /// </summary>
        public async Task RemoveFromFavoriteAsync(string userId, string equipmentId)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userFavoriteRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<UserFavorite>>();

                    // Находим запись
                    var favorites = await userFavoriteRepo.FindAsync(f =>
                        f.UserId == userId && f.EquipmentId == equipmentId);

                    if (favorites.Any())
                    {
                        await userFavoriteRepo.RemoveAsync(favorites.First());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении из избранного: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Проверяет, находится ли техника в избранном
        /// </summary>
        public async Task<bool> IsFavoriteAsync(string userId, string equipmentId)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userFavoriteRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<UserFavorite>>();

                    var existing = await userFavoriteRepo.FindAsync(f =>
                        f.UserId == userId && f.EquipmentId == equipmentId);

                    return existing.Any();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке избранного: {ex.Message}");
                return false;
            }
        }
    }
}