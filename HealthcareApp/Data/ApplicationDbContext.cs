using Microsoft.EntityFrameworkCore;

namespace HealthcareApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Models.User> Users { get; set; }

        public DbSet<Models.Doctor> Doctors { get; set; }
        public DbSet<Models.Appointments> Appointments { get; set; }

        public DbSet<Models.Patient> Patients { get; set; }
    }
}
