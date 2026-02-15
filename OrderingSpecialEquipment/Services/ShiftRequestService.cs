using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
using OrderingSpecialEquipment.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OrderingSpecialEquipment.Services
{
    /// <summary>
    /// Сервис для работы с заявками
    /// </summary>
    public class ShiftRequestService : DataService<ShiftRequest>, IShiftRequestService
    {
        #region Конструктор

        /// <summary>
        /// Конструктор сервиса заявок
        /// </summary>
        public ShiftRequestService(IDbContextFactory contextFactory, IAuthorizationService authorizationService)
            : base(contextFactory, authorizationService)
        {
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Получение заявок по дате и смене
        /// </summary>
        public async Task<List<ShiftRequest>> GetByDateAndShiftAsync(DateTime date, int shift)
        {
            using var context = _contextFactory.CreateDbContext();

            // Приводим дату к UTC для поиска
            DateTime utcDate = date.ToUniversalTime().Date;

            return await context.ShiftRequests
                .Include(sr => sr.Equipment)
                .Include(sr => sr.Warehouse)
                .Include(sr => sr.Area)
                .Include(sr => sr.LicensePlate)
                .Include(sr => sr.LessorOrganization)
                .Include(sr => sr.Department)
                .Include(sr => sr.CreatedByUser)
                .Where(sr => sr.Date.Date == utcDate && sr.Shift == shift)
                .OrderBy(sr => sr.Warehouse.Name)
                .ThenBy(sr => sr.Equipment.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Получение заявок с пагинацией
        /// </summary>
        public async Task<List<ShiftRequest>> GetPagedAsync(int page, int pageSize,
            Expression<Func<ShiftRequest, bool>>? predicate = null,
            Func<IQueryable<ShiftRequest>, IOrderedQueryable<ShiftRequest>>? orderBy = null)
        {
            using var context = _contextFactory.CreateDbContext();

            var query = context.ShiftRequests
                .Include(sr => sr.Equipment)
                .Include(sr => sr.Warehouse)
                .Include(sr => sr.Area)
                .Include(sr => sr.LicensePlate)
                .Include(sr => sr.LessorOrganization)
                .Include(sr => sr.Department)
                .Include(sr => sr.CreatedByUser)
                .AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            else
            {
                query = query.OrderByDescending(sr => sr.Date)
                             .ThenBy(sr => sr.Shift)
                             .ThenBy(sr => sr.Warehouse.Name);
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Получение количества часов по транспортной программе для техники
        /// </summary>
        public async Task<decimal> GetTransportProgramHoursAsync(string departmentId, string equipmentId, int year, int month)
        {
            using var context = _contextFactory.CreateDbContext();

            var tp = await context.TransportProgram
                .FirstOrDefaultAsync(t => t.DepartmentId == departmentId &&
                                          t.EquipmentId == equipmentId &&
                                          t.Year == year);

            if (tp == null)
                return 0;

            return tp.GetHoursByMonth(month);
        }

        /// <summary>
        /// Блокировка заявки для редактирования
        /// </summary>
        public async Task<bool> LockRequestAsync(int requestKey, string userId)
        {
            using var context = _contextFactory.CreateDbContext();

            var request = await context.ShiftRequests
                .FirstOrDefaultAsync(sr => sr.Key == requestKey);

            if (request == null)
                return false;

            // Проверяем, не заблокирована ли уже запись другим пользователем
            if (request.LockedByUserId != null && request.LockedByUserId != userId)
            {
                // Проверяем, не истекла ли блокировка
                if (request.LockedAt.HasValue &&
                    request.LockedAt.Value.AddMinutes(30) > DateTime.UtcNow)
                {
                    return false; // Заблокировано другим пользователем
                }
            }

            request.LockedByUserId = userId;
            request.LockedAt = DateTime.UtcNow;
            request.IsBlocked = true;

            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Разблокировка заявки
        /// </summary>
        public async Task<bool> UnlockRequestAsync(int requestKey)
        {
            using var context = _contextFactory.CreateDbContext();

            var request = await context.ShiftRequests
                .FirstOrDefaultAsync(sr => sr.Key == requestKey);

            if (request == null)
                return false;

            request.LockedByUserId = null;
            request.LockedAt = null;
            request.IsBlocked = false;

            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Очистка устаревших блокировок
        /// </summary>
        public async Task CleanupExpiredLocksAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            var expiredRequests = await context.ShiftRequests
                .Where(sr => sr.LockedAt.HasValue &&
                             sr.LockedAt.Value < DateTime.UtcNow.AddMinutes(-30))
                .ToListAsync();

            foreach (var request in expiredRequests)
            {
                request.LockedByUserId = null;
                request.LockedAt = null;
                request.IsBlocked = false;
            }

            if (expiredRequests.Any())
            {
                await context.SaveChangesAsync();
            }
        }

        #endregion

        #region Переопределенные методы

        /// <summary>
        /// Добавление заявки с проверкой зависимостей
        /// </summary>
        public override async Task<ShiftRequest> AddAsync(ShiftRequest entity)
        {
            // Проверяем права на запись
            if (!_authorizationService.CanWriteTable("ShiftRequests"))
                throw new UnauthorizedAccessException("Нет прав на создание заявок");

            // Проверяем доступ к отделу
            if (entity.DepartmentId != null &&
                !await _authorizationService.HasDepartmentAccessAsync(entity.DepartmentId))
                throw new UnauthorizedAccessException("Нет доступа к указанному отделу");

            // Проверяем доступ к складу
            if (!await _authorizationService.HasWarehouseAccessAsync(entity.WarehouseId))
                throw new UnauthorizedAccessException("Нет доступа к указанному складу");

            // Убеждаемся, что дата в UTC
            entity.Date = entity.Date.ToUniversalTime();
            entity.CreatedAt = DateTime.UtcNow;

            return await base.AddAsync(entity);
        }

        /// <summary>
        /// Обновление заявки
        /// </summary>
        public override async Task<ShiftRequest> UpdateAsync(ShiftRequest entity)
        {
            // Проверяем права на запись
            if (!_authorizationService.CanWriteTable("ShiftRequests"))
                throw new UnauthorizedAccessException("Нет прав на редактирование заявок");

            using var context = _contextFactory.CreateDbContext();

            // Проверяем блокировку
            var existing = await context.ShiftRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(sr => sr.Key == entity.Key);

            if (existing != null && existing.IsBlocked &&
                existing.LockedByUserId != _authorizationService.IsSystemAdmin.ToString())
            {
                throw new InvalidOperationException("Запись заблокирована другим пользователем");
            }

            // Убеждаемся, что дата в UTC
            entity.Date = entity.Date.ToUniversalTime();

            return await base.UpdateAsync(entity);
        }

        /// <summary>
        /// Удаление заявки
        /// </summary>
        public override async Task<bool> DeleteAsync(object id)
        {
            // Проверяем права на запись
            if (!_authorizationService.CanWriteTable("ShiftRequests"))
                throw new UnauthorizedAccessException("Нет прав на удаление заявок");

            return await base.DeleteAsync(id);
        }

        #endregion
    }
}