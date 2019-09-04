using InversionRepo.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace InversionRepo.UnitTests.Entities
{
    public abstract class BaseEntity : IEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    }
}
