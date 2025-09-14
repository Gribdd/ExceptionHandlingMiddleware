namespace ExceptionHandling.Database.Entities;

public class AuditTrail
{
    public required Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public TrailType TrailType { get; set; }

    public DateTime DateUtc { get; set; }

    public required string EntityName { get; set; }

    public string? PrimaryKey { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public string? ChangedColumn { get; set; }
}




public enum TrailType : byte
{
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 3
}