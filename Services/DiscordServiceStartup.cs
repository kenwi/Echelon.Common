using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Echelon.Bot.Services
{
    public class DiscordServiceStartup : BackgroundService
    {
        private readonly DiscordService discordService;

        public DiscordServiceStartup(IServiceProvider serviceProvider)
        {
            this.discordService = serviceProvider.GetRequiredService<DiscordService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await discordService.StartAsync(stoppingToken);
        }
    }
}

