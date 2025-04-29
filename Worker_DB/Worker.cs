//Event Hub
using Azure.Messaging.EventHubs;

// Event processor 
using Azure.Messaging.EventHubs.Processor;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

// Blob
using Azure.Storage.Blobs;
using Azure.Core;
using MVC.Data;
using System.Text;
using MVC.Models;
using System.Text.Json;

namespace Worker_DB
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private WorkerOptions _options;
        private EventProcessorClient _eventProcessorClient;
        private ConcurrentQueue<EventData> _messageQueue;

        private SemaphoreSlim _semaphore;

        private const int Delay = 2;
        private const int MaxDelay = 30;
        private const int MaxRetry = 5;
        private const int ConcurentJobLimit = 1;

        private IRepository _repo;


        public Worker(ILogger<Worker> logger, IOptions<WorkerOptions> options, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _options = options.Value;

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContextNoSQL>();
                context.Database.EnsureCreatedAsync();
            }

            _repo = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IRepository>();

            // Blob ...
            BlobClientOptions blobClientOptions = new BlobClientOptions
            {
                Retry = {
                        Delay = TimeSpan.FromSeconds(Delay),     //The delay between retry attempts for a fixed approach or the delay on which to base
                                                             //calculations for a backoff-based approach
                        MaxRetries = MaxRetry,                      //The maximum number of retry attempts before giving up
                        Mode = RetryMode.Exponential,        //The approach to use for calculating retry delays
                        MaxDelay = TimeSpan.FromSeconds(MaxDelay)  //The maximum permissible delay between retry attempts
                        },
            };

            // Azure Event Hub
            EventProcessorClientOptions eventProcessorClientOptions = new EventProcessorClientOptions
            {
                RetryOptions = new EventHubsRetryOptions
                {
                    Delay = TimeSpan.FromSeconds(Delay),
                    MaximumDelay = TimeSpan.FromSeconds(MaxDelay),
                    Mode = EventHubsRetryMode.Exponential,
                    MaximumRetries = MaxRetry,
                },
                ConnectionOptions = new EventHubConnectionOptions
                {
                    TransportType = EventHubsTransportType.AmqpWebSockets,
                    ConnectionIdleTimeout = TimeSpan.FromSeconds(MaxDelay),
                }
            };
            
            // Blob pour la synchro
            BlobContainerClient _blobServiceClientEvent = new BlobContainerClient(_options.BlobStorageKey, _options.storageBlobContainerName3);

            // nom du consummer group + nom du event hub apres la Key
            _eventProcessorClient = new EventProcessorClient(_blobServiceClientEvent, _options.EventHubConsumerGroupName, _options.EventHubKey + ";EntityPath=" + _options.EventHubHubName, eventProcessorClientOptions);

            _messageQueue = new ConcurrentQueue<EventData>();

            _eventProcessorClient.ProcessEventAsync += MessageHandler;
            _eventProcessorClient.ProcessErrorAsync += ErrorHandler;

            _semaphore = new SemaphoreSlim(ConcurentJobLimit); // Limit le nombre de job concurente.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Start the Event Processor
            await _eventProcessorClient.StartProcessingAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }

            // Stop the Event Processor before exiting
            await _eventProcessorClient.StopProcessingAsync(stoppingToken);
        }


        private async Task MessageHandler(ProcessEventArgs args)
        {
            await _semaphore.WaitAsync();
            _messageQueue.Enqueue(args.Data);

            _ = ProcessMessagesAsync(args);
        }
        private async Task ProcessMessagesAsync(ProcessEventArgs args)
        {
            try
            {
                // Deserialize the message
                byte[] body = args.Data.EventBody.ToArray();
                string json = Encoding.UTF8.GetString(body);
                Event? message = JsonSerializer.Deserialize<Event>(json);

                if (message != null)
                {
                    _logger.LogInformation(string.Format("{0} : Receiving Event : {1} {2}", DateTime.Now,message.ItemType,message.Action));

                    // Processing the message in the repo ...
                    switch (message.ItemType)
                    {
                        case ItemType.Post:
                            switch (message.Action)
                            {
                                case MVC.Models.Action.Create:
                                    await _repo.Add(message.Post!);
                                    break;

                                case MVC.Models.Action.Like:
                                    await _repo.IncrementPostLike((Guid)message.Id!);
                                    break;

                                case MVC.Models.Action.Dislike:
                                    await _repo.IncrementPostDislike((Guid)message.Id!);
                                    break;

                                case MVC.Models.Action.Approve:
                                    await _repo.ApprovePost((Guid)message.Id!, true, message.Uri!);
                                    break;

                                case MVC.Models.Action.Rejected:
                                    await _repo.ApprovePost((Guid)message.Id!, false, message.Uri!);
                                    break;
                            }
                            break;

                        case ItemType.Comment:
                            switch (message.Action)
                            {
                                case MVC.Models.Action.Create:
                                    await _repo.AddComments(message.Comment!);
                                    break;

                                case MVC.Models.Action.Like:
                                    await _repo.IncrementCommentLike((Guid)message.Id!);
                                    break;

                                case MVC.Models.Action.Dislike:
                                    await _repo.IncrementCommentDislike((Guid)message.Id!);
                                    break;

                                case MVC.Models.Action.Approve:
                                    await _repo.ApproveComment((Guid)message.Id!, true);
                                    break;

                                case MVC.Models.Action.Rejected:
                                    await _repo.ApproveComment((Guid)message.Id!, false);
                                    break;
                            }
                            break;
                    }

                }

                // Complete the message
                await args.UpdateCheckpointAsync();
                await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message: {args.Data.MessageId}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error processing messages.");
            return Task.CompletedTask;
        }
    }
}
