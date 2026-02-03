using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель избранной техники пользователя
    /// </summary>
    [Table("UserFavorites")]
    public class UserFavorite
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// ID пользователя
        /// </summary>
        [Column("UserId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Пользователь", Description = "Ссылка на пользователя")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к пользователю
        /// </summary>
        [ForeignKey("UserId")]
        [Display(Name = "Пользователь", Description = "Детали пользователя")]
        public User? User { get; set; }

        /// <summary>
        /// ID техники
        /// </summary>
        [Column("EquipmentId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Техника", Description = "Ссылка на технику")]
        public string EquipmentId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к технике
        /// </summary>
        [ForeignKey("EquipmentId")]
        [Display(Name = "Техника", Description = "Детали техники")]
        public Equipment? Equipment { get; set; }

        /// <summary>
        /// Порядок сортировки в избранном
        /// </summary>
        [Column("SortOrder")]
        [Display(Name = "Порядок", Description = "Порядок отображения в списке избранного")]
        [DefaultValue(0)]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}