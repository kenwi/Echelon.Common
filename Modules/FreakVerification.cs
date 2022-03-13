using Discord.Commands;

namespace Echelon.Bot.Services
{
    public class FreakVerification : ModuleBase<SocketCommandContext>
    {
        private static string verifiedFile = "verified.txt";
        private readonly IServiceProvider serviceProvider;

        public FreakVerification(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

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

            if(hasRequestedVerification && !isVerified)
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
}

