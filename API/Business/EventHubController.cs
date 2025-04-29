// Event Hub
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

// Serialization
using System.Text;
using System.Text.Json;

using MVC.Models;

namespace MVC.Business
{
    public class EventHubController
    {
        // Ajout du logger
        private ILogger _logger;

        // Event Hub producer
        private EventHubProducerClient eventHubProducerClient { get; }

        public EventHubController(ILogger logger, string EventHubKey, string EventHubName) 
        {
            _logger = logger;

            // create client options
            EventHubProducerClientOptions _producerClientOptions = new EventHubProducerClientOptions
            {
                RetryOptions =
                {
                    MaximumRetries = 5,
                    Delay = TimeSpan.FromSeconds(2),
                    MaximumDelay = TimeSpan.FromSeconds(30),
                    Mode = EventHubsRetryMode.Exponential
                }
            };

            // Nom de l'event hub 
            eventHubProducerClient = new EventHubProducerClient(EventHubKey, EventHubName, _producerClientOptions);
        }

        public async Task SendEvent(Event message)
        {
            try
            {
                // Ajout du log
                _logger.LogInformation(string.Format("{0} : Sending Event : {1} {2}", DateTime.Now, message.ItemType, message.Action));

                EventDataBatch eventDataBatch = await eventHubProducerClient.CreateBatchAsync();

                EventData eventData = new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)))
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                eventDataBatch.TryAdd(eventData);

                await eventHubProducerClient.SendAsync(eventDataBatch);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending Event : " + message.Id, ex);
            }


        }
    }
}
