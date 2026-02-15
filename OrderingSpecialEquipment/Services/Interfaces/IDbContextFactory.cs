using OrderingSpecialEquipment.Data;

namespace OrderingSpecialEquipment.Services.Interfaces
{
    /// <summary>
    /// Интерфейс фабрики для создания контекстов БД
    /// </summary>
    public interface IDbContextFactory
    {
        /// <summary>
        /// Создание нового экземпляра контекста БД
        /// </summary>
        ApplicationDbContext CreateDbContext();
    }
}