using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InversionRepo.UnitTests.Entities
{
    public abstract class BaseEntityWithDate : BaseEntity
    {
        public BaseEntityWithDate()
        {
            if (!CreationDate.HasValue)
                CreationDate = DateTime.Now;

            LastModificationDate = DateTime.Now;
        }

        public DateTime? CreationDate { get; set; } 

        public DateTime? LastModificationDate { get; set; }
    }
}
