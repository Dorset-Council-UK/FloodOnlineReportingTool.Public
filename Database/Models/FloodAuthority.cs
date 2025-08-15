namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// Authorities and agencies responsible for managing, mitigating, and responding to flood risks and incidents
/// </summary>
public record FloodAuthority
{
    public FloodAuthority(Guid id, string authorityName, string authorityDescription)
    {
        Id = id;
        AuthorityName = authorityName;
        AuthorityDescription = authorityDescription;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string AuthorityName { get; init; } = "";
    public string AuthorityDescription { get; init; } = "";
}
