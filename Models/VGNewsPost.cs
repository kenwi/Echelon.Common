namespace Echelon.Bot.Models
{
    public class VGNewsPost
    {
        public string? Title { get; set; } = "";
        public string? Link { get; set; } = "";
        public override string ToString() => $"{Title} {Link}";
    }
}