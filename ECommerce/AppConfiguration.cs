using ECommerce.Utiltie;

namespace ECommerce
{
    public static class AppConfiguration
    {
        public static void RegisterConfig(this IServiceCollection services , string connection)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(option =>
            {
                option.User.RequireUniqueEmail = true;
                option.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<ApplicationDBContext>()
                .AddDefaultTokenProviders();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IRepositroy<Brand>, Repositroy<Brand>>();
            services.AddScoped<IRepositroy<Categroy>, Repositroy<Categroy>>();
            services.AddScoped<IRepositroy<Product>, Repositroy<Product>>();
            services.AddScoped<IRepositroy<ProductSubImage>, Repositroy<ProductSubImage>>();
            services.AddScoped<IRepositroy<ProductColor>, Repositroy<ProductColor>>();
            services.AddDbContext<ApplicationDBContext>(option =>
            {
                option.UseSqlServer(connection);
            });

        }
    }
}
