namespace Echelon.Bot.Models
{
    public class NRKNewsPost
    {
        public string? Title { get; set; } = "";
        public string? Link { get; set; } = "";
        public override string ToString() => $"{Title} {Link}";
    }
}
