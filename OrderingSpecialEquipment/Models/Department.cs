using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель для таблицы отделов.
    /// </summary>
    [Table("Departments")]
    public class Department
    {
        [Key]
        [Column("Id")]
        [StringLength(10)]
        [Display(Name = "ID отдела", Description = "Уникальный идентификатор отдела")]
        public string Id { get; set; } = string.Empty;

        [Column("Key")]
        [Display(Name = "Ключ", Description = "Суррогатный ключ (SERIAL)")]
        public int Key { get; set; } // SERIAL

        [Column("Name")]
        [StringLength(100)]
        [Display(Name = "Название", Description = "Название отдела")]
        public string Name { get; set; } = string.Empty;

        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Активен ли отдел")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- НАВИГАЦИОННЫЕ СВОЙСТВА ---
        // Связь: Один ко многим (Department -> UserDepartmentAccess)
        /// <summary>
        /// Коллекция записей доступа пользователей к отделу.
        /// </summary>
        // УБРАНО: [ForeignKey("DepartmentId")] - EF Core сам поймёт связь по названию свойства DepartmentId в UserDepartmentAccess
        public virtual ICollection<UserDepartmentAccess> UserDepartmentAccesses { get; set; } = new List<UserDepartmentAccess>();

        // Связь: Один ко многим (Department -> Warehouse)
        /// <summary>
        /// Коллекция складов, принадлежащих отделу.
        /// </summary>
        // УБРАНО: [ForeignKey("DepartmentId")] - EF Core сам поймёт связь по названию свойства DepartmentId в Warehouse
        public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

        // Связь: Один ко многим (Department -> ShiftRequest)
        /// <summary>
        /// Коллекция заявок, связанных с отделом.
        /// </summary>
        // УБРАНО: [ForeignKey("DepartmentId")] - EF Core сам поймёт связь по названию свойства DepartmentId в ShiftRequest
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; } = new List<ShiftRequest>();

        // Связь: Один ко многим (Department -> TransportProgram)
        /// <summary>
        /// Коллекция записей транспортной программы для отдела.
        /// </summary>
        // УБРАНО: [ForeignKey("DepartmentId")] - EF Core сам поймёт связь по названию свойства DepartmentId в TransportProgram
        public virtual ICollection<TransportProgram> TransportPrograms { get; set; } = new List<TransportProgram>();
    }
}