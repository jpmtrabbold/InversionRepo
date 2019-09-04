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
            return entities.Paginate(listRequest.PageSize, listRequest.PageNumber);
        }
        
        public static IQueryable<T> Paginate<T>(this IQueryable<T> entities, int? pageSize, int? pageNumber)
        {
            var skip = SkipNumber(pageNumber, pageSize);
            var take = TakeNumber(pageSize);
            var count = entities.Count();
            if ((skip + take) > count)
                return entities.Skip(skip);
            else
                return entities.Skip(skip).Take(take);
        }

        static int SkipNumber(int? pageNumber, int? pageSize)
        {
            return ((pageNumber ?? 1) - 1) * (pageSize ?? 10);
        }

        static int TakeNumber(int? pageSize)
        {
            return pageSize ?? 10;
        }
    }
}
