namespace ExpenseTracker.Domain.Bills;

/// <summary>Recognises accepted upload types from their leading bytes, not their claimed type.</summary>
public static class BillFileType
{
    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";
    public const string Webp = "image/webp";
    public const string Pdf = "application/pdf";

    /// <summary>Returns the detected content type, or null when the file is not accepted.</summary>
    public static string? Detect(ReadOnlySpan<byte> d)
    {
        if (d.StartsWith<byte>([0xFF, 0xD8, 0xFF])) return Jpeg;
        if (d.StartsWith<byte>([0x89, 0x50, 0x4E, 0x47])) return Png;
        if (d.StartsWith("%PDF"u8)) return Pdf;
        if (d.Length >= 12 && d.StartsWith("RIFF"u8) && d[8..12].SequenceEqual("WEBP"u8)) return Webp;
        return null;
    }
}
