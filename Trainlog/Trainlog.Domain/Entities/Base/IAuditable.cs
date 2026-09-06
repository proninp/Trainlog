namespace Trainlog.Domain.Entities.Base;

public interface IAuditable<TKey> : IEntity<TKey>
    where TKey : struct, IEquatable<TKey>
{
    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; protected internal set; }
}