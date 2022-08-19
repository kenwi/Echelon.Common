using System.Text.Json;

namespace Echelon.Bot.Models
{
    public class SpotifyItem
    {
        public string ChannelId { get; set; } = "";
        public string PlaylistId { get; set; } = "";
        public string OwnerId { get; set; } = "";
        public string AccessCode { get; set; } = "";
        public string ResponseCode { get; set; } = "";
        public string OwnerName { get; set; } = "";
        public string ServerName { get; set; } = "";
    }
}

