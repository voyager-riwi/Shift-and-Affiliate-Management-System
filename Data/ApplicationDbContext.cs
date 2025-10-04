using Microsoft.EntityFrameworkCore;
using System_EPS.Models;

namespace System_EPS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Cada DbSet representa una tabla en tu base de datos.
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Affiliate> Affiliates { get; set; }
        public DbSet<ServiceDesk> ServiceDesks { get; set; }
    }
}