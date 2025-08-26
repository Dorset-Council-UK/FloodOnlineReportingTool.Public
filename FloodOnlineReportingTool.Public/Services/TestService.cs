using Bogus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Database.DbContexts;
using MassTransit;

namespace FloodOnlineReportingTool.Public.Services;

public sealed class TestService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly PublicDbContext _context;

    public TestService(IPublishEndpoint publishEndpoint, PublicDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
    }

    internal async Task TestMessage(CancellationToken ct)
    {
#if DEBUG
        // Make a test message for the EligibilityCheckCreated event
        Randomizer.Seed = new Random(232589734);

        var eligibilityCheckOrganisationFaker = new Faker<EligibilityCheckOrganisation>("en_GB")
            .CustomInstantiator(f => new(
                f.Random.Uuid(),
                f.Company.CompanyName(),
                f.Random.Uuid(),
                f.Commerce.ProductName()
            ));

        var eligibilityCheckCreatedFaker = new Faker<EligibilityCheckCreated>("en_GB")
            .CustomInstantiator(f => new(
                f.Random.Uuid(),
                f.Random.Hexadecimal(8, "").ToUpperInvariant(),
                f.Date.RecentOffset(),
                f.Random.Long(1, 9999999999),
                f.Random.Double(0, 700000),
                f.Random.Double(0, 1300000),
                f.Date.PastOffset(),
                f.Random.Int(1, 72),
                f.Random.Bool(),
                f.Random.Bool(),
                f.Random.Int(0, 5),
                eligibilityCheckOrganisationFaker.GenerateBetween<EligibilityCheckOrganisation>(1, 3)
            ));

        var message = eligibilityCheckCreatedFaker.Generate();

        await _publishEndpoint.Publish(message, ct).ConfigureAwait(false);
        await _context.SaveChangesAsync(ct).ConfigureAwait(false);
#endif
    }
}
