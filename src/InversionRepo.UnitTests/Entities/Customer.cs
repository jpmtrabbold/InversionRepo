namespace InversionRepo.UnitTests.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; }
        public int AddressId { get; set; }
        public Address Address { get; set; }
    }
}