using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория зависимостей техники
    /// </summary>
    public interface IEquipmentDependencyRepository : IGenericRepository<EquipmentDependency>
    {
        /// <summary>
        /// Получает зависимости для основной техники
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<EquipmentDependency>> GetDependenciesForMainEquipmentAsync(string mainEquipmentId);
    }
}