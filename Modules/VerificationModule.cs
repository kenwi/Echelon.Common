using Discord.Commands;
using Discord.WebSocket;
using Echelon.Common.Extensions;
using System.Text;

namespace Echelon.Bot.Services
{
    public class VerificationProcessModule : ModuleBase<SocketCommandContext>
    {
        private static string verifiedFile = "verified.txt";

        [Command("verify")]
        [Summary("Initiates verification.")]
        public async Task VerifyAsync()
        {
            var channel = await Context.User.CreateDMChannelAsync();
            var fileContent = File.ReadAllLines(verifiedFile);

            var isVerified = fileContent.Where(line =>
                line.Contains(Context.User.Username)
                && line.Contains("OK"))
                .Any();

            var hasRequestedVerification = fileContent.Where(line =>
                line.Contains(Context.User.Username)
                && !line.Contains("OK"))
                .Any();

            if (isVerified)
            {
                await channel.SendMessageAsync("Du er allerede verifisert.");
                return;
            }

            if (hasRequestedVerification && !isVerified)
            {
                await channel.SendMessageAsync("Du har allerede forespurt verifisering.");
                return;
            }

            var guid = Guid.NewGuid();
            var message = "Hei!"
                + Environment.NewLine
                + Environment.NewLine
                + $"For å verifisere, vennligst bruk linken til å sende en privat melding. Fyll inn både ***tittel*** og ***melding*** med: ***{guid}***"
                + Environment.NewLine
                + Environment.NewLine
                + "Mobilbrukere kan kopiere neste melding."
                + Environment.NewLine
                + Environment.NewLine
                + "https://freak.no/forum/private.php?do=newpm&u=2182";

            await channel.SendMessageAsync(message);
            await channel.SendMessageAsync(guid.ToString());

            File.AppendAllText(verifiedFile, $"{Context.User.Username}\t{Context.User.Id}\t{guid}" + Environment.NewLine);
        }
    }

    //[Group("verify")]
    public class VerificationModule : ModuleBase<SocketCommandContext>
    {
        private static string verifiedFile = "verified.txt";
        private readonly IServiceProvider serviceProvider;

        public VerificationModule(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Command("verify check")]
        public async Task CheckAsync(string username)
        {
            if (!Context.IsMessageFromModeratorChannel() && !Context.IsMessageFromDevelopmentChannel())
                return;

            if (!Context.IsUserInModeratorRole())
                return;

            username = username.ToLower();
            var fileContent = File.ReadAllLines(verifiedFile);
            var isVerified = fileContent.Where(line =>
            {
                var content = line.Split("\t");
                return (content.First().ToLower() == username || content.Skip(content.Length - 2)?.First().ToLower() == username)
                    && content.Last().ToLower() == "ok";
            }).Any();
            await ReplyAsync(isVerified ? $"{username} is verified" : $"{username} is not verified");
        }

        [Command("verify show")]
        public async Task ShowVerifiedAsync()
        {
#if RELEASE
            if (!Context.IsMessageFromModeratorChannel() && !Context.IsMessageFromDevelopmentChannel())
                return;

            if (!Context.IsUserInModeratorRole())
                return;
#endif
            var names = new List<string>();
            var builder = new StringBuilder();
            var fileContent = File.ReadAllLines(verifiedFile);
            foreach(var line in fileContent.Where(line => line.Contains("OK")))
            {
                var partCount = line.Split("\t").Length;
                var discordUsername = line.Split("\t").First();
                var freakUsername = line.Split("\t").Skip(partCount - 2).FirstOrDefault();
                names.Add($"{discordUsername}({freakUsername})");
            }
            names.Sort();
            var nameList = string.Join(", ", names).TrimEnd();
            await ReplyAsync(nameList);
        }
    }
}

