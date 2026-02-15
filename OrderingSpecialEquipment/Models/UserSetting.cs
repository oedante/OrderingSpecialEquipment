using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель настроек пользователя
    /// Таблица UserSettings в базе данных
    /// </summary>
    [Table("UserSettings")]
    [Display(Name = "Настройки пользователя", Description = "Настройки пользователя в формате JSON")]
    public class UserSetting
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
        /// Ключ настройки
        /// </summary>
        [Required]
        [Column("SettingKey")]
        [Display(Name = "Ключ настройки", Description = "Ключ настройки")]
        [StringLength(50)]
        public string SettingKey { get; set; }

        /// <summary>
        /// Значение настройки в формате JSON
        /// </summary>
        [Required]
        [Column("SettingValue")]
        [Display(Name = "Значение", Description = "Значение настройки в формате JSON")]
        public string SettingValue { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата обновления записи
        /// </summary>
        [Column("UpdatedAt")]
        [Display(Name = "Дата обновления", Description = "Дата и время обновления записи")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Пользователь
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        #endregion
    }
}