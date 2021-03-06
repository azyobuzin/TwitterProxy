﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace TwitterProxy.WebServer
{
    static class Extensions
    {
        public static IAppBuilder Match<T>(this IAppBuilder app, string path, params object[] args)
        {
            return app.MapWhen(ctx => ctx.Request.Path.Value.Equals(path, StringComparison.OrdinalIgnoreCase), a => a.Use<T>(args));
        }

        public static IAppBuilder Match<T>(this IAppBuilder app, IEnumerable<string> paths, params object[] args)
        {
            return app.MapWhen(ctx => paths.Contains(ctx.Request.Path.Value, StringComparer.OrdinalIgnoreCase), a => a.Use<T>(args));
        }

        public static bool EqualsIgnoreCase(this string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsGet(this IOwinRequest request)
        {
            return request.Method.EqualsIgnoreCase("GET");
        }

        public static bool IsHead(this IOwinRequest request)
        {
            return request.Method.EqualsIgnoreCase("HEAD");
        }

        public static Dictionary<string, string> ParseAuthorizationHeader(this IOwinRequest request)
        {
            var authHeader = request.Headers.Get("Authorization");
            if (authHeader == null || !authHeader.StartsWith("OAuth")) return null;

            return authHeader.Substring(6)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().Split('='))
                .ToDictionary(
                    x => Uri.UnescapeDataString(x[0]),
                    x => Uri.UnescapeDataString(x[1].Substring(1, x[1].Length - 2))
                );
        }

        public static async Task<Stream> ReadAsStreamAsync2(this HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Headers.ContentEncoding.Contains("gzip", StringComparer.OrdinalIgnoreCase)
                ? new GZipStream(stream, CompressionMode.Decompress)
                : stream;
        }

        public static async Task<string> ReadAsStringAsync2(this HttpContent content)
        {
            using (var reader = new StreamReader(await content.ReadAsStreamAsync2().ConfigureAwait(false)))
                return await reader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
