using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Interfaces.Fields;
using Microsoft.EntityFrameworkCore;

namespace EasyPeasy.Data;

/// <summary>
/// EF Core context for the EasyPeasy SQLite database. Schema is defined entirely through data
/// annotations on the <c>EasyPeasy.Core.Entities.*</c> classes (<c>[Table]</c>, <c>[Column]</c>,
/// <c>[MaxLength]</c>, <c>[ForeignKey]</c>) — there is no <c>OnModelCreating</c> override here, and no
/// fluent-API configuration (indexes, unique constraints) beyond what those attributes imply. For the
/// current schema/entity-relationship diagrams see <c>EasyPeasy.Docs/Diagrams/database.mdpuml</c>
/// and <c>entities.mdpuml</c>.
/// </summary>
public class EasyPeasyDbContext : DbContext
{
    public EasyPeasyDbContext(DbContextOptions<EasyPeasyDbContext> options)
        : base(options)
    {
    }

    public DbSet<CourseEntity> Courses => this.Set<CourseEntity>();

    public DbSet<SubjectEntity> Subjects => this.Set<SubjectEntity>();

    public DbSet<WordEntity> Words => this.Set<WordEntity>();

    public DbSet<ExampleEntity> Examples => this.Set<ExampleEntity>();

    public DbSet<IrregularFormEntity> IrregularForms => this.Set<IrregularFormEntity>();

    public DbSet<StudyCardEntity> StudyCards => this.Set<StudyCardEntity>();

    public DbSet<TestCardEntity> TestCards => this.Set<TestCardEntity>();

    public DbSet<UnitEntity> Units => this.Set<UnitEntity>();

    /// <inheritdoc/>
    /// <remarks>Also stamps <see cref="IAuditInfo"/> timestamps via <see cref="UpdateAuditInfo"/> before saving.</remarks>
    public override int SaveChanges()
    {
        UpdateAuditInfo();
        return base.SaveChanges();
    }

    /// <inheritdoc/>
    /// <remarks>Also stamps <see cref="IAuditInfo"/> timestamps via <see cref="UpdateAuditInfo"/> before saving.</remarks>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Sets <see cref="IAuditInfo.CreatedAt"/> on every newly-added tracked entity and
    /// <see cref="IAuditInfo.UpdatedAt"/> on every modified one, for entities implementing
    /// <see cref="IAuditInfo"/>. Runs once per <see cref="SaveChanges()"/>/<see cref="SaveChangesAsync"/>
    /// call, before the actual write — callers never need to set these timestamps by hand.
    /// </summary>
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