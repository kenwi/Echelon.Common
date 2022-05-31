using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Echelon.Bot.Services
{
    [Group("notification")]
    public class NotificationModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider serviceProvider;

        public NotificationModule(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Command("on")]
        public async Task On()
        {
            var username = Context.User.Username.ToLower();
            var configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
            var filename = configuration.GetValue<string>("notificationFile");
            var fileContent = await File.ReadAllLinesAsync(filename);
            var isAlreadyEnabled = fileContent.Any(line => line.ToLower().Contains(username));

            if (isAlreadyEnabled)
            {
                await ReplyAsync(configuration.GetValue<string>("userIsAlreadyEnabledMessage"));
                return;
            }

            try
            {
                var verificationService = serviceProvider.GetRequiredService<UserService>();
                var freakUserName = await verificationService.GetFreakUserName(username);
                if (freakUserName is null)
                {
                    await ReplyAsync(configuration.GetValue<string>("pleaseVerifyMessage"));
                    return;
                }

                await File.AppendAllTextAsync(filename, $"{username}\t{Context.User.Id}\t{freakUserName}{Environment.NewLine}");
                await ReplyAsync(configuration.GetValue<string>("notificationsEnabledMessage"));
            }
            catch (Exception ex)
            {
                var messageWriter = serviceProvider.GetRequiredService<IMessageWriter>();
                messageWriter.Write(ex.ToString());
                throw;
            }
        }

        [Command("off")]
        public async Task Off()
        {
            var username = Context.User.Username.ToLower();
            var configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
            var filename = configuration.GetValue<string>("notificationFile");
            var fileContent = await File.ReadAllLinesAsync(filename);
            var isAlreadyEnabled = fileContent.Any(line => line.ToLower().Contains(username));

            if (!isAlreadyEnabled)
            {
                await ReplyAsync(configuration.GetValue<string>("notificationsNotEnabledMessage"));
                return;
            }

            try
            {
                var filteredContent = fileContent.Where(line => !line.ToLower().Contains(username));
                await File.WriteAllLinesAsync(filename, filteredContent);
                await ReplyAsync(configuration.GetValue<string>("notificationsDisabledMessage"));
            }
            catch (Exception ex)
            {
                var messageWriter = serviceProvider.GetRequiredService<IMessageWriter>();
                messageWriter.Write(ex.ToString());
                throw;
            }
        }
    }
}

