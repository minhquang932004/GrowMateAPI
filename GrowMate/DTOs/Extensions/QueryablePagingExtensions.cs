using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GrowMate.DTOs.Extensions
{
    public static class QueryablePagingExtensions
    {
        public static async Task<PageResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query, int page, int pageSize) where T : class
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<T>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        // Overload có projection sang DTO để query vẫn chạy trên DB
        //Dùng khi bạn muốn trả về DTO khác với entity (map ngay trên query).
        //Chỉ select các cột cần thiết => nhẹ hơn, nhanh hơn, giảm data trả về.
        public static async Task<PageResult<TDest>> ToPagedResultAsync<TSource, TDest>(
            this IQueryable<TSource> query,
            int page, int pageSize,
            Expression<Func<TSource, TDest>> selector)
            where TSource : class
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync();

            return new PageResult<TDest>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
    }
}
