using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GrowMate.Repositories.Extensions
{
    public static class QueryablePagingExtensions
    {
        public static async Task<PageResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query, int page, int pageSize, CancellationToken ct) where T : class
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalItems = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PageResult<T>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        public static async Task<PageResult<TDest>> ToPagedResultAsync<TSource, TDest>(
            this IQueryable<TSource> query, int page, int pageSize,
            Expression<Func<TSource, TDest>> selector, CancellationToken ct)
            where TSource : class
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalItems = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync(ct);

            return new PageResult<TDest>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        public static Task<PageResult<TDest>> ToPagedResultAsync<TSource, TDest>(
            this IQueryable<TSource> query, int page, int pageSize,
            Expression<Func<TSource, TDest>> selector)
            where TSource : class
            => ToPagedResultAsync(query, page, pageSize, selector, CancellationToken.None);
    }
}
