using System.Collections.Generic;
using System.Linq;
using DBreeze.Transactions;
using LightNode.Server;
using Newtonsoft.Json.Linq;
using TwitterProxy.CoreServer.Models;

namespace TwitterProxy.CoreServer.Contracts
{
    class TwitterUsers : LightNodeContract
    {
        public string GetScreenName(ulong id)
        {
            using (var tran = Database.GetTransaction())
            {
                var row = tran.Select<ulong, byte[]>(Database.TwitterUsers, id);
                if (!row.Exists) return null;
                return (string)row.Value.ReadAsBsonObject()["screen_name"];
            }
        }

        #region Static Members

        private static string[] nullableFields = new[]
        {
            "description", "location", "time_zone", "url", "utc_offset"
        };

        private static string[] ignoreFields = new[]
        {
            "following", "follow_request_sent", "notifications", "muting"
        };

        public static void InsertJObjectWithTransaction(Transaction tran, JObject jo, ulong? currentUser)
        {
            var id = (ulong)jo["id"];
            var row = tran.Select<ulong, byte[]>(Database.TwitterUsers, id);
            JObject user;
            if (row.Exists)
            {
                user = row.Value.ReadAsBsonObject();

                foreach (var p in jo.Properties())
                {
                    if (user[p.Name] == null || p.Value.Type != JTokenType.Null || nullableFields.Contains(p.Name))
                        user[p.Name] = p.Value;
                }
            }
            else
            {
                user = new JObject(jo);
            }

            if (currentUser.HasValue)
            {
                var friendsRow = tran.Select<ulong, FriendsInfo>(Database.Friends, id);
                var changed = !friendsRow.Exists;
                var friendsInfo = friendsRow.Exists ? friendsRow.Value : new FriendsInfo();

                if (MergeId(id, jo["following"], friendsInfo.Friends))
                    changed = true;
                if (MergeId(id, jo["follow_request_sent"], friendsInfo.Friends))
                    changed = true;
                if (MergeId(id, jo["muting"], friendsInfo.Friends))
                    changed = true;

                if (changed)
                    tran.Insert(Database.Friends, id, friendsInfo);
            }

            foreach (var name in ignoreFields)
                user.Remove(name);

            tran.Insert(Database.TwitterUsers, id, user.ToBson());
        }

        private static bool MergeId(ulong id, JToken jt, IList<ulong> ids)
        {
            if (!jt.IsNull())
            {
                if ((bool)jt)
                {
                    if (!ids.Contains(id))
                    {
                        ids.Add(id);
                        return true;
                    }
                }
                else
                {
                    var index = ids.IndexOf(id);
                    if (index >= 0)
                    {
                        ids.RemoveAt(index);
                        return true;
                    }
                }
            }
            return false;
        }

        public static void InsertJObject(JObject jo, ulong? currentUser)
        {
            using (var tran = Database.GetTransaction())
            {
                tran.SynchronizeTables(Database.TwitterUsers, Database.Friends);
                InsertJObjectWithTransaction(tran, jo, currentUser);
                tran.Commit();
            }
        }

        #endregion
    }
}
