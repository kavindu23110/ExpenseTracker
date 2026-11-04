namespace ExpenseTracker.Domain.Bills;

/// <summary>Languages a bill can be written in.</summary>
public static class BillLanguage
{
    public const string English = "en";
    public const string Sinhala = "si";

    /// <summary>True for a supported language code.</summary>
    public static bool IsValid(string? lang) => lang is English or Sinhala;
}
