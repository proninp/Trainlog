namespace Trainlog.Domain.Entities.Base;

public interface ISoftDeletable<TKey> : IEntity<TKey>
    where TKey : struct, IEquatable<TKey>
{
    public DateTime? DeletedAt { get; protected internal set; }

    public void Delete();

    public void Restore();
}