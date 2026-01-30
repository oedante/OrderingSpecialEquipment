using OrderingSpecialEquipment.Data.Repositories;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Реализация IFavoriteEquipmentService.
    /// </summary>
    public class FavoriteEquipmentService : IFavoriteEquipmentService
    {
        private readonly IUserFavoriteRepository _userFavoriteRepo;
        private readonly IEquipmentRepositoryBase _equipmentRepo;

        public FavoriteEquipmentService(
            IUserFavoriteRepository userFavoriteRepo,
            IEquipmentRepositoryBase equipmentRepo)
        {
            _userFavoriteRepo = userFavoriteRepo;
            _equipmentRepo = equipmentRepo;
        }

        public async Task<List<Equipment>> GetFavoriteEquipmentAsync(string userId)
        {
            var favoriteRecords = await _userFavoriteRepo.GetByUserAsync(userId);
            var equipmentList = new List<Equipment>();

            foreach (var fav in favoriteRecords.OrderBy(f => f.SortOrder))
            {
                var equipment = await _equipmentRepo.GetByIdStringAsync(fav.EquipmentId);
                if (equipment != null && equipment.IsActive) // Фильтруем неактивную технику
                {
                    equipmentList.Add(equipment);
                }
            }
            return equipmentList;
        }

        public async Task<bool> AddToFavoritesAsync(string userId, string equipmentId, int sortOrder = 0)
        {
            // Проверим, существует ли техника и активна ли она
            var equipment = await _equipmentRepo.GetByIdStringAsync(equipmentId);
            if (equipment == null || !equipment.IsActive)
            {
                Console.WriteLine("Техника не найдена или не активна.");
                return false;
            }

            // Проверим, нет ли уже в избранном
            var existing = await _userFavoriteRepo.GetByUserEquipmentAsync(userId, equipmentId);
            if (existing != null)
            {
                Console.WriteLine("Техника уже в избранном.");
                return false;
            }

            var newFavorite = new UserFavorite
            {
                UserId = userId,
                EquipmentId = equipmentId,
                SortOrder = sortOrder
            };

            try
            {
                await _userFavoriteRepo.AddAsync(newFavorite);
                await _userFavoriteRepo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления в избранное: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveFromFavoritesAsync(string userId, string equipmentId)
        {
            var record = await _userFavoriteRepo.GetByUserEquipmentAsync(userId, equipmentId);
            if (record == null)
            {
                Console.WriteLine("Техника не найдена в избранном.");
                return false;
            }

            try
            {
                _userFavoriteRepo.Delete(record);
                await _userFavoriteRepo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления из избранного: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsFavoriteAsync(string userId, string equipmentId)
        {
            var record = await _userFavoriteRepo.GetByUserEquipmentAsync(userId, equipmentId);
            return record != null;
        }
    }
}