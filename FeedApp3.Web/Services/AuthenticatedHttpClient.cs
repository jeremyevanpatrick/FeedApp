namespace FeedApp3.Web.Services
{
    public class AuthenticatedHttpClient
    {
        public HttpClient Client { get; }

        public AuthenticatedHttpClient(HttpClient client)
        {
            Client = client;
        }
    }
}
