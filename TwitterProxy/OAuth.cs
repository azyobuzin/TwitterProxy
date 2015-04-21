using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TwitterProxy
{
    static class OAuth
    {
        private static string[] encodeMap = new[]
        {
            "%00", "%01", "%02", "%03", "%04", "%05", "%06", "%07",
            "%08", "%09", "%0A", "%0B", "%0C", "%0D", "%0E", "%0F",
            "%10", "%11", "%12", "%13", "%14", "%15", "%16", "%17",
            "%18", "%19", "%1A", "%1B", "%1C", "%1D", "%1E", "%1F",
            "%20", "%21", "%22", "%23", "%24", "%25", "%26", "%27",
            "%28", "%29", "%2A", "%2B", "%2C", "-", ".", "%2F",
            "0", "1", "2", "3", "4", "5", "6", "7",
            "8", "9", "%3A", "%3B", "%3C", "%3D", "%3E", "%3F",
            "%40", "A", "B", "C", "D", "E", "F", "G",
            "H", "I", "J", "K", "L", "M", "N", "O",
            "P", "Q", "R", "S", "T", "U", "V", "W",
            "X", "Y", "Z", "%5B", "%5C", "%5D", "%5E", "_",
            "%60", "a", "b", "c", "d", "e", "f", "g",
            "h", "i", "j", "k", "l", "m", "n", "o",
            "p", "q", "r", "s", "t", "u", "v", "w",
            "x", "y", "z", "%7B", "%7C", "%7D", "~", "%7F",
            "%80", "%81", "%82", "%83", "%84", "%85", "%86", "%87",
            "%88", "%89", "%8A", "%8B", "%8C", "%8D", "%8E", "%8F",
            "%90", "%91", "%92", "%93", "%94", "%95", "%96", "%97",
            "%98", "%99", "%9A", "%9B", "%9C", "%9D", "%9E", "%9F",
            "%A0", "%A1", "%A2", "%A3", "%A4", "%A5", "%A6", "%A7",
            "%A8", "%A9", "%AA", "%AB", "%AC", "%AD", "%AE", "%AF",
            "%B0", "%B1", "%B2", "%B3", "%B4", "%B5", "%B6", "%B7",
            "%B8", "%B9", "%BA", "%BB", "%BC", "%BD", "%BE", "%BF",
            "%C0", "%C1", "%C2", "%C3", "%C4", "%C5", "%C6", "%C7",
            "%C8", "%C9", "%CA", "%CB", "%CC", "%CD", "%CE", "%CF",
            "%D0", "%D1", "%D2", "%D3", "%D4", "%D5", "%D6", "%D7",
            "%D8", "%D9", "%DA", "%DB", "%DC", "%DD", "%DE", "%DF",
            "%E0", "%E1", "%E2", "%E3", "%E4", "%E5", "%E6", "%E7",
            "%E8", "%E9", "%EA", "%EB", "%EC", "%ED", "%EE", "%EF",
            "%F0", "%F1", "%F2", "%F3", "%F4", "%F5", "%F6", "%F7",
            "%F8", "%F9", "%FA", "%FB", "%FC", "%FD", "%FE", "%FF"
        };

        public static string PercentEncode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var sb = new StringBuilder(bytes.Length * 3);
            foreach (var b in bytes)
                sb.Append(encodeMap[b]);
            return sb.ToString();
        }

        private static string BaseStringUrl(string url)
        {
            return new Uri(url).GetLeftPart(UriPartial.Path);
        }

        private static string NormalizeParameters(string query)
        {
            var pairs = query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            Array.Sort(pairs, Comparer<string>.Create((x, y) =>
            {
                var xs = x.Split('=');
                var ys = y.Split('=');
                var result = string.CompareOrdinal(xs[0], ys[0]);
                return result != 0
                    ? result
                    : string.CompareOrdinal(xs[1], ys[1]);
            }));
            return string.Join("&", pairs);
        }

        private static long GetUnixTime()
        {
            return (DateTime.UtcNow.Ticks - 621355968000000000) / 10000000;
        }

        private static Random rnd = new Random();
        private static string Nonce()
        {
            var s = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var sb = new StringBuilder(42);
            for (var i = 0; i < 42; i++)
                sb.Append(s[rnd.Next(s.Length)]);
            return sb.ToString();
        }

        public static Dictionary<string, string> OAuthParameters(string consumerKey, string token, string callback, string verifier)
        {
            var dic = new Dictionary<string, string>()
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_signature_method",  "HMAC-SHA1" },
                { "oauth_timestamp", GetUnixTime().ToString("D") },
                { "oauth_nonce", Nonce() }
            };

            if (!string.IsNullOrEmpty(token))
                dic.Add("oauth_token", token);

            if (!string.IsNullOrEmpty(callback))
                dic.Add("oauth_callback", callback);

            if (!string.IsNullOrEmpty(verifier))
                dic.Add("oauth_verifier", verifier);

            return dic;
        }

        public static string SignatureBaseString(string method, string url, string query, IReadOnlyDictionary<string, string> oauthParams)
        {
            var querysb = new StringBuilder();
            if (query != null) querysb.Append(query);

            foreach (var kvp in oauthParams)
            {
                if (kvp.Key != "realm")
                    querysb.AppendFormat("&{0}={1}", PercentEncode(kvp.Key), PercentEncode(kvp.Value));
            }

            return string.Format(
                "{0}&{1}&{2}",
                method.ToUpperInvariant(),
                PercentEncode(BaseStringUrl(url)),
                PercentEncode(NormalizeParameters(querysb.ToString()))
            );
        }

        public static string Signature(string baseString, string consumerSecret, string tokenSecret)
        {
            var key = string.Format("{0}&{1}", PercentEncode(consumerSecret), tokenSecret != null ? PercentEncode(tokenSecret) : "");
            using (var hs1 = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
                return Convert.ToBase64String(hs1.ComputeHash(Encoding.UTF8.GetBytes(baseString)));
        }

        public static string CreateHeader(IReadOnlyDictionary<string, string> oauthParams, string signature)
        {
            var sb = new StringBuilder();
            foreach (var kvp in oauthParams.Concat(new[] { new KeyValuePair<string, string>("oauth_signature", signature) }))
            {
                sb.AppendFormat(@"{0}=""{1}"",", PercentEncode(kvp.Key), PercentEncode(kvp.Value));
            }
            if (sb.Length > 0)
                sb.Length--;
            return sb.ToString();
        }
    }
}
