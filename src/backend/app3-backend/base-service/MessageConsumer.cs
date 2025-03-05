using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared;

namespace BaseService;

public class App3MessageConsumer(ILogger<App3MessageConsumer> logger) : IConsumer<Message>
{
    private readonly ILogger<App3MessageConsumer> _logger = logger;

    public Task Consume(ConsumeContext<Message> context)
    {
        _logger.LogInformation("App3 Base Service Message Received: {JsonData}", context.Message.JsonData);
        return Task.CompletedTask;
    }
}
