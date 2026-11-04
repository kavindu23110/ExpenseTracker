using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Household;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Application.Services;

/// <summary>Household members and their passwords.</summary>
public sealed class AuthService(IStore<AppUser> users, IPasswordHasher<AppUser> hasher)
{
    /// <summary>True until the first account exists.</summary>
    public async Task<bool> NeedsSetupAsync(CancellationToken ct) => (await users.ListAsync(ct)).Count == 0;

    /// <summary>Creates the owner; only allowed while there are no accounts.</summary>
    public async Task<MemberDto> SetupAsync(CredentialsRequest r, CancellationToken ct)
    {
        if (!await NeedsSetupAsync(ct)) throw new DomainException("The first account already exists. Sign in instead.");
        return await CreateAsync(r, isOwner: true, ct);
    }

    /// <summary>Returns the member when the name and password match.</summary>
    public async Task<MemberDto?> SignInAsync(CredentialsRequest r, CancellationToken ct)
    {
        var user = await FindByNameAsync(r.Name, ct);
        if (user is null || string.IsNullOrEmpty(r.Password)) return null;
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, r.Password);
        if (result == PasswordVerificationResult.Failed) return null;
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.ChangeHash(hasher.HashPassword(user, r.Password));
            await users.SaveAsync(ct);
        }

        return ToDto(user);
    }

    /// <summary>Finds a member by id.</summary>
    public async Task<MemberDto?> FindAsync(int id, CancellationToken ct) => await users.FindAsync(id, ct) is { } user ? ToDto(user) : null;

    /// <summary>Lists the household.</summary>
    public async Task<IReadOnlyList<MemberDto>> ListAsync(CancellationToken ct) =>
        (await users.ListAsync(ct)).OrderByDescending(u => u.IsOwner).ThenBy(u => u.Name).Select(ToDto).ToList();

    /// <summary>Adds a member.</summary>
    public Task<MemberDto> AddMemberAsync(CredentialsRequest r, CancellationToken ct) => CreateAsync(r, isOwner: false, ct);

    /// <summary>Removes a member; the owner cannot be removed. False when missing.</summary>
    public async Task<bool> RemoveMemberAsync(int id, CancellationToken ct)
    {
        if (await users.FindAsync(id, ct) is not { } user) return false;
        if (user.IsOwner) throw new DomainException("The owner cannot be removed.");
        users.Remove(user);
        await users.SaveAsync(ct);
        return true;
    }

    /// <summary>Links a member's WhatsApp number, refusing one another member uses. False when missing.</summary>
    public async Task<bool> LinkWhatsAppAsync(int id, string? number, CancellationToken ct)
    {
        if (await users.FindAsync(id, ct) is not { } user) return false;
        user.LinkWhatsApp(number);
        if (user.WhatsAppNumber is not null && (await users.ListAsync(ct)).Any(u => u.Id != id && u.WhatsAppNumber == user.WhatsAppNumber)) throw new DomainException("Another member already uses that number.");
        await users.SaveAsync(ct);
        return true;
    }

    private async Task<MemberDto> CreateAsync(CredentialsRequest r, bool isOwner, CancellationToken ct)
    {
        AppUser.CheckPassword(r.Password);
        if (await FindByNameAsync(r.Name, ct) is not null) throw new DomainException("That name is already used.");
        var user = new AppUser(r.Name ?? "", "", isOwner);
        user.ChangeHash(hasher.HashPassword(user, r.Password!));
        users.Add(user);
        await users.SaveAsync(ct);
        return ToDto(user);
    }

    private async Task<AppUser?> FindByNameAsync(string? name, CancellationToken ct) =>
        (await users.ListAsync(ct)).FirstOrDefault(u => string.Equals(u.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));

    private static MemberDto ToDto(AppUser u) => new(u.Id, u.Name, u.IsOwner, u.WhatsAppNumber);
}
