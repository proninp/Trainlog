namespace Trainlog.Domain.Entities.Base;

public abstract class SoftDeletableEntity : BaseEntity, ISoftDeletable<Guid>
{
    public DateTime? DeletedAt { get; set; }

    public virtual void Delete()
    {
        if (DeletedAt.HasValue) return;
        Touch();
        DeletedAt = DateTime.UtcNow;
    }

    public virtual void Restore()
    {
        if (!DeletedAt.HasValue) return;
        UpdatedAt = DateTime.UtcNow;
        DeletedAt = null;
    }
}