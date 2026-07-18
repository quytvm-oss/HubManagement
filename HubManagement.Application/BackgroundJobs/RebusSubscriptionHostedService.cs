using HubManagement.Application.IntegrationEvents;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;

namespace HubManagement.Application.BackgroundJobs;

public sealed class RebusSubscriptionHostedService(
    IBus bus) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await bus.Subscribe<SendConfirmationEmailEvent>();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}