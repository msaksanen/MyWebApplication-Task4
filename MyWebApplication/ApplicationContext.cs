using Microsoft.EntityFrameworkCore;

namespace MyWebApplication
{
    class ApplicationContext : DbContext
    {
        public DbSet<Person> Users { get; set; } = null!;
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=usersdb;Username=postgres;Password=0456769");
        }
    }
}
