using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы техники.
    /// </summary>
    [Table("Equipments")]
    public class Equipment
    {
        [Key]
        [Column("Id")]
        [StringLength(10)]
        [Display(Name = "ID техники", Description = "Уникальный идентификатор техники")]
        public string Id { get; set; } = string.Empty;

        [Column("Key")]
        [Display(Name = "Ключ", Description = "Суррогатный ключ (SERIAL)")]
        public int Key { get; set; } // SERIAL

        [Column("Name")]
        [StringLength(200)]
        [Display(Name = "Название", Description = "Название техники")]
        public string Name { get; set; } = string.Empty;

        [Column("Category")]
        [StringLength(50)]
        [Display(Name = "Категория", Description = "Категория техники (например, Спецтехника, Рабочий)")]
        public string? Category { get; set; }

        [Column("CanOrderMultiple")]
        [Display(Name = "Можно заказать много", Description = "Можно ли заказать больше одной единицы в одной строке")]
        public bool CanOrderMultiple { get; set; } = false;

        [Column("HourlyCost")]
        [Display(Name = "Стоимость часа", Description = "Стоимость часа работы в рублях")]
        public decimal? HourlyCost { get; set; }

        [Column("RequiresOperator")]
        [Display(Name = "Требует оператора", Description = "Требуется ли оператор для управления")]
        public bool RequiresOperator { get; set; } = false;

        [Column("Description")]
        [StringLength(500)]
        [Display(Name = "Описание", Description = "Дополнительное описание техники")]
        public string? Description { get; set; }

        [Column("IsActive")]
        [Display(Name = "Активна", Description = "Активна ли запись техники")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual ICollection<EquipmentDependency> EquipmentDependenciesAsMain { get; set; } = new List<EquipmentDependency>();
        public virtual ICollection<EquipmentDependency> EquipmentDependenciesAsDependent { get; set; } = new List<EquipmentDependency>();
        public virtual ICollection<LicensePlate> LicensePlates { get; set; } = new List<LicensePlate>();
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; } = new List<ShiftRequest>();
        public virtual ICollection<TransportProgram> TransportPrograms { get; set; } = new List<TransportProgram>();
        public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
    }
}