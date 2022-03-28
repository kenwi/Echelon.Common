using Discord.Commands;
using Echelon.Bot.Models;
using Echelon.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;

namespace Echelon.Bot.Services
{
    [Group("note")]
    public class NoteModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider serviceProvider;

        public NoteModule(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        async Task<List<Note>> InitializeList()
        {
            var configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
            var filename = configuration.GetValue<string>("notesfile");

            var content = await File.ReadAllTextAsync(filename);
            var data = new List<Note>();
            if (!string.IsNullOrEmpty(content))
            {
                data = JsonSerializer.Deserialize<List<Note>>(content);
            }
            return data;
        }

        [Command("show")]
        public async Task Show(string? username = null)
        {
            if (!Context.IsMessageFromModeratorChannel() && !Context.IsMessageFromDevelopmentChannel())
                return;

            if (!Context.IsUserInModeratorRole())
                return;

            var logger = serviceProvider.GetRequiredService<IMessageWriter>();
            var builder = new StringBuilder(Environment.NewLine);
            try
            {
                int noteIndex = 0;
                var data = await InitializeList();
                foreach(var note in data)
                {
                    builder.AppendLine($"[{noteIndex++}] [{note.DateTime.ToString()}] [{note.Author}] [{note.Username}] {note.Text}");
                }
            }
            catch(Exception ex)
            {
                logger.Write(ex.Message);
            }
            await ReplyAsync(builder.ToString());
        }

        [Command("add")]
        public async Task AddNoteAsync(string username, string text)
        {
            if (!Context.IsMessageFromModeratorChannel() && !Context.IsMessageFromDevelopmentChannel())
                return;

            if (!Context.IsUserInModeratorRole())
                return;

            var logger = serviceProvider.GetRequiredService<IMessageWriter>();
            var configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
            var filename = configuration.GetValue<string>("notesfile");
            ulong userId = 0;
            
            try
            {
                var content = await File.ReadAllTextAsync(filename);
                var data = new List<Note>();
                if (!string.IsNullOrEmpty(content))
                {
                    data = JsonSerializer.Deserialize<List<Note>>(content);
                }

                data?.Add(new Note
                {
                    DateTime = DateTime.Now,
                    UserId = userId,
                    Username = username,
                    Author = Context.Message.Author.Username,
                    AuthorId = Context.Message.Author.Id,
                    Text = text
                });

                content = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filename, content);
                await ReplyAsync($"Added note to user {username}");
            }
            catch (Exception ex)
            {
                logger.Write(ex.Message);
            }
        }
    }
}

