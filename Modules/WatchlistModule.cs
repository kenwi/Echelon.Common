using Discord.Commands;
using Discord.WebSocket;
using Echelon.Bot.Components;
using Echelon.Bot.Providers;
using Echelon.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Echelon.Bot.Services
{
    [Group("watch")]
    public class WatchlistModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider serviceProvider;

        public WatchlistModule(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        //[Command("list")]
        //public async Task ListAsync()
        //{

        //}

        [Command("add")]
        public async Task AddAsync(string username)
        {
            if (!Context.IsMessageFromModeratorChannel() && !Context.IsMessageFromDevelopmentChannel())
                return;

            if (!Context.IsUserInModeratorRole())
                return;

            var configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
            var filename = configuration.GetValue<string>("watchlistFile");
            var fileContent = await File.ReadAllLinesAsync(filename);
            var isAlreadyAdded = fileContent.Any(line => line.ToLower().Contains(username.ToLower()));

            if (isAlreadyAdded)
            {
                var userAlreadyOnWatchlistMessage = configuration.GetValue<string>("userAlreadyOnWatchlistMessage")
                    .Replace("{username}", username);
                await ReplyAsync(userAlreadyOnWatchlistMessage);
                return;
            }

            var provider = serviceProvider.GetRequiredService<FreakUserIdProvider>();
            provider.SetUsername(username);
            var document = await provider.GetAsync();
            var id = document.GetUserId();

            if(id == null)
            {
                var couldNotFindUserMessage = configuration.GetValue<string>("couldNotFindUserMessage")
                    .Replace("{username}", username);
                await ReplyAsync(couldNotFindUserMessage);
                return;
            }
            await File.AppendAllTextAsync(filename, $"{username}\t{id}{Environment.NewLine}");

            var addedUserToWatchlistMessage = configuration.GetValue<string>("addedUserToWatchlistMessage")
                .Replace("{username}", username)
                .Replace("{id}", id);
            await ReplyAsync(addedUserToWatchlistMessage);

//#if RELEASE
//            return;
//#endif
//            var queue = serviceProvider.GetRequiredService<QueueComponent>();
//            queue.QueueMessage(new Models.OutboundMessage
//            {
//                MessageType = Models.OutboundMessageType.Freak
//            });
        }
    }
}

