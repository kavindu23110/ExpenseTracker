namespace ExpenseTracker.Domain.Household;

/// <summary>A household member who can sign in; all members share the same data.</summary>
public sealed class AppUser
{
    public const int MinPasswordLength = 8;

    private AppUser() { Name = PasswordHash = string.Empty; }

    /// <summary>Creates a member with an already hashed password.</summary>
    public AppUser(string name, string passwordHash, bool isOwner)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 50) throw new DomainException("Enter a name of up to 50 characters.");
        Name = name.Trim();
        PasswordHash = passwordHash;
        IsOwner = isOwner;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsOwner { get; private set; }

    /// <summary>Checks a password meets the minimum before it is hashed.</summary>
    public static void CheckPassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength) throw new DomainException($"Use a password of at least {MinPasswordLength} characters.");
    }

    /// <summary>Stores a stronger hash of the same password.</summary>
    public void ChangeHash(string passwordHash) => PasswordHash = passwordHash;
}
