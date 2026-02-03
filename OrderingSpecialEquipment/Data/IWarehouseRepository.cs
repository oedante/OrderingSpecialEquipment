using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория складов
    /// </summary>
    public interface IWarehouseRepository : IGenericRepository<Warehouse>
    {
        /// <summary>
        /// Получает активные склады
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<Warehouse>> GetActiveWarehousesAsync();
    }
}