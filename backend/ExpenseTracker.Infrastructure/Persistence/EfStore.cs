using ExpenseTracker.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence;

/// <inheritdoc />
public sealed class EfStore<T>(AppDbContext db) : IStore<T> where T : class
{
    /// <inheritdoc />
    public Task<List<T>> ListAsync(CancellationToken ct) => db.Set<T>().ToListAsync(ct);

    /// <inheritdoc />
    public async Task<T?> FindAsync(object key, CancellationToken ct) => await db.Set<T>().FindAsync([key], ct);

    /// <inheritdoc />
    public void Add(T item) => db.Set<T>().Add(item);

    /// <inheritdoc />
    public void Remove(T item) => db.Set<T>().Remove(item);

    /// <inheritdoc />
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
