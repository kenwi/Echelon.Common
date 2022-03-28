using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echelon.Bot.Models
{
    public enum OutboundMessageType
    {
        Discord,
        Freak
    }

    public class OutboundMessage
    {
        public OutboundMessageType MessageType { get; set; }
        public string? Text { get; set; }
        public ulong TargetID { get; set; }
        public string? Caller { get; set; }
    }
}
