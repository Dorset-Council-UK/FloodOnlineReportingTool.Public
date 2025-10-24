﻿using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Contact;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace FloodOnlineReportingTool.Public.Services;

internal sealed class FortEmailSender(ILogger<FortEmailSender> logger, IServiceScopeFactory serviceScopeFactory) : IEmailSender<FortUser>
{
    public async Task SendConfirmationLinkAsync(FortUser user, string email, string confirmationLink)
    {
        logger.LogDebug("Creating confirmation link 'message' for user {UserId}", user.Id);
        logger.LogDebug("Confirmation link: {ConfirmationLink}", confirmationLink);

        var message = new ConfirmationLinkSent(user.Id, email, confirmationLink);
        await PublishMessage(message).ConfigureAwait(false);

        logger.LogInformation("Confirmation link 'message' published for user {UserId}", user.Id);
    }

    public async Task SendPasswordResetCodeAsync(FortUser user, string email, string resetCode)
    {
        logger.LogDebug("Creating password reset code 'message' for user {UserId}", user.Id);
        logger.LogDebug("Reset code: {ResetCode}", resetCode);

        var message = new PasswordResetCodeSent(user.Id, email, resetCode);
        await PublishMessage(message).ConfigureAwait(false);

        logger.LogInformation("Password reset code 'message' published for user {UserId}", user.Id);
    }

    public async Task SendPasswordResetLinkAsync(FortUser user, string email, string resetLink)
    {
        logger.LogDebug("Creating password reset link 'message' for user {UserId}", user.Id);
        logger.LogDebug("Reset link: {ResetLink}", resetLink);

        var message = new PasswordResetLinkSent(user.Id, email, resetLink);
        await PublishMessage(message).ConfigureAwait(false);

        logger.LogInformation("Password reset link 'message' published for user {UserId}", user.Id);
    }

    private async Task PublishMessage<T>(T message) where T : class
    {
        using var scope = serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PublicDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        await publishEndpoint.Publish(message).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}
