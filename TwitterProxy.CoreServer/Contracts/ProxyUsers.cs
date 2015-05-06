using LightNode.Server;
using TwitterProxy.Common;
using TwitterProxy.Common.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace TwitterProxy.CoreServer.Contracts
{
    class ProxyUsers : LightNodeContract
    {
        public async Task<ProxyUser> Insert(ulong userId, string accessToken, string accessTokenSecret)
        {
            using (var hc = new HttpClient(new TwitterOAuthHttpMessageHandler(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip })
            {
                ConsumerKey = EnvironmentVariables.ConsumerKey,
                ConsumerSecret = EnvironmentVariables.ConsumerSecret,
                OAuthToken = accessToken,
                OAuthTokenSecret = accessTokenSecret
            }))
            {
                TwitterUsers.InsertJObject(
                    JObject.Parse(
                        await hc.GetStringAsync("https://api.twitter.com/1.1/account/verify_credentials.json").ConfigureAwait(false)
                    ),
                    userId
                );
            }

            var value = new ProxyUser() { AccessToken = accessToken, AccessTokenSecret = accessTokenSecret };
            using (var tran = Database.GetTransaction())
            {
                tran.Insert(Database.ProxyUsers, userId, value);
                tran.Commit();
            }
            return value;
        }

        public ProxyUser Get(ulong userId)
        {
            using (var tran = Database.GetTransaction())
            {
                var row = tran.Select<ulong, ProxyUser>(Database.ProxyUsers, userId);
                return row.Exists ? row.Value : null;
            }
        }
    }
}
