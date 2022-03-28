using Discord.Commands;
using Discord.WebSocket;

namespace Echelon.Common.Extensions
{
    internal static class SocketCommandContextEx
    {
        public static bool IsUserInModeratorRole(this SocketCommandContext context)
        {
            var author = context.Message.Author as SocketGuildUser;
            return author!.Roles.Any(role => role.Name.ToLower() == "mods");
        }

        public static bool IsMessageFromModeratorChannel(this SocketCommandContext context)
            => context.Channel.Name.StartsWith("mods");

        public static bool IsMessageFromDevelopmentChannel(this SocketCommandContext context)
            => context.Channel.Name.Contains("dev");
    }
}
