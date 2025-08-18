using Bogus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Database.DbContexts;
using MassTransit;
using System.Globalization;

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
        var faker = new Faker("en_GB");

        var message = new EligibilityCheckCreated(
            faker.Random.Uuid(),
            faker.Random.AlphaNumeric(8).ToUpper(CultureInfo.CurrentCulture),
            faker.Date.PastOffset(),
            faker.Random.Long(1, 9999999999),
            faker.Random.Double(0, 700000),
            faker.Random.Double(0, 1300000),
            faker.Date.PastOffset(),
            faker.Random.Int(1, 72),
            faker.Random.Bool(),
            faker.Random.Bool(),
            faker.Random.Int(0, 5)
        );

        await _publishEndpoint.Publish(message, ct).ConfigureAwait(false);
        await _context.SaveChangesAsync(ct).ConfigureAwait(false);
#endif
    }
}
