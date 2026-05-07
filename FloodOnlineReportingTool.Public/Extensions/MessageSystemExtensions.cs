namespace Microsoft.AspNetCore.Builder;

internal static class MessageSystemExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Add the message system. The Public project only needs to publish messages, not consume them
        /// </summary>
        internal IHostApplicationBuilder AddMessageSystem()
        {
            var connectionString = builder.Configuration.GetConnectionString("service-bus");
            var useMessaging = !string.IsNullOrWhiteSpace(connectionString);

            // TODO: add and setup Azure Service Bus messaging, using outbox pattern
            return builder;
        }
    }
}
