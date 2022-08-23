using SpotifyAPI.Web;
using System.Text.Json;

namespace Echelon.Bot.Models
{
    public class SpotifyItem
    {
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; } = "";
        public string PlaylistId { get; set; } = "";
        public string PlaylistName { get; set; } = "";
        public ulong OwnerId { get; set; }
        public string Challenge { get; set; } = "";
        public string OwnerName { get; set; } = "";
        public string ServerName { get; set; } = "";
        public bool IsVerbose { get; set; } = true;
        public PKCETokenResponse? Token { get; set;} = null;
        public DateTime TokenUpdated { get; internal set; }
    }
}

