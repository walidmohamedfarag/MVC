

namespace ECommerce.DataAccess
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Categroy> Categroys { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductSubImage> ProductSubImages { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //    => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=ECommerce;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductSubImageConfiguration).Assembly);
        }
    }
}
