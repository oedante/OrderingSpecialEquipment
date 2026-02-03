using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория техники
    /// </summary>
    public interface IEquipmentRepository : IGenericRepository<Equipment>
    {
        /// <summary>
        /// Получает активную технику
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<Equipment>> GetActiveEquipmentAsync();
    }
}