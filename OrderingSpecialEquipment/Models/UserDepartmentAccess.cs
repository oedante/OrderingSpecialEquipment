using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы доступа пользователей к отделам.
    /// </summary>
    [Table("UserDepartmentAccess")]
    public class UserDepartmentAccess
    {
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Уникальный ключ доступа к отделу")]
        public int Key { get; set; } // SERIAL

        [Column("UserId")]
        [StringLength(10)]
        [Display(Name = "ID пользователя", Description = "ID пользователя, имеющего доступ")]
        public string UserId { get; set; } = string.Empty;

        [Column("DepartmentId")]
        [StringLength(10)]
        [Display(Name = "ID отдела", Description = "ID отдела, к которому есть доступ")]
        public string DepartmentId { get; set; } = string.Empty;

        [Column("HasAllWarehouses")]
        [Display(Name = "Доступ ко всем складам", Description = "Имеет ли пользователь доступ ко всем складам отдела")]
        public bool HasAllWarehouses { get; set; } = false;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual User User { get; set; } = null!;
        public virtual Department Department { get; set; } = null!;
        public virtual ICollection<UserWarehouseAccess> UserWarehouseAccesses { get; set; } = new List<UserWarehouseAccess>();
    }
}