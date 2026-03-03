namespace Domain.Common.Abstractions;

public interface IRead<TModel, TId>
{
    Task<TModel?> GetByIdAsync(TId id, CancellationToken ct);
    Task<IReadOnlyList<TModel>> GetAllAsync(CancellationToken ct);
}