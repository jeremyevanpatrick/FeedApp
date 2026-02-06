namespace FeedApp3.Web.Services
{
    public class PublicHttpClient
    {
        public HttpClient Client { get; }

        public PublicHttpClient(HttpClient client)
        {
            Client = client;
        }
    }
}
