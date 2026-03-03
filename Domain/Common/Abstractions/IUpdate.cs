namespace Domain.Common.Abstractions;

public interface IUpdate<TId, TModel>
{
    Task<bool> UpdateAsync(TId id, TModel model, CancellationToken ct);
}
