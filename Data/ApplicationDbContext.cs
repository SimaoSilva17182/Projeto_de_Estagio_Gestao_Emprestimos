using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectEstagio.Models;

namespace ProjectEstágio.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Veicule> Veicules { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<RoomRequest> RoomRequests { get; set; }
        public DbSet<EquipmentRequest> EquipmentRequests { get; set; }
        public DbSet<VeiculeRequest> VeiculeRequests { get; set; }
            
    }
}
