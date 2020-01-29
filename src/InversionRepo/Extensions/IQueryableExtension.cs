using InversionRepo.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace InversionRepo.Extensions
{
    public static class IQueryableExtension
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> entities, IListRequest listRequest)
        {
            if (listRequest.PageSize.HasValue)
                return entities.Paginate(listRequest.PageSize, listRequest.PageNumber);

            return entities;
        }
        
        public static IQueryable<T> Paginate<T>(this IQueryable<T> entities, int? pageSize, int? pageNumber)
        {
            var skip = (pageNumber ?? 0) * (pageSize ?? 10);
            var take = pageSize ?? 10;
            return entities.Skip(skip).Take(take);
        }
    }
}