using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель лога аудита изменений
    /// </summary>
    [Table("AuditLogs")]
    public class AuditLog
    {
        /// <summary>
        /// Внутренний числовой ключ (для связей)
        /// </summary>
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Внутренний числовой идентификатор записи")]
        public int Key { get; set; }

        /// <summary>
        /// Имя таблицы
        /// </summary>
        [Column("TableName")]
        [StringLength(50)]
        [Required]
        [Display(Name = "Таблица", Description = "Имя таблицы, в которой произошло изменение")]
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// ID записи
        /// </summary>
        [Column("RecordId")]
        [StringLength(50)]
        [Required]
        [Display(Name = "ID записи", Description = "Идентификатор измененной записи")]
        public string RecordId { get; set; } = string.Empty;

        /// <summary>
        /// Действие (INSERT, UPDATE, DELETE)
        /// </summary>
        [Column("Action")]
        [StringLength(20)]
        [Required]
        [Display(Name = "Действие", Description = "Тип действия (INSERT, UPDATE, DELETE)")]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Старые значения (JSON)
        /// </summary>
        [Column("OldValues")]
        [Display(Name = "Старые значения", Description = "Старые значения в формате JSON")]
        public string? OldValues { get; set; }

        /// <summary>
        /// Новые значения (JSON)
        /// </summary>
        [Column("NewValues")]
        [Display(Name = "Новые значения", Description = "Новые значения в формате JSON")]
        public string? NewValues { get; set; }

        /// <summary>
        /// ID пользователя, внесшего изменения
        /// </summary>
        [Column("ChangedByUserId")]
        [StringLength(10)]
        [Required]
        [Display(Name = "Изменил", Description = "Ссылка на пользователя, внесшего изменения")]
        public string ChangedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к пользователю
        /// </summary>
        [ForeignKey("ChangedByUserId")]
        [Display(Name = "Изменил", Description = "Детали пользователя")]
        public User? ChangedByUser { get; set; }

        /// <summary>
        /// Дата и время изменения
        /// </summary>
        [Column("ChangedAt")]
        [Required]
        [Display(Name = "Дата изменения", Description = "Дата и время внесения изменений")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// IP адрес пользователя
        /// </summary>
        [Column("IPAddress")]
        [StringLength(50)]
        [Display(Name = "IP адрес", Description = "IP адрес пользователя")]
        public string? IPAddress { get; set; }

        /// <summary>
        /// User Agent браузера/приложения
        /// </summary>
        [Column("UserAgent")]
        [StringLength(500)]
        [Display(Name = "User Agent", Description = "User Agent приложения")]
        public string? UserAgent { get; set; }
    }
}