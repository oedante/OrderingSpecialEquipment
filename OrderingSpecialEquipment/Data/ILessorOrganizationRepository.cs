using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория организаций-арендодателей
    /// </summary>
    public interface ILessorOrganizationRepository : IGenericRepository<LessorOrganization>
    {
        /// <summary>
        /// Получает активные организации
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<LessorOrganization>> GetActiveLessorOrganizationsAsync();
    }
}