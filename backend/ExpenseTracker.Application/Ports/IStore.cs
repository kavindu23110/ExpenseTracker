namespace ExpenseTracker.Application.Ports;

/// <summary>Simple storage for a household record type; changes apply on save.</summary>
public interface IStore<T> where T : class
{
    /// <summary>Loads every record.</summary>
    Task<List<T>> ListAsync(CancellationToken ct);

    /// <summary>Finds a record by key.</summary>
    Task<T?> FindAsync(object key, CancellationToken ct);

    /// <summary>Adds a record.</summary>
    void Add(T item);

    /// <summary>Removes a record.</summary>
    void Remove(T item);

    /// <summary>Saves all pending changes together.</summary>
    Task SaveAsync(CancellationToken ct);
}
