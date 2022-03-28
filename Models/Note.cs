namespace Echelon.Bot.Models
{
    public class Note
    {
        public ulong UserId { get; set; }
        public string? Username { get; set; }
        public string? Author { get; set; }
        public ulong AuthorId { get; set; }
        public string? Text { get; set; }
        public DateTime DateTime { get; set; }
    }
}