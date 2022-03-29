using Discord.Commands;
using Echelon.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Echelon.Bot.Services
{
    [Group("youtube")]
    public class YoutubeModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider serviceProvider;

        public YoutubeModule(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Command("play")]
        public async Task PlayAsync(string url)
        {
            if (!Context.IsMessageFromMusicChannel() && !Context.IsMessageFromDevelopmentChannel())
                return;

            url = url.ToLower();
            var isValidLink = url.StartsWith("https://") || url.StartsWith("http://");
            var isYoutube = url.Contains("youtube.com/watch") || url.Contains("youtu.be/");
            if (!isValidLink && !isYoutube)
                return;

            var logger = serviceProvider.GetRequiredService<IMessageWriter>();
            try
            {
                var configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
                var filename = configuration.GetValue<string>("playlistfile")
                    .StripBackslashes();

                await File.WriteAllTextAsync(filename, url);
                await ReplyAsync($"Playing");
            }
            catch (Exception ex)
            {
                logger.Write(ex.Message);
            }
        }
    }
}

