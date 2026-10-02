namespace AuthApi.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }

    public string Token { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public bool IsExpired =>
        DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked =>
        RevokedAt != null;

    public bool IsActive =>
        !IsExpired && !IsRevoked;

    public RefreshToken(
        string token,
        DateTime expiresAt,
        Guid userId)
    {
        Id = Guid.NewGuid();
        Token = token;
        ExpiresAt = expiresAt;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}