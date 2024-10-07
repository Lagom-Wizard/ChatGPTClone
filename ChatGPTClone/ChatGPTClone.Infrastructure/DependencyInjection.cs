using ChatGPTClone.Application.Common.Interfaces;
using ChatGPTClone.Domain.Identity;
using ChatGPTClone.Infrastructure.Identity;
using ChatGPTClone.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ChatGPTClone.Infrastructure
{
    // Bu sınıf, uygulama altyapısının bağımlılık enjeksiyonunu yapılandırır
    public static class DependencyInjection
    {
        // Bu metod, altyapı servislerini IServiceCollection'a ekler
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Veritabanı bağlantı dizesini yapılandırmadan alır
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // ApplicationDbContext'i PostgreSQL ile kullanmak üzere yapılandırır
            services.AddDbContext<ApplicationDbContext>(opt => opt.UseNpgsql(connectionString));

            services.AddScoped<ApplicationDbContext, ApplicationDbContext>();

            return services;
        }
    }
}