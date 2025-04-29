

using System.Security;

namespace MVC.Models
{
    public class ApplicationConfiguration
    {
        public ApplicationConfiguration()
        { 
            FontSize = 10;
            FontColor = "blue";
            WelcomePhrase = "Bienvenue sur le merveilleux site !!!";
            Sentinel = 0;
            BlobConnectionString = "UseDevelopmentStorage=true";
            UnvalidatedBlob = "Unvalidated";
            ValidatedBlob = "Validated";
            ServiceBusConnectionString = "";
            ServiceBusQueue1Name = "imageresizemessage";
            ServiceBusQueue2Name = "contentsafetymessage";
        }

        public int FontSize { get; set; }
        public required string FontColor { get; set; }
        public required string WelcomePhrase { get; set; }
        public required int Sentinel { get; set; }

        // Connection String pour le Blob qui sera acquis du AppConfig
        public required string BlobConnectionString { get; set; }
        // Nom du blob pour les images non valider
        public required string UnvalidatedBlob { get; set; } 
        // Nom du blob pour les images valider
        public required string ValidatedBlob { get; set; }
        // Nom du blob pour synchroniser les Event Hub
        public required string SynchroBlob { get; set; }

        // Connection String pour le Service Bus qui sera acquis du AppConfig
        public required string ServiceBusConnectionString { get; set; }
        // Nom de la Queue pour Redimensionner les Images
        public required string ServiceBusQueue1Name { get; set; }
        // Nom de la Queue pour valider les text/images
        public required string ServiceBusQueue2Name { get; set; }
        // Nom du Consumer Group pour l'Event Hub
        public required string EventHubConsumerName { get; set; }
    }
}
