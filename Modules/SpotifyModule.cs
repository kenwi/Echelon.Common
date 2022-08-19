using Discord.Commands;
using Echelon.Bot.Models;
using Echelon.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Echelon.Bot.Services
{
    [Group("spotify")]
    public class SpotifyModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageWriter messageWriter;

        public SpotifyModule(IServiceProvider serviceProvider, IMessageWriter messageWriter)
        {            
            this.serviceProvider = serviceProvider;
            this.messageWriter = messageWriter;
            messageWriter.Write("SpotifyModule Started");
        }

        private async void PrintHelp(ulong channelId)
        {
            await ReplyAsync("Please provide a channel-id and playlist-id or link to a playlist"
                + "```!spotify register <discord-channel-id> <spotify-playlist-id-or-link>```"
                + "For this specific channel example: "
                + $"```!spotify register {channelId} https://open.spotify.com/playlist/6tWMX0Hf2An9rP9vwAKNmP?si=8ac2538637684e3c```");
        }

        [Command("register")]
        public async Task Register(string channelId = "", string playlistId = "")
        {
            if (!Context.IsUserInModeratorRole())
                return;

            try
            {
                if (channelId.Length < 10)
                {
                    PrintHelp(Context.Channel.Id);
                    return;
                }

                if(playlistId.StartsWith("https://open.spotify.com/playlist/"))
                {
                    playlistId = playlistId.Split("/")
                        .Last()
                        .Split("?")
                        .First();
                    
                    if(string.IsNullOrEmpty(playlistId))
                    {
                        await ReplyAsync($"Invalid spotify playlist link");
                        PrintHelp(Context.Channel.Id);
                        return;
                    }
                }
                else
                {
                    await ReplyAsync($"Invalid spotify playlist link");
                    PrintHelp(Context.Channel.Id);
                    return;
                }

                var jsonService = serviceProvider.GetRequiredService<JsonService>();
                var items = await jsonService.GetItems();

                if (!items.ContainsKey(channelId))
                {
                    var callbackService = serviceProvider.GetRequiredService<SpotifyCallbackService>();
                    var uri = callbackService.CreateLoginRequestUri();
                    var code = uri.Split("code=").Last();
                    callbackService.Start();

                    items.Add(channelId, new SpotifyItem
                    {
                        ChannelId = channelId,
                        PlaylistId = playlistId,
                        OwnerId = Context.User.Id.ToString(),
                        OwnerName = Context.User.Username,
                        ServerName = Context.Guild.Name
                    });
                    await jsonService.SaveItems(items);
                    await ReplyAsync($"Registered channel **{channelId}** with playlist **{playlistId}**. Please verify your Spotify account with the link received by PM");
                                        
                    var discordService = serviceProvider.GetRequiredService<DiscordService>();
                    discordService.SendMessage($"Please login to Spotify with this link: {uri}", Context.User.Id, true);
                }
                else
                {
                    await ReplyAsync($"Channel **{channelId}** is already registered");
                }
            }
            catch(Exception ex)
            {
                messageWriter.Write(ex.Message);
            }
        }
        
        
        [Command("play")]
        public async Task Play(string url)
        {
            if (!Context.IsUserInModeratorRole())
                return;

            var service = serviceProvider.GetRequiredService<SpotifyService>();
            await Task.Delay(1);
            //var items = await JsonUtils.GetItems();

            //if (items.ContainsKey(Context.Channel.Id.ToString()))
            //{
            //    items.TryGetValue(Context.Channel.Id.ToString(), out var item);

            //    if (!await service.PlaySong(item.AccessCode, url))
            //    {
            //        var link = service.CreateLoginRequestUri();
            //        await ReplyAsync("Please authorize with this link: " + link);

            //    }
            //    else
            //        await ReplyAsync($"Added song  to queue");
            //}
        }

        [Command("next")]
        public async Task Next()
        {
            if (!Context.IsUserInModeratorRole())
                return;

            var spotifyService = serviceProvider.GetRequiredService<SpotifyService>();
            
            var jsonService = serviceProvider.GetRequiredService<JsonService>();
            var items = await jsonService.GetItems();
            
            if (items.ContainsKey(Context.Channel.Id.ToString()))
            {
                items.TryGetValue(Context.Channel.Id.ToString(), out var item);
                if(item is null)
                {
                    messageWriter.Write("Failed getting item");
                    return;
                }
                
                await spotifyService.PlayNextSong(item.AccessCode);
                await ReplyAsync("Playing next track");
            }
        }

        [Command("playlist")]
        public async Task PlaylistAsync(string playlistId)
        {
            if (!Context.IsUserInModeratorRole())
                return;

            var channelId = Context.Channel.Id.ToString();
            var serverName = Context.Guild.Name;
            await Task.Delay(1);

            //var items = await JsonUtils.GetItems();

            //if (items.ContainsKey(channelId))
            //{
            //    await ReplyAsync("This channel is already added");
            //    return;
            //}

            //var service = serviceProvider.GetRequiredService<SpotifyService>();
            ////var link = service.CreateLoginRequestUri();
            ////Console.WriteLine(link);

            //var item = new SpotifyItem
            //{
            //    ChannelId = channelId,
            //    PlaylistId = playlistId,
            //    AccessCode = "",
            //};
            //items.Add(item.ChannelId, item);        


            //await JsonUtils.WriteItems(items);
            //await ReplyAsync("Added channel to application. Please authorize with this link: " + link);
        }
    }
}

