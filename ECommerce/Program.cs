using ECommerce.Repositries;

namespace ECommerce
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<IRepositroy<Brand>, Repositroy<Brand>>();
            builder.Services.AddScoped<IRepositroy<Categroy>, Repositroy<Categroy>>();
            builder.Services.AddScoped<IRepositroy<Product>, Repositroy<Product>>();
            builder.Services.AddScoped<IRepositroy<ProductSubImage>, Repositroy<ProductSubImage>>();
            builder.Services.AddScoped<IRepositroy<ProductColor>, Repositroy<ProductColor>>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
