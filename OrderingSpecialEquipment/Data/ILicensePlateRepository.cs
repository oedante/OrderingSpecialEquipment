using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория госномеров
    /// </summary>
    public interface ILicensePlateRepository : IGenericRepository<LicensePlate>
    {
        /// <summary>
        /// Получает активные госномера
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<LicensePlate>> GetActiveLicensePlatesAsync();

        /// <summary>
        /// Поиск госномеров по частичному совпадению
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<LicensePlate>> SearchByPlateNumberAsync(string searchTerm);
    }
}