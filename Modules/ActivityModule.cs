using Discord.Commands;
using Echelon.Bot.Models;
using Echelon.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Echelon.Bot.Services
{
    [Group("activity")]
    public class ActivityModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider serviceProvider;

        public ActivityModule(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Command("show")]
        public async Task ReportAsync()
        {
            if (!Context.IsMessageFromModeratorChannel() && !Context.IsMessageFromDevelopmentChannel())
                return;

            if (!Context.IsUserInModeratorRole())
                return;

            var builder = new StringBuilder();
            var logger = serviceProvider.GetRequiredService<IMessageWriter>();

            static bool isFreakRelated(string text)
                => text.ToLower()
                    .StartsWith("kvalitetspoeng")
                || text.ToLower()
                    .StartsWith("nytt");

            try
            {
                var configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
                var filename = configuration.GetValue<string>("logfile");
                var fileContent = await File.ReadAllLinesAsync(filename);
                //builder.AppendLine($"Loglines: {fileContent.Length}");

                int postsToday = 0, postsYesterday = 0, week = 0, freakrelated = 0;
                int kpToday = 0, kpYesterday = 0;
                foreach (var line in fileContent)
                {
                    var logLine = new LogLine(line);
                    if (!isFreakRelated(logLine.Text))
                        continue;
                    freakrelated++;

                    logger.Write($"{logLine.Date} {logLine.Text}");
                    if (logLine.Date > DateTime.Today.Date)
                    {
                        if(logLine.Text.ToLower().StartsWith("nytt"))
                            postsToday++;

                        if(logLine.Text.ToLower().StartsWith("kvalitetspoeng"))
                            kpToday++;
                    }

                    if (logLine.Date > DateTime.Today.Date.AddDays(-1))
                    {
                        if (logLine.Text.ToLower().StartsWith("nytt"))
                            postsYesterday++;

                        if (logLine.Text.ToLower().StartsWith("kvalitetspoeng"))
                            kpYesterday++;
                    }

                    var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Now.DayOfWeek - 7);
                    if (logLine.Date > startOfWeek)
                    {
                        week++;
                    }
                }
                builder.AppendLine($"Posts today: {postsToday}");
                builder.AppendLine($"Posts yesterday: {postsYesterday}");
                builder.AppendLine($"Kvalitetspoeng today: {kpToday}");
                builder.AppendLine($"Kvalitetspoeng yesterday: {kpYesterday}");
                logger.Write(builder.ToString());
            }
            catch (Exception ex)
            {
                logger.Write(ex.Message);
            }
#if RELEASE
            await ReplyAsync(builder.ToString());
#endif
        }
    }
}

