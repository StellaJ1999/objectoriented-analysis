using Domain.Common.Abstractions.Base;
using Infrastructure.Persistense.EFCore.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistense.EFCore.Repositories;

/// Bas-klass för alla repositories med gemensamma CRUD-operationer
public abstract class RepositoryBase<TModel, TId> : IRepositoryBase<TModel, TId>
    where TModel : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TModel> _dbSet;

    protected RepositoryBase(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TModel>();
    }

    /// Skapar en ny entity i databasen
    public virtual async Task<bool> CreateAsync(TModel model, CancellationToken ct = default)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        try
        {
            await _dbSet.AddAsync(model, ct);
            var result = await _context.SaveChangesAsync(ct);
            return result > 0;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    /// Hämtar en entity baserat på ID
    public virtual async Task<TModel?> GetByIdAsync(TId id, CancellationToken ct = default)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        return await _dbSet.FindAsync(new object[] { id }, ct);
    }

    /// Hämtar alla entities
    public virtual async Task<IReadOnlyList<TModel>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// Uppdaterar en befintlig entity
    public virtual async Task<bool> UpdateAsync(TId id, TModel model, CancellationToken ct = default)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        if (model == null)
            throw new ArgumentNullException(nameof(model));

        try
        {
            _dbSet.Update(model);
            var result = await _context.SaveChangesAsync(ct);
            return result > 0;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    /// Tar bort en entity baserat på ID
    public virtual async Task<bool> DeleteAsync(TId id, CancellationToken ct = default)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        var entity = await GetByIdAsync(id, ct);
        if (entity == null)
            return false;

        try
        {
            _dbSet.Remove(entity);
            var result = await _context.SaveChangesAsync(ct);
            return result > 0;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}