using System.Collections.Generic;
using System.Linq;
using LightNode.Server;
using TwitterProxy.Common.Models;

namespace TwitterProxy.CoreServer.Contracts
{
    class Consumers : LightNodeContract
    {
        public Consumer Insert(ulong userId, string key, string secret, string name)
        {
            Consumer value;
            using (var tran = Database.GetTransaction())
            {
                tran.SynchronizeTables(Database.Consumers);
                var row = tran.Select<ulong, List<Consumer>>(Database.Consumers, userId);
                var userConsumers = row.Exists ? row.Value : new List<Consumer>();
                var index = userConsumers.FindIndex(x => x.Key == key && x.Secret == secret);
                if (index >= 0)
                {
                    value = userConsumers[index];
                    value.Name = name;
                }
                else
                {
                    value = new Consumer()
                    {
                        Key = key,
                        Secret = secret,
                        Name = name
                    };
                    userConsumers.Add(value);
                }
                tran.Insert(Database.Consumers, userId, userConsumers);
                tran.Commit();
            }
            return value;
        }

        public IList<Consumer> GetAllOfUser(ulong userId)
        {
            using (var tran = Database.GetTransaction())
            {
                var row = tran.Select<ulong, IList<Consumer>>(Database.Consumers, userId);
                return row.Exists ? row.Value : new Consumer[] { };
            }
        }

        public string GetSecret(ulong userId, string key)
        {
            using (var tran = Database.GetTransaction())
            {
                var row = tran.Select<ulong, IEnumerable<Consumer>>(Database.Consumers, userId);
                if (!row.Exists) return null;
                return row.Value.Where(x => x.Key == key).Select(x => x.Secret).FirstOrDefault();
            }
        }
    }
}
