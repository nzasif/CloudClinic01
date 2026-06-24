using CloudClinic.Data.DbSets;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CloudClinic.Data
{
    public class CloudClinicDb : IdentityDbContext<AppUser>
    {
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<RemovedUser> RemovedUsers { get; set; }
        public DbSet<DrDetail> DrDetails { get; set; }
        public DbSet<DrPracTime> DrPracTimes { get; set; }
        public DbSet<UnVerifiedDr> UnVerifiedDrs { get; set; }
        public DbSet<DrHoliday> DrHolidays { get; set; }
        public DbSet<StaffDetail> StaffDetails { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PatLabTest> PatLabTests { get; set; }
        public DbSet<PatXray> PatXrays { get; set; }
        public DbSet<PatDrug> PatDrugs { get; set; }
        public DbSet<PatReport> PatReports { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageReply> MessageReplies { get; set; }

        public CloudClinicDb(DbContextOptions<CloudClinicDb> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Table mapping for Identity
            builder.Entity<AppUser>(b => b.ToTable("AppUsers"));
            builder.Entity<IdentityRole>(b => b.ToTable("Roles"));
            builder.Entity<IdentityUserRole<string>>(b => b.ToTable("UserRoles"));
            builder.Entity<IdentityUserToken<string>>(b => b.ToTable("UserTokens"));
            builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("UserLogins"));
            builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("UserClaims"));
            builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("RoleClaims"));
            builder.Entity<AppUser>().Property(p => p.Id).HasColumnName("UserId");

            // --- CASCADE CYCLE FIXES ---

            // StaffDetail: Restrict DrDetails, Cascade User
            builder.Entity<StaffDetail>().HasOne(t => t.AppUser).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<StaffDetail>().HasOne(t => t.DrDetails).WithMany().HasForeignKey(k => k.DrId).OnDelete(DeleteBehavior.Restrict);

            // Messages: Restrict DrDetails, Cascade User
            builder.Entity<Message>().HasOne(m => m.DrDetail).WithMany().HasForeignKey(fk => fk.DrId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Message>().HasOne(m => m.AppUser).WithMany().HasForeignKey(fk => fk.PatId).OnDelete(DeleteBehavior.Cascade);

            // Reviews: Restrict DrDetails, Cascade User
            builder.Entity<Review>().HasOne(c => c.DrDetail).WithMany().HasForeignKey(fk => fk.DrId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Review>().HasOne(c => c.AppUser).WithMany().HasForeignKey(fk => fk.PatId).OnDelete(DeleteBehavior.Cascade);

            // Appointments & PatReports: Restrict both to prevent loops
            builder.Entity<Appointment>().HasOne(ap => ap.DrDetail).WithMany().HasForeignKey(fk => fk.DrId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Appointment>().HasOne(pat => pat.AppUser).WithMany().HasForeignKey(fk => fk.PatId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PatReport>().HasOne(dr => dr.DrDetail).WithMany().HasForeignKey(fk => fk.DrId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PatReport>().HasOne(pat => pat.AppUser).WithMany().HasForeignKey(fk => fk.PatId).OnDelete(DeleteBehavior.Restrict);

            // Other entity constraints
            builder.Entity<RemovedUser>().HasOne(u => u.AppUser).WithMany().HasForeignKey(fk => fk.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<DrDetail>().HasOne(t => t.AppUser).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<UnVerifiedDr>().HasOne(dr => dr.DrDetail).WithMany().HasForeignKey(fk => fk.DrId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<MessageReply>().HasOne(r => r.Message).WithMany().HasForeignKey(fk => fk.MessageId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}