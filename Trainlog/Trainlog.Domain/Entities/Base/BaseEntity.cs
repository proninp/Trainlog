namespace Trainlog.Domain.Entities.Base;

public abstract class BaseEntity : IAuditable<Guid>
{
    public Guid Id { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;

        return ReferenceEquals(this, other) || Id.Equals(other.Id);
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    protected void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}