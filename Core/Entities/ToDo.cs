using NodaTime;

namespace Core.Entities
{
    public class ToDo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? DeletedAtUtc { get; set; }

        public long TenantId { get; set; }

        public ToDo(string name, IClock clock, long tenantId)
        {
            Name = name;
            CreatedAtUtc = clock.GetCurrentInstant().ToDateTimeUtc();
            TenantId = tenantId;
        }

        public ToDo()
        {
        }
    }
}