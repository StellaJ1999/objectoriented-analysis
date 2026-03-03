namespace Domain.Common.Abstractions;

public interface ICreate<TModel>
{
    Task<bool> CreateAsync(TModel model, CancellationToken ct);
}
