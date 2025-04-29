using Worker_DB;

//Event Hub
using Azure.Messaging.EventHubs;
using System.Collections.Concurrent;

//Azure Identity
using Azure.Identity;

//App Config
using Azure.Data.AppConfiguration;

//KeyVault
using Azure.Security.KeyVault.Secrets;

//Application Insight
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

//Database
using Microsoft.EntityFrameworkCore;

using MVC.Business;
using MVC.Data;

namespace Worker_DB
{
    public class Worker_DB
    {
        public static void Main(string[] args)
        {

            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();

            // Code différent pour le Azure.Data.AppConfiguration
            string AppConfigEndPoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")!;

            // Option pour le credential recu des variables d'environement.
            DefaultAzureCredential defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeEnvironmentCredential = false
            });

            // Création du Client App Config
            ConfigurationClient appConfigClient = new ConfigurationClient(new Uri(AppConfigEndPoint), defaultAzureCredential);

            // Création du Client Key Vault
            ConfigurationSetting endpointKeyVault = appConfigClient.GetConfigurationSetting("Endpoints:KeyVault");
            ConfigurationSetting storageBlobContainerName3 = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:SynchroBlob");
            ConfigurationSetting EventHubHubName = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:EventHubName");
            ConfigurationSetting EventHubConsumerGroupName = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:EventHubConsumerName");
            ConfigurationSetting CosmosDBdatabaseName = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:CosmosDBdatabaseName");

            SecretClient keyVaultClient = new SecretClient(new Uri(endpointKeyVault.Value), defaultAzureCredential);

            KeyVaultSecret blobKeyVault = keyVaultClient.GetSecret("ConnectionStringBlob");
            KeyVaultSecret EventHubKey = keyVaultClient.GetSecret("ConnectionStringEventHub");
            KeyVaultSecret applicationinsightKeyVault = keyVaultClient.GetSecret("ConnectionStringApplicationInsight");
            KeyVaultSecret CosmosDB = keyVaultClient.GetSecret("ConnectionStringCosmosDB");

            // Ajout de secrets a la configuration du worker
            builder.Services.Configure<WorkerOptions>(options =>
            {
                options.EventHubKey = EventHubKey.Value;
                options.BlobStorageKey = blobKeyVault.Value;
                options.storageBlobContainerName3 = storageBlobContainerName3.Value;
                options.EventHubHubName = EventHubHubName.Value;
                options.EventHubConsumerGroupName = EventHubConsumerGroupName.Value;
            });

            // Ajout des bases de données
            builder.Services.AddDbContext<ApplicationDbContextNoSQL>(options =>
                options.UseCosmos(
                        connectionString: CosmosDB.Value,
                        databaseName: CosmosDBdatabaseName.Value,
                        cosmosOptionsAction: options =>
                        {
                            options.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Gateway);
                        }));

            builder.Services.AddScoped<IRepository, EFRepositoryNoSQL>();


            // Application Insight trace/log/metrics
            // https://medium.com/@chuck.beasley/how-to-instrument-a-net-5537ea851763
            builder.Services.AddSingleton<ITelemetryInitializer>(new CustomTelemetryInitializer("Worker_DB", Environment.GetEnvironmentVariable("HOSTNAME")!));
            builder.Services.AddLogging(logging =>
            {
                logging.AddApplicationInsights(
                    configureTelemetryConfiguration: (config) =>
                    config.ConnectionString = applicationinsightKeyVault.Value,
                    configureApplicationInsightsLoggerOptions: (options) => { }
                    );
                logging.AddFilter<ApplicationInsightsLoggerProvider>("Worker_DB", LogLevel.Trace);

            });
            builder.Services.AddApplicationInsightsTelemetryWorkerService();
            builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.EnableSqlCommandTextInstrumentation = true;
                o.ConnectionString = applicationinsightKeyVault.Value;
            });

            var host = builder.Build();

            host.Run();
        }
    }
}

public class WorkerOptions
{
    public required string EventHubKey { get; set; }
    public required string BlobStorageKey { get; set; }
    public required string storageBlobContainerName3 { get; set; }
    public required string EventHubHubName { get; set; }
    public required string EventHubConsumerGroupName { get; set; }
}