namespace ExpenseTracker.Domain.Bills;

/// <summary>The original photo or PDF of a bill.</summary>
public sealed class BillFile
{
    private BillFile() { ContentType = string.Empty; Data = []; }

    /// <summary>Holds a file whose type has been checked by BillFileType.</summary>
    public BillFile(string contentType, byte[] data)
    {
        ContentType = contentType;
        Data = data;
    }

    public int Id { get; private set; }
    public int BillId { get; private set; }
    public string ContentType { get; private set; }
    public byte[] Data { get; private set; }
}
