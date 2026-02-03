using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория отделов
    /// Расширяет обобщенный репозиторий специфичными методами
    /// </summary>
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        /// <summary>
        /// Получает активные отделы
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<Department>> GetActiveDepartmentsAsync();
    }
}