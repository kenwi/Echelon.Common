namespace Echelon.Bot.Providers
{
    public class FreakKvalitetsPoengProvider : DocumentProviderBase
    {
        public FreakKvalitetsPoengProvider(IMessageWriter messageWriter)
            : base(messageWriter)
        {
            this.url = "https://freak.no/forum/kvalitetspoeng.php";
        }
    }
}
