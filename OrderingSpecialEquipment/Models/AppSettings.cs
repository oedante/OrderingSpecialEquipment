using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для хранения настроек приложения, включая подключение к БД.
    /// </summary>
    [Table("AppSettings")] // Не соответствует существующей таблице, используется для внутренних целей
    public class AppSettings
    {
        [Key]
        [Column("SettingKey")]
        [Display(Name = "Ключ настройки", Description = "Уникальный ключ настройки")]
        public string SettingKey { get; set; } = string.Empty;

        [Column("SettingValue")]
        [Display(Name = "Значение настройки", Description = "Значение настройки")]
        public string SettingValue { get; set; } = string.Empty;

        [Column("Description")]
        [Display(Name = "Описание", Description = "Описание назначения настройки")]
        public string? Description { get; set; }
    }
}