using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace OrderingSpecialEquipment.Data
{
    /// <summary>
    /// Конвертер для преобразования DateTime в UTC перед сохранением в БД
    /// </summary>
    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(
                v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    /// <summary>
    /// Конвертер для nullable DateTime
    /// </summary>
    public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public UtcNullableDateTimeConverter()
            : base(
                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
        {
        }
    }
}