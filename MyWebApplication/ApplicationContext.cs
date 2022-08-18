using Microsoft.EntityFrameworkCore;

namespace MyWebApplication
{
    class ApplicationContext : DbContext
    {
        public DbSet<Person> Users { get; set; } = null!;
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().HasData(
             new() { Name = "Tom", Age = 35, Email = "tom@gmail.com", Password = "12345" },
             new() { Name = "Bob", Age = 41, Email = "bob@gmail.com", Password = "55555" },
             new() { Name = "Sam", Age = 24, Email = "sam@gmail.com", Password = "22222" }
            );
        }
    }
}
