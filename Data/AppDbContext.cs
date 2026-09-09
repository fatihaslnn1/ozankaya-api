using Microsoft.EntityFrameworkCore;
using ozankaya_api.Models;

namespace ozankaya_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Barber> Barbers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<BlockedSlot> BlockedSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Barber>().HasData(
                new Barber { Id = 1, Name = "Ozan KAYA", Title = "Saç & Sakal Tasarım Uzmanı", Chair = "1. Koltuk" },
                new Barber { Id = 2, Name = "Efehan SAYICI", Title = "Saç & Bakım Uzmanı", Chair = "2. Koltuk" }
            );
        }
    }
}