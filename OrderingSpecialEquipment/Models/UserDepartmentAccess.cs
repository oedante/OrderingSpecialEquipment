using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель доступа пользователя к отделам
    /// Таблица UserDepartmentAccess в базе данных
    /// </summary>
    [Table("UserDepartmentAccess")]
    [Display(Name = "Доступ к отделам", Description = "Настройки доступа пользователя к отделам")]
    public class UserDepartmentAccess
    {
        #region Свойства

        /// <summary>
        /// Числовой ключ (первичный ключ)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        [Required]
        [Column("UserId")]
        [Display(Name = "Пользователь", Description = "Идентификатор пользователя")]
        [StringLength(10)]
        public string UserId { get; set; }

        /// <summary>
        /// Идентификатор отдела
        /// </summary>
        [Required]
        [Column("DepartmentId")]
        [Display(Name = "Отдел", Description = "Идентификатор отдела")]
        [StringLength(10)]
        public string DepartmentId { get; set; }

        /// <summary>
        /// Имеет ли доступ ко всем складам отдела
        /// </summary>
        [Column("HasAllWarehouses")]
        [Display(Name = "Все склады", Description = "Имеет ли пользователь доступ ко всем складам отдела")]
        [DefaultValue(false)]
        public bool HasAllWarehouses { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Пользователь
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        /// <summary>
        /// Отдел
        /// </summary>
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        /// <summary>
        /// Список доступов к складам
        /// </summary>
        public virtual ICollection<UserWarehouseAccess> UserWarehouseAccesses { get; set; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public UserDepartmentAccess()
        {
            UserWarehouseAccesses = new HashSet<UserWarehouseAccess>();
        }

        #endregion
    }
}