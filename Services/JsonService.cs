using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Echelon.Bot.Services
{
    public class JsonService : BackgroundService
    {
        private readonly IConfigurationRoot configuration;
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageWriter messageWriter;
        private readonly string path;
        private readonly string jsonFile;

        public JsonService(IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IMessageWriter messageWriter)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.messageWriter = messageWriter;
            this.path = configuration["Spotify-JsonPath"];
            this.jsonFile = Path.Join(path, configuration["Spotify-JsonFile"]);
            
            VerifyDirectories();
            messageWriter.Write("Started JsonService");
        }

        public void VerifyDirectories()
        {
            messageWriter.Write("Verifying directories");
            try
            {
                var path = configuration["Spotify-JsonPath"];
                var jsonFile = Path.Join(path, configuration["Spotify-JsonFile"]);
                if (!Directory.Exists(path))
                {                    
                    Directory.CreateDirectory(path);
                    messageWriter.Write($"{path} did not exist. Created");
                } 
                else
                {
                    messageWriter.Write($"Directory exists {path}");
                }

                if (!File.Exists(jsonFile))
                {
                    File.Create(jsonFile);
                    messageWriter.Write($"{jsonFile} did not exist. Created");
                }
                else
                {
                    messageWriter.Write($"File exists {jsonFile}");
                }
                messageWriter.Write("Directories verified");
            }
            catch (Exception ex)
            {
                messageWriter.Write(ex.Message);
            }
        }

        public async Task<Dictionary<string, SpotifyItem>> GetItems()
        {
            var fileContent = await File.ReadAllTextAsync(jsonFile);
            if (string.IsNullOrEmpty(fileContent))
                return new Dictionary<string, SpotifyItem>();

            var value = JsonSerializer.Deserialize<Dictionary<string, SpotifyItem>>(fileContent);
            if (value == null)
                value = new Dictionary<string, SpotifyItem>();
            
            return value;
        }
        
        public async Task SaveItems(Dictionary<string, SpotifyItem> items)
        {
            var fileContent = JsonSerializer.Serialize(items, new JsonSerializerOptions{ WriteIndented = true });
            await File.WriteAllTextAsync(jsonFile, fileContent);
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

    }
}

