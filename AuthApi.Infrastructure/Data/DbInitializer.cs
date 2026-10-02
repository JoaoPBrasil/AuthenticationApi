using AuthApi.Domain.Entities;
using AuthApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        AppDbContext context,
        string adminEmail,
        string adminPassword)
    {
        await context.Database.MigrateAsync();

        var adminExists = await context.Users
            .AnyAsync(x => x.Email == adminEmail);

        if (adminExists)
            return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(
            adminPassword);

        var admin = new User(
            "System Admin",
            adminEmail,
            passwordHash,
            UserRole.Admin);

        await context.Users.AddAsync(admin);

        await context.SaveChangesAsync();
    }
}