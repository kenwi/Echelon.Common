using Discord.Commands;
using Echelon.Common.Extensions;
using System.Text;

namespace Echelon.Bot.Services
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpAsync()
        {
            if (!Context.IsMessageFromModeratorChannel() && !Context.IsMessageFromDevelopmentChannel())
                return;

            if (!Context.IsUserInModeratorRole())
                return;

            var builder = new StringBuilder();
            builder.AppendLine("**!activity show**: Show the forum activity counters");
            builder.AppendLine("**!note add <user> <text>**: Add a note to a discord user");
            builder.AppendLine("**!note show**: Show all notes");
            builder.AppendLine("**!verify**: Initiates the verification process");
            builder.AppendLine("**!verify check <user>**: Check if a user is verified");
            builder.AppendLine("**!verify show**: Show all verified users");
            builder.AppendLine("**!watch add <user>**: Add a freak user to the watchlist");
            //builder.AppendLine("**!activity search <keyword>**: Search through activity");
            //builder.AppendLine("**!warning add <user>**: Send a warning to a freak user");
            //builder.AppendLine("**!watch list**: View watchlist");
            //builder.AppendLine("**!mute add <user>**: Mute user");
            //builder.AppendLine("**!mute clear <user>**: Unmute user");
            //builder.AppendLine("**!mute list**: View muted users");
            await ReplyAsync(builder.ToString());
        }
    }
}

