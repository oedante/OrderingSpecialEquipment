using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для логирования изменений в других таблицах.
    /// </summary>
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        [Column("Key")]
        [Display(Name = "Ключ лога", Description = "Уникальный ключ записи лога")]
        public int Key { get; set; }

        [Column("TableName")]
        [StringLength(50)]
        [Display(Name = "Таблица", Description = "Имя таблицы, в которой произошло изменение")]
        public string TableName { get; set; } = string.Empty;

        [Column("RecordId")]
        [StringLength(50)]
        [Display(Name = "ID записи", Description = "ID записи в таблице, которая была изменена")]
        public string RecordId { get; set; } = string.Empty;

        [Column("Action")]
        [StringLength(20)]
        [Display(Name = "Действие", Description = "Тип действия: INSERT, UPDATE, DELETE")]
        public string Action { get; set; } = string.Empty;

        [Column("OldValues")]
        [Display(Name = "Старые значения", Description = "JSON представление старых значений до изменения")]
        public string? OldValues { get; set; }

        [Column("NewValues")]
        [Display(Name = "Новые значения", Description = "JSON представление новых значений после изменения")]
        public string? NewValues { get; set; }

        [Column("ChangedByUserId")]
        [StringLength(10)]
        [Display(Name = "ID пользователя", Description = "ID пользователя, который совершил изменение")]
        public string ChangedByUserId { get; set; } = string.Empty;

        [Column("ChangedAt")]
        [Display(Name = "Дата изменения", Description = "Дата и время изменения")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Column("IPAddress")]
        [StringLength(50)]
        [Display(Name = "IP адрес", Description = "IP-адрес клиента, инициировавшего изменение")]
        public string? IPAddress { get; set; }

        [Column("UserAgent")]
        [StringLength(500)]
        [Display(Name = "User Agent", Description = "Информация о браузере/клиенте")]
        public string? UserAgent { get; set; }
    }
}