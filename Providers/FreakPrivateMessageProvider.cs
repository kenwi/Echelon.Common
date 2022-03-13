using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Echelon.Bot.Providers
{
    public class FreakPrivateMessageProvider : DocumentProviderBase
    {
        private readonly IConfigurationRoot configuration;

        public FreakPrivateMessageProvider(
            IConfigurationRoot configuration,
            IMessageWriter messageWriter) 
            : base(messageWriter)
        {
            this.url = "https://freak.no/forum/private.php";
            this.configuration = configuration;
        }

        public override async Task<HtmlDocument> GetAsync()
        {
            var freakPassword = configuration.GetValue<string>("freakpassword");
            var freakUserId = configuration.GetValue<string>("freakuserid");
            var cookies = new CookieContainer();
            cookies.Add(new Uri("https://freak.no"), new Cookie("freakpassword", freakPassword));
            cookies.Add(new Uri("https://freak.no"), new Cookie("freakuserid", freakUserId));
            var request = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = cookies
            });
            var result = await request.GetAsync(url);
            result.EnsureSuccessStatusCode();
            var document = new HtmlDocument();
            document.LoadHtml(await result.Content.ReadAsStringAsync());
            return document;
        }
    }
}
