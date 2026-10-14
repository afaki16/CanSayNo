namespace CanSayNo.Domain.Entities.Hayirci;

public class DailyUsage : BaseAuditableEntity
{
    // Composite PK: (UserId, Date). Inherited int Id is ignored in configuration.
    public int UserId { get; set; }            // FK -> User.Id (int)
    public User User { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int SessionsCount { get; set; }
    public int TokensUsed { get; set; }
}
