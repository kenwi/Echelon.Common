namespace Echelon.Bot.Models
{
    interface IMessage
    {
        string GetMessage();
    }

    public class FreakPrivateMessage : IMessage
    {
        //public int MessageCount { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }

        public string? Sender { get; set; }

        public string GetMessage()
        {
            return "FreakPrivateMessage";
        }
    }
}