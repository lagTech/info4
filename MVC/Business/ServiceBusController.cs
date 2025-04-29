using MVC.Models;
using Microsoft.Extensions.Options;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace MVC.Business
{
    public class ServiceBusController
    {
        private ApplicationConfiguration _applicationConfiguration { get; }
        private ServiceBusClientOptions _serviceBusClientOptions { get; }

        private const int Delay = 2;
        private const int MaxDelay = 30;
        private const int MaxRetry = 5;
        private const int SendOffset = 5;

        public ServiceBusController(IOptionsSnapshot<ApplicationConfiguration> options)
        {
            _applicationConfiguration = options.Value;

            // Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
            // If you use the default AmqpTcp, ensure that ports 5671 and 5672 are open.
            // Service Bus Retry options
            // https://learn.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific

            _serviceBusClientOptions = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Delay = TimeSpan.FromSeconds(Delay),
                    MaxDelay = TimeSpan.FromSeconds(MaxDelay),
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = MaxRetry,
                },
                TransportType = ServiceBusTransportType.AmqpWebSockets,
                ConnectionIdleTimeout = TimeSpan.FromSeconds(MaxDelay)   //Défault = 1 minutes
            };
        }

        private async Task SendMessageAsync(string queueName, ServiceBusMessage message, int Defer = 0)
        {
            await using ServiceBusClient serviceBusClient = new ServiceBusClient(_applicationConfiguration.ServiceBusConnectionString, _serviceBusClientOptions);
            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(queueName);

            if (Defer != 0)
            {
                DateTimeOffset scheduleTime = DateTimeOffset.UtcNow.AddSeconds(SendOffset);
                await serviceBusSender.ScheduleMessageAsync(message, scheduleTime);
            }
            else 
                await serviceBusSender.SendMessageAsync(message);
        }

        public async Task SendImageToResize(Guid imageName, Guid Id)
        {
            Console.WriteLine("Envoi d'un message pour ImageResize : " + DateTime.Now.ToString());
            ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(new Tuple<Guid,Guid> (imageName,Id)));
            await SendMessageAsync(_applicationConfiguration.ServiceBusQueue1Name, message);
        }

        public async Task SendContentTextToValidation(string text, Guid CommentId, Guid PostId)
        {
            Console.WriteLine("Envoi d'un message pour Text Content Validation : " + DateTime.Now.ToString());
            ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(new ContentTypeValidation(ContentType.Text, text, CommentId, PostId)));
            await SendMessageAsync(_applicationConfiguration.ServiceBusQueue2Name, message);
        }

        public async Task SendContentImageToValidation(Guid imageName, Guid PostId)
        {
            Console.WriteLine("Envoi d'un message pour Image Content Validation : " + DateTime.Now.ToString());
            ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(new ContentTypeValidation(ContentType.Image, imageName.ToString(), PostId)));

            // Messsage planifier dans 5 seconds, le but étant de laisser le temps au Resize de passé avant.
            // Ceci n'est vraiment pas un design idéal.

            await SendMessageAsync(_applicationConfiguration.ServiceBusQueue2Name, message, 1);
        }
    }
}
