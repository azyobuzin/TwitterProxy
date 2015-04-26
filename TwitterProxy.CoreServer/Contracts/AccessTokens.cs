using System;
using System.Collections.Generic;
using System.Linq;
using LightNode.Server;
using TwitterProxy.Common;

namespace TwitterProxy.CoreServer.Contracts
{
    class AccessTokens : LightNodeContract
    {
        public AccessTokenInfo Insert(string accessToken, string accessTokenSecret, ulong userId, string consumerKey, string consumerSecret = null)
        {
            if (string.IsNullOrEmpty(consumerSecret))
            {
                consumerSecret = new Consumers().GetSecret(userId, consumerKey);
                if (string.IsNullOrEmpty(consumerSecret))
                    return null;
            }

            var value = new AccessTokenInfo()
            {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                AccessTokenSecret = accessTokenSecret,
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };

            using (var tran = Database.GetTransaction())
            {
                tran.SynchronizeTables(Database.AccessTokens);
                var row = tran.Select<string, AccessTokenInfo>(Database.AccessTokens, accessToken);
                if (row.Exists)
                    return row.Value;
                tran.Insert(Database.AccessTokens, accessToken, value);
                tran.Commit();
            }

            return value;
        }

        public AccessTokenInfo Get(string accessToken)
        {
            using (var tran = Database.GetTransaction())
            {
                var row = tran.Select<string, AccessTokenInfo>(Database.AccessTokens, accessToken);
                return row.Exists ? row.Value : null;
            }
        }

        public IList<AccessTokenViewModel> GetAllOfUser(ulong userId)
        {
            var consumers = new Consumers().GetAllOfUser(userId);
            using (var tran = Database.GetTransaction())
            {
                return tran.SelectForwardStartsWith<string, AccessTokenInfo>(Database.AccessTokens, userId.ToString("D") + "-")
                    .Where(row => row.Value.UserId == userId)
                    .Select(row =>
                    {
                        var value = row.Value;
                        return new AccessTokenViewModel()
                        {
                            Consumer = consumers.FirstOrDefault(c => c.Key == value.ConsumerKey)
                                ?? new Consumer() { Key = value.ConsumerKey, Secret = value.ConsumerSecret },
                            AccessToken = row.Key,
                            CreatedAtUtc = value.CreatedAtUtc
                        };
                    })
                    .ToArray();
            }
        }
    }
}
