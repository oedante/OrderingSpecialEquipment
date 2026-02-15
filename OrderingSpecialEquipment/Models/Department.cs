using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OrderingSpecialEquipment.Models
{
    /// <summary>
    /// Модель отдела
    /// Таблица Departments в базе данных
    /// </summary>
    [Table("Departments")]
    [Display(Name = "Отдел", Description = "Информация об отделе предприятия")]
    public class Department
    {
        #region Свойства

        /// <summary>
        /// Уникальный идентификатор отдела (первичный ключ)
        /// Формат: DE000001
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "Идентификатор", Description = "Уникальный идентификатор отдела")]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Числовой ключ (автоинкремент)
        /// </summary>
        [Column("Key")]
        [Display(Name = "Ключ", Description = "Числовой автоинкрементный ключ")]
        public int Key { get; set; }

        /// <summary>
        /// Наименование отдела
        /// </summary>
        [Required(ErrorMessage = "Наименование отдела обязательно")]
        [Column("Name")]
        [Display(Name = "Наименование", Description = "Наименование отдела")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Активен ли отдел
        /// </summary>
        [Column("IsActive")]
        [Display(Name = "Активен", Description = "Признак активности отдела")]
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("CreatedAt")]
        [Display(Name = "Дата создания", Description = "Дата и время создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Навигационные свойства

        /// <summary>
        /// Список доступов пользователей к этому отделу
        /// </summary>
        public virtual ICollection<UserDepartmentAccess> UserDepartmentAccesses { get; set; }

        /// <summary>
        /// Список складов этого отдела
        /// </summary>
        public virtual ICollection<Warehouse> Warehouses { get; set; }

        /// <summary>
        /// Список заявок этого отдела
        /// </summary>
        public virtual ICollection<ShiftRequest> ShiftRequests { get; set; }

        /// <summary>
        /// Список записей транспортной программы этого отдела
        /// </summary>
        public virtual ICollection<TransportProgram> TransportPrograms { get; set; }

        /// <summary>
        /// Список пользователей, для которых этот отдел является отделом по умолчанию
        /// </summary>
        public virtual ICollection<User> Users { get; set; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Department()
        {
            UserDepartmentAccesses = new HashSet<UserDepartmentAccess>();
            Warehouses = new HashSet<Warehouse>();
            ShiftRequests = new HashSet<ShiftRequest>();
            TransportPrograms = new HashSet<TransportProgram>();
            Users = new HashSet<User>();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Возвращает строковое представление отдела
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Id})";
        }

        #endregion
    }
}