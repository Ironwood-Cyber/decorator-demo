using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared;

namespace BaseService;

public class App1MessageConsumer(ILogger<App1MessageConsumer> logger) : IConsumer<Message>
{
    private readonly ILogger<App1MessageConsumer> _logger = logger;

    public Task Consume(ConsumeContext<Message> context)
    {
        _logger.LogInformation("App1 Base Service Message Received: {JsonData}", context.Message.JsonData);
        return Task.CompletedTask;
    }
}
