using InversionRepo.UnitTests.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace InversionRepo.UnitTests.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static int HasData<TEntity>(this ModelBuilder mb, TEntity entity) where TEntity : class
        {
            if (entity is BaseEntityWithDate entityWithDate)
            {
                entityWithDate.CreationDate = new DateTime(2019, 07, 01);
                entityWithDate.LastModificationDate = new DateTime(2019, 07, 01);
            }
            mb.Entity<TEntity>().HasData(entity);

            if (entity is BaseEntity baseEntity)
            {
                return baseEntity.Id;
            }

            return 0;
        }
    }
}
