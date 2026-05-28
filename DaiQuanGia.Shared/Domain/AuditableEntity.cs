namespace DaiQuanGia.Shared.Domain;

public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; protected set; } = DateTimeOffset.UtcNow;

    protected void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
