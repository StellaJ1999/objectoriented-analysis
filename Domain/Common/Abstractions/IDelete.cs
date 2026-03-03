namespace Domain.Common.Abstractions;

public interface IDelete<TId>
{
    Task<bool> DeleteAsync(TId id, CancellationToken ct);

}