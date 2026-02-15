using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Фабрика для создания контекста БД во время разработки (для миграций)
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        /// <summary>
        /// Создание контекста БД
        /// </summary>
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // По умолчанию используем PostgreSQL для разработки
            optionsBuilder.UseNpgsql("Host=217.114.43.126;Port=5432;Database=OrderingSpecialEquipment;Username=student;Password=Qq587655!;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}