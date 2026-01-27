using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы избранного пользователей.
    /// Хранится локально для каждого пользователя.
    /// </summary>
    [Table("UserFavorites")]
    public class UserFavorite
    {
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Уникальный ключ избранного")]
        public int Key { get; set; } // SERIAL

        [Column("UserId")]
        [StringLength(10)]
        [Display(Name = "ID пользователя", Description = "ID пользователя, чье избранное")]
        public string UserId { get; set; } = string.Empty;

        [Column("EquipmentId")]
        [StringLength(10)]
        [Display(Name = "ID техники", Description = "ID техники, добавленной в избранное")]
        public string EquipmentId { get; set; } = string.Empty;

        [Column("SortOrder")]
        [Display(Name = "Порядок сортировки", Description = "Порядок отображения в списке избранного")]
        public int SortOrder { get; set; } = 0;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время добавления в избранное")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual User User { get; set; } = null!;
        public virtual Equipment Equipment { get; set; } = null!;
    }
}