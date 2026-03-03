namespace Domain.Common.Abstractions.Base;

public interface IRepositoryBase<TModel, TId> :
    ICreate<TModel>,
    IUpdate<TId, TModel>,
    IDelete<TId>,
    IRead<TModel, TId>
{

}