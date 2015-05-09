using LightNode.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace LightNode.Client
{
    public partial class LightNodeClient : _IAccessTokens, _IConsumers, _IProxyUsers, _ITwitterUsers
    {
        static IContentFormatter defaultContentFormatter = new LightNode.Formatter.MsgPackContentFormatter();
        readonly string rootEndPoint;
        readonly HttpClient httpClient;

        partial void OnAfterInitialized();

        public System.Net.Http.Headers.HttpRequestHeaders DefaultRequestHeaders
        {
            get { return httpClient.DefaultRequestHeaders; }
        }

        public long MaxResponseContentBufferSize
        {
            get { return httpClient.MaxResponseContentBufferSize; }
            set { httpClient.MaxResponseContentBufferSize = value; }
        }

        public TimeSpan Timeout
        {
            get { return httpClient.Timeout; }
            set { httpClient.Timeout = value; }
        }

        IContentFormatter contentFormatter;
        public IContentFormatter ContentFormatter
        {
            get { return contentFormatter = (contentFormatter ?? defaultContentFormatter); }
            set { contentFormatter = value; }
        }

        public _IAccessTokens AccessTokens { get { return this; } }
        public _IConsumers Consumers { get { return this; } }
        public _IProxyUsers ProxyUsers { get { return this; } }
        public _ITwitterUsers TwitterUsers { get { return this; } }

        public LightNodeClient(string rootEndPoint)
        {
            this.httpClient = new HttpClient();
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
            OnAfterInitialized();
        }

        public LightNodeClient(string rootEndPoint, HttpMessageHandler innerHandler)
        {
            this.httpClient = new HttpClient(innerHandler);
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
            OnAfterInitialized();
        }

        public LightNodeClient(string rootEndPoint, HttpMessageHandler innerHandler, bool disposeHandler)
        {
            this.httpClient = new HttpClient(innerHandler, disposeHandler);
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
            OnAfterInitialized();
        }

        protected virtual async Task PostAsync(string method, FormUrlEncodedContent content, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsync(rootEndPoint + method, content, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        protected virtual async Task<T> PostAsync<T>(string method, FormUrlEncodedContent content, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsync(rootEndPoint + method, content, cancellationToken).ConfigureAwait(false);
            using (var stream = await response.EnsureSuccessStatusCode().Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                return (T)ContentFormatter.Deserialize(typeof(T), stream);
            }
        }

        #region _IAccessTokens

        System.Threading.Tasks.Task<TwitterProxy.Common.Models.AccessTokenInfo> _IAccessTokens.Insert(System.String consumerKey, System.String accessToken, System.String accessTokenSecret, System.UInt64 userId, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(4);
            if (consumerKey != null) list.Add(new KeyValuePair<string, string>("consumerKey", consumerKey));
            if (accessToken != null) list.Add(new KeyValuePair<string, string>("accessToken", accessToken));
            if (accessTokenSecret != null) list.Add(new KeyValuePair<string, string>("accessTokenSecret", accessTokenSecret));
            list.Add(new KeyValuePair<string, string>("userId", userId.ToString()));

            return PostAsync<TwitterProxy.Common.Models.AccessTokenInfo>("/AccessTokens/Insert", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task<TwitterProxy.Common.Models.AccessTokenInfo> _IAccessTokens.Get(System.String accessToken, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(1);
            if (accessToken != null) list.Add(new KeyValuePair<string, string>("accessToken", accessToken));

            return PostAsync<TwitterProxy.Common.Models.AccessTokenInfo>("/AccessTokens/Get", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task<System.Collections.Generic.IList<TwitterProxy.Common.Models.AccessTokenViewModel>> _IAccessTokens.GetAllOfUser(System.UInt64 userId, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(1);
            list.Add(new KeyValuePair<string, string>("userId", userId.ToString()));

            return PostAsync<System.Collections.Generic.IList<TwitterProxy.Common.Models.AccessTokenViewModel>>("/AccessTokens/GetAllOfUser", new FormUrlEncodedContent(list), cancellationToken);
        }

        #endregion

        #region _IConsumers

        System.Threading.Tasks.Task<TwitterProxy.Common.Models.Consumer> _IConsumers.Insert(System.UInt64 userId, System.String key, System.String secret, System.String name, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(4);
            list.Add(new KeyValuePair<string, string>("userId", userId.ToString()));
            if (key != null) list.Add(new KeyValuePair<string, string>("key", key));
            if (secret != null) list.Add(new KeyValuePair<string, string>("secret", secret));
            if (name != null) list.Add(new KeyValuePair<string, string>("name", name));

            return PostAsync<TwitterProxy.Common.Models.Consumer>("/Consumers/Insert", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task<System.Collections.Generic.IList<TwitterProxy.Common.Models.Consumer>> _IConsumers.GetAllOfUser(System.UInt64 userId, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(1);
            list.Add(new KeyValuePair<string, string>("userId", userId.ToString()));

            return PostAsync<System.Collections.Generic.IList<TwitterProxy.Common.Models.Consumer>>("/Consumers/GetAllOfUser", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task<System.String> _IConsumers.GetSecret(System.UInt64 userId, System.String key, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(2);
            list.Add(new KeyValuePair<string, string>("userId", userId.ToString()));
            if (key != null) list.Add(new KeyValuePair<string, string>("key", key));

            return PostAsync<System.String>("/Consumers/GetSecret", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task<System.Boolean> _IConsumers.Delete(System.UInt64 userId, System.String key, System.String secret, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(3);
            list.Add(new KeyValuePair<string, string>("userId", userId.ToString()));
            if (key != null) list.Add(new KeyValuePair<string, string>("key", key));
            if (secret != null) list.Add(new KeyValuePair<string, string>("secret", secret));

            return PostAsync<System.Boolean>("/Consumers/Delete", new FormUrlEncodedContent(list), cancellationToken);
        }

        #endregion

        #region _IProxyUsers

        System.Threading.Tasks.Task<TwitterProxy.Common.Models.ProxyUser> _IProxyUsers.Insert(System.UInt64 userId, System.String accessToken, System.String accessTokenSecret, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(3);
            list.Add(new KeyValuePair<string, string>("userId", userId.ToString()));
            if (accessToken != null) list.Add(new KeyValuePair<string, string>("accessToken", accessToken));
            if (accessTokenSecret != null) list.Add(new KeyValuePair<string, string>("accessTokenSecret", accessTokenSecret));

            return PostAsync<TwitterProxy.Common.Models.ProxyUser>("/ProxyUsers/Insert", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task<TwitterProxy.Common.Models.ProxyUser> _IProxyUsers.Get(System.UInt64 userId, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(1);
            list.Add(new KeyValuePair<string, string>("userId", userId.ToString()));

            return PostAsync<TwitterProxy.Common.Models.ProxyUser>("/ProxyUsers/Get", new FormUrlEncodedContent(list), cancellationToken);
        }

        #endregion

        #region _ITwitterUsers

        System.Threading.Tasks.Task<System.String> _ITwitterUsers.GetScreenName(System.UInt64 id, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(1);
            list.Add(new KeyValuePair<string, string>("id", id.ToString()));

            return PostAsync<System.String>("/TwitterUsers/GetScreenName", new FormUrlEncodedContent(list), cancellationToken);
        }

        #endregion

    }

    public interface _IAccessTokens
    {
        System.Threading.Tasks.Task<TwitterProxy.Common.Models.AccessTokenInfo> Insert(System.String consumerKey, System.String accessToken, System.String accessTokenSecret, System.UInt64 userId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task<TwitterProxy.Common.Models.AccessTokenInfo> Get(System.String accessToken, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task<System.Collections.Generic.IList<TwitterProxy.Common.Models.AccessTokenViewModel>> GetAllOfUser(System.UInt64 userId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface _IConsumers
    {
        System.Threading.Tasks.Task<TwitterProxy.Common.Models.Consumer> Insert(System.UInt64 userId, System.String key, System.String secret, System.String name, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task<System.Collections.Generic.IList<TwitterProxy.Common.Models.Consumer>> GetAllOfUser(System.UInt64 userId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task<System.String> GetSecret(System.UInt64 userId, System.String key, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task<System.Boolean> Delete(System.UInt64 userId, System.String key, System.String secret, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface _IProxyUsers
    {
        System.Threading.Tasks.Task<TwitterProxy.Common.Models.ProxyUser> Insert(System.UInt64 userId, System.String accessToken, System.String accessTokenSecret, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task<TwitterProxy.Common.Models.ProxyUser> Get(System.UInt64 userId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface _ITwitterUsers
    {
        System.Threading.Tasks.Task<System.String> GetScreenName(System.UInt64 id, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    }

}

