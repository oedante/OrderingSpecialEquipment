using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы пользователей.
    /// </summary>
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("Id")]
        [StringLength(10)]
        [Display(Name = "ID пользователя", Description = "Уникальный идентификатор пользователя")]
        public string Id { get; set; } = string.Empty;

        [Column("Key")]
        [Display(Name = "Ключ", Description = "Суррогатный ключ (SERIAL)")]
        public int Key { get; set; } // SERIAL

        [Column("WindowsLogin")]
        [StringLength(100)]
        [Display(Name = "Логин Windows", Description = "Имя входа пользователя Windows")]
        public string WindowsLogin { get; set; } = string.Empty;

        [Column("FullName")]
        [StringLength(150)]
        [Display(Name = "ФИО", Description = "Полное имя пользователя")]
        public string FullName { get; set; } = string.Empty;

        [Column("Email")]
        [StringLength(100)]
        [Display(Name = "Email", Description = "Контактный email")]
        public string? Email { get; set; }

        [Column("Phone")]
        [StringLength(20)]
        [Display(Name = "Телефон", Description = "Контактный телефон")]
        public string? Phone { get; set; }

        [Column("RoleId")]
        [StringLength(10)]
        [Display(Name = "ID роли", Description = "ID назначенной роли пользователя")]
        public string RoleId { get; set; } = string.Empty;

        [Column("DefaultDepartmentId")]
        [StringLength(10)]
        [Display(Name = "ID отдела по умолчанию", Description = "ID отдела, используемого по умолчанию")]
        public string? DefaultDepartmentId { get; set; }

        [Column("HasAllDepartments")]
        [Display(Name = "Доступ ко всем отделам", Description = "Имеет ли пользователь доступ ко всем отделам")]
        public bool HasAllDepartments { get; set; } = false;

        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Активен ли пользователь")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства (опционально, для EF)
        public virtual Role Role { get; set; } = null!;
        public virtual Department? DefaultDepartment { get; set; }
        public virtual ICollection<UserDepartmentAccess> UserDepartmentAccesses { get; set; } = new List<UserDepartmentAccess>();
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; } = new List<ShiftRequest>();
        public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
    }
}