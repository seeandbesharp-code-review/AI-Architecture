using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;

namespace WebApiShop.DataMigration
{
    public static class PasswordHashMigration
    {
        public static async Task RunAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiShopContext>();
            var passwordServices = scope.ServiceProvider.GetRequiredService<IPasswordServices>();

            var users = await context.Users.ToListAsync();

            foreach (var user in users)
            {
                // If password does NOT start with $2a$ it is plain-text - hash it
                if (!user.Password.StartsWith("$2a$"))
                {
                    user.Password = passwordServices.HashPassword(user.Password);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
