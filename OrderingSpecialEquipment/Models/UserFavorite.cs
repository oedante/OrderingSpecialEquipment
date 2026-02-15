using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель избранного пользователя
    /// Таблица UserFavorites в базе данных
    /// </summary>
    [Table("UserFavorites")]
    [Display(Name = "Избранное", Description = "Избранная техника пользователя")]
    public class UserFavorite
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
        /// Идентификатор техники
        /// </summary>
        [Required]
        [Column("EquipmentId")]
        [Display(Name = "Техника", Description = "Идентификатор техники")]
        [StringLength(10)]
        public string EquipmentId { get; set; }

        /// <summary>
        /// Порядок сортировки
        /// </summary>
        [Column("SortOrder")]
        [Display(Name = "Порядок", Description = "Порядок сортировки")]
        public int SortOrder { get; set; }

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
        /// Техника
        /// </summary>
        [ForeignKey("EquipmentId")]
        public virtual Equipment Equipment { get; set; }

        #endregion
    }
}