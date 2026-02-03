using OrderingSpecialEquipment.Models;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Интерфейс сервиса зависимостей техники
    /// Управляет автоматическим созданием зависимых заявок
    /// </summary>
    public interface IEquipmentDependencyService
    {
        /// <summary>
        /// Обрабатывает зависимости техники при создании/обновлении заявки
        /// Автоматически создает заявки на зависимую технику
        /// </summary>
        Task ProcessDependenciesAsync(ShiftRequest mainRequest, User currentUser);
    }
}