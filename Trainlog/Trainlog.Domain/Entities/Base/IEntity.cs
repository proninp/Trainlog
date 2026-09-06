namespace Trainlog.Domain.Entities.Base;

#pragma warning disable CA1040 // Avoid empty interfaces
/// <summary>
///     Маркерный интерфейс для сущности, хранимой в базе.
/// </summary>
public interface IEntity;

public interface IEntity<TKey> : IEntity
    where TKey : struct, IEquatable<TKey>
{
    TKey Id { get; init; }
}