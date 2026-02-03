using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Интерфейс репозитория транспортной программы
    /// </summary>
    public interface ITransportProgramRepository : IGenericRepository<TransportProgram>
    {
        /// <summary>
        /// Получает программу для отдела за год
        /// </summary>
        Task<System.Collections.Generic.IEnumerable<TransportProgram>> GetProgramForDepartmentAsync(string departmentId, int year);
    }
}