using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель журнала аудита
    /// Таблица AuditLogs в базе данных
    /// </summary>
    [Table("AuditLogs")]
    [Display(Name = "Журнал аудита", Description = "Журнал изменений в системе")]
    public class AuditLog
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
        /// Наименование таблицы
        /// </summary>
        [Required]
        [Column("TableName")]
        [Display(Name = "Таблица", Description = "Наименование таблицы")]
        [StringLength(50)]
        public string TableName { get; set; }

        /// <summary>
        /// Идентификатор записи
        /// </summary>
        [Required]
        [Column("RecordId")]
        [Display(Name = "ID записи", Description = "Идентификатор измененной записи")]
        [StringLength(50)]
        public string RecordId { get; set; }

        /// <summary>
        /// Действие (INSERT, UPDATE, DELETE)
        /// </summary>
        [Required]
        [Column("Action")]
        [Display(Name = "Действие", Description = "Выполненное действие")]
        [StringLength(20)]
        public string Action { get; set; }

        /// <summary>
        /// Старые значения в формате JSON
        /// </summary>
        [Column("OldValues")]
        [Display(Name = "Старые значения", Description = "Старые значения в формате JSON")]
        public string OldValues { get; set; }

        /// <summary>
        /// Новые значения в формате JSON
        /// </summary>
        [Column("NewValues")]
        [Display(Name = "Новые значения", Description = "Новые значения в формате JSON")]
        public string NewValues { get; set; }

        /// <summary>
        /// ID пользователя, выполнившего изменение
        /// </summary>
        [Required]
        [Column("ChangedByUserId")]
        [Display(Name = "Пользователь", Description = "ID пользователя, выполнившего изменение")]
        [StringLength(10)]
        public string ChangedByUserId { get; set; }

        /// <summary>
        /// Дата и время изменения
        /// </summary>
        [Column("ChangedAt")]
        [Display(Name = "Дата изменения", Description = "Дата и время изменения")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// IP-адрес
        /// </summary>
        [Column("IPAddress")]
        [Display(Name = "IP-адрес", Description = "IP-адрес пользователя")]
        [StringLength(50)]
        public string IPAddress { get; set; }

        /// <summary>
        /// User-Agent
        /// </summary>
        [Column("UserAgent")]
        [Display(Name = "User-Agent", Description = "User-Agent браузера/клиента")]
        [StringLength(500)]
        public string UserAgent { get; set; }

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Пользователь, выполнивший изменение
        /// </summary>
        [ForeignKey("ChangedByUserId")]
        public virtual User ChangedByUser { get; set; }

        #endregion
    }
}