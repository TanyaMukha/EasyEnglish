using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Fields;
using Microsoft.EntityFrameworkCore;

namespace EasyEnglish.Data;

public class EasyEnglishDbContext : DbContext
{
    public EasyEnglishDbContext(DbContextOptions<EasyEnglishDbContext> options)
        : base(options)
    {
    }

    public DbSet<CourseEntity> Courses => this.Set<CourseEntity>();

    public DbSet<WordEntity> Words => this.Set<WordEntity>();

    public DbSet<ExampleEntity> Examples => this.Set<ExampleEntity>();

    public DbSet<IrregularFormEntity> IrregularForms => this.Set<IrregularFormEntity>();

    public DbSet<StudyCardEntity> StudyCards => this.Set<StudyCardEntity>();

    public DbSet<TestCardEntity> TestCards => this.Set<TestCardEntity>();

    public DbSet<UnitEntity> Units => this.Set<UnitEntity>();

    public override int SaveChanges()
    {
        UpdateAuditInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditInfo()
    {
        var entries = ChangeTracker.Entries<IAuditInfo>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }
}