using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Data;
using OrderingSpecialEquipment.Services.Interfaces;
using System;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Фабрика для создания контекстов БД
    /// </summary>
    public class DbContextFactory : IDbContextFactory
    {
        private readonly IDatabaseService _databaseService;
        private readonly object _lockObject = new object();

        public DbContextFactory(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        /// <summary>
        /// Создание нового экземпляра контекста БД
        /// </summary>
        public ApplicationDbContext CreateDbContext()
        {
            if (!_databaseService.IsConnected || _databaseService.DatabaseType == null)
                throw new InvalidOperationException("База данных не подключена");

            lock (_lockObject)
            {
                var options = DbConnectionFactory.CreateDbContextOptions(
                    _databaseService.GetConnectionString(),
                    _databaseService.DatabaseType.Value);

                return new ApplicationDbContext(options);
            }
        }
    }
}