using ExceptionHandling.Database.Entities;
using ExceptionHandling.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ExceptionHandling.Database;

public class AuditableInterceptor(ICurrentSessionProvider sessionProvider) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result,
        CancellationToken cancellationToken)
    {
        var context = eventData.Context!;
        var entries = context.ChangeTracker.Entries<IAuditableEntity>();

        var userId = sessionProvider.GetUserId();
        var currentUser = userId?.ToString() ?? "system";

        // Set auditable properties with the actual user
        SetAuditableProperties(context, currentUser);

        var auditTrails = HandleAuditingBeforeSaveChanges(context, userId);

        if (auditTrails.Any())
        {
            context.Set<AuditTrail>().AddRange(auditTrails);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetAuditableProperties(DbContext context, string currentUser)
    {

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                entry.Entity.CreatedBy = currentUser;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                entry.Entity.UpdatedBy = currentUser;
            }
        }
    }

    private List<AuditTrail> HandleAuditingBeforeSaveChanges(DbContext context, Guid? userId)
    {
        var entries = context.ChangeTracker.Entries<IAuditableEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

        var auditTrails = new List<AuditTrail>();

        foreach (var entry in entries)
        {
          
            foreach (var prop in entry.Properties)
            {
                var audit = new AuditTrail
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DateUtc = DateTime.UtcNow,
                    EntityName = entry.Entity.GetType().Name,
                    PrimaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString(),
                    ChangedColumn = prop.Metadata.Name
                };

                switch (entry.State)
                {
                    case EntityState.Added:
                        audit.TrailType = TrailType.Create;
                        audit.OldValue = null;
                        audit.NewValue = prop.CurrentValue?.ToString();
                        break;
                    case EntityState.Modified:
                        audit.TrailType = TrailType.Update;
                        audit.OldValue = prop.OriginalValue?.ToString();
                        audit.NewValue = prop.CurrentValue?.ToString();
                        break;
                    case EntityState.Deleted:
                        audit.TrailType = TrailType.Delete;
                        audit.OldValue = prop.OriginalValue?.ToString();
                        audit.NewValue = null;
                        break;
                }
                if (audit.TrailType != TrailType.None)
                    auditTrails.Add(audit);
            }
        }
        return auditTrails;
    }
}
