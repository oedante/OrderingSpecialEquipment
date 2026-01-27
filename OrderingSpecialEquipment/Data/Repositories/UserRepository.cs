using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(string id)
        {
            return await Context.Users.AnyAsync(u => u.Id == id);
        }

        // --- НОВЫЙ МЕТОД ---
        public async Task<User?> GetByWindowsLoginAsync(string windowsLogin)
        {
            // Ищем пользователя в БД по WindowsLogin, учитывая активность
            return await Context.Users
                                .Include(u => u.Role) // Загружаем роль сразу
                                .FirstOrDefaultAsync(u => u.WindowsLogin == windowsLogin && u.IsActive);
        }
        // --- /НОВЫЙ МЕТОД ---

        public async Task<IList<User>> GetActiveAsync()
        {
            return await Context.Users
                                .Include(u => u.Role)
                                .Where(u => u.IsActive)
                                .OrderBy(u => u.FullName)
                                .ToListAsync();
        }

        public override async Task<IList<User>> GetAllAsync()
        {
            return await Context.Users
                                .Include(u => u.Role)
                                .OrderBy(u => u.FullName)
                                .ToListAsync();
        }

        public override async Task<User?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await Context.Users
                                    .Include(u => u.Role)
                                    .FirstOrDefaultAsync(u => u.Key == intId);
            }
            return null;
        }

        public override async Task<User?> GetByIdStringAsync(string id)
        {
            return await Context.Users
                                .Include(u => u.Role)
                                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}