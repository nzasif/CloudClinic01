namespace CloudClinic.Data.DbSets
{
    public class RemovedUser
    {
        public Guid RemovedUserId { get; set; }
        public DateTime RemovalDate { get; set; }
        public string? RemovalCause { get; set; }
        public required string UserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}
