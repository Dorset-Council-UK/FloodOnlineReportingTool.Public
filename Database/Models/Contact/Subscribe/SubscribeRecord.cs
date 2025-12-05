using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloodOnlineReportingTool.Database.Models.Contact.Subscribe;


public record SubscribeRecord
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public string ContactName { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public bool IsEmailVerified { get; set; } = false;
    public bool IsSubscribed { get; set; } = false;
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset RedactionDate { get; init; }
    public int? VerificationCode { get; set; }
    public DateTimeOffset? VerificationExpiryUtc { get; set; }

    // Optional foreign key to ContactRecord
    public Guid? ContactRecordId { get; set; }
    public ContactRecord? ContactRecord { get; set; }

}
