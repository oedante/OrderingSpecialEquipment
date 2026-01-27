using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class UserFavoriteRepository : GenericRepository<UserFavorite>, IUserFavoriteRepository
    {
        public UserFavoriteRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IList<UserFavorite>> GetByUserAsync(string userId)
        {
            return await Context.UserFavorites
                                .Where(uf => uf.UserId == userId)
                                .OrderBy(uf => uf.SortOrder)
                                .ToListAsync();
        }

        public async Task<IList<UserFavorite>> GetByUserAndEquipmentAsync(string userId, string equipmentId)
        {
            return await Context.UserFavorites
                                .Where(uf => uf.UserId == userId && uf.EquipmentId == equipmentId)
                                .ToListAsync();
        }

        public async Task<UserFavorite?> GetByUserEquipmentAsync(string userId, string equipmentId)
        {
            return await Context.UserFavorites
                                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.EquipmentId == equipmentId);
        }

        public override async Task<UserFavorite?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.UserFavorites
                                    .FirstOrDefaultAsync(uf => uf.Key == intId);
            }
            return null;
        }

        // UserFavorite не имеет Id типа string, поэтому метод не переопределяется.
        // public override async Task<UserFavorite?> GetByIdStringAsync(string id) => null;
    }
}