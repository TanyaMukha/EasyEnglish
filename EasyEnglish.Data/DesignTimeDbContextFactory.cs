
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyEnglish.Data;

/// <summary>
/// Factory для створення DbContext під час design-time операцій (міграції)
/// Цей клас використовується EF Tools для створення міграцій
/// </summary>
public class EasyEnglishDbContextFactory : IDesignTimeDbContextFactory<EasyEnglishDbContext>
{
    public EasyEnglishDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EasyEnglishDbContext>();

        // Використовуємо простий connection string для міграцій
        // Шлях буде відносний до папки EasyEnglish.Data
        optionsBuilder.UseSqlite("Data Source=../EasyEnglish.db");

        // Вимикаємо логування для чистого виводу
        //optionsBuilder.EnableSensitiveDataLogging(false);
        //optionsBuilder.EnableDetailedErrors(false);

        return new EasyEnglishDbContext(optionsBuilder.Options);
    }
}