using Microsoft.EntityFrameworkCore;
using OrderingSpecialEquipment.Models;
using OrderingSpecialEquipment.Services.Interfaces;
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
        public ShiftRequestService(IDatabaseService databaseService, IAuthorizationService authorizationService)
            : base(databaseService, authorizationService)
        {
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Получение заявок по дате и смене
        /// </summary>
        public async Task<List<ShiftRequest>> GetByDateAndShiftAsync(DateTime date, int shift)
        {
            return await _databaseService.Context.ShiftRequests
                .Include(sr => sr.Equipment)
                .Include(sr => sr.Warehouse)
                .Include(sr => sr.Area)
                .Include(sr => sr.LicensePlate)
                .Include(sr => sr.LessorOrganization)
                .Include(sr => sr.Department)
                .Include(sr => sr.CreatedByUser)
                .Where(sr => sr.Date.Date == date.Date && sr.Shift == shift)
                .OrderBy(sr => sr.Warehouse.Name)
                .ThenBy(sr => sr.Equipment.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Получение заявок с пагинацией
        /// </summary>
        public async Task<List<ShiftRequest>> GetPagedAsync(int page, int pageSize,
            Expression<Func<ShiftRequest, bool>> predicate = null,
            Func<IQueryable<ShiftRequest>, IOrderedQueryable<ShiftRequest>> orderBy = null)
        {
            var query = _databaseService.Context.ShiftRequests
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
            var tp = await _databaseService.Context.TransportProgram
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
            var request = await _databaseService.Context.ShiftRequests
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

            await _databaseService.Context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Разблокировка заявки
        /// </summary>
        public async Task<bool> UnlockRequestAsync(int requestKey)
        {
            var request = await _databaseService.Context.ShiftRequests
                .FirstOrDefaultAsync(sr => sr.Key == requestKey);

            if (request == null)
                return false;

            request.LockedByUserId = null;
            request.LockedAt = null;
            request.IsBlocked = false;

            await _databaseService.Context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Очистка устаревших блокировок
        /// </summary>
        public async Task CleanupExpiredLocksAsync()
        {
            var expiredRequests = await _databaseService.Context.ShiftRequests
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
                await _databaseService.Context.SaveChangesAsync();
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

            // Проверяем блокировку
            var existing = await _databaseService.Context.ShiftRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(sr => sr.Key == entity.Key);

            if (existing != null && existing.IsBlocked &&
                existing.LockedByUserId != _authorizationService.IsSystemAdmin.ToString())
            {
                throw new InvalidOperationException("Запись заблокирована другим пользователем");
            }

            return await base.UpdateAsync(entity);
        }

        /// <summary>
        /// Удаление заявки
        /// </summary>
        public override async Task<bool> DeleteAsync(ShiftRequest entity)
        {
            // Проверяем права на запись
            if (!_authorizationService.CanWriteTable("ShiftRequests"))
                throw new UnauthorizedAccessException("Нет прав на удаление заявок");

            return await base.DeleteAsync(entity);
        }

        #endregion
    }
}