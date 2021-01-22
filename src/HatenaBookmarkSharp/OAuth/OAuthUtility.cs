﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace HatenaBookmarkSharp.OAuth
{
    internal static class OAuthUtility
    {
        static readonly Random random = new Random();

        static byte[] ComputeHash(byte[] key, byte[] buffer)
        {
            return new HMACSHA1(key).ComputeHash(buffer);
        }

        public static IEnumerable<KeyValuePair<string, string>> BuildBasicParameters(
            string consumerKey,
            string consumerSecret,
            string url,
            HttpMethod method,
            Token? token = null,
            IEnumerable<KeyValuePair<string, string>>? optionalParameters = null)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            var parameters = new List<KeyValuePair<string, string>>(capacity: 7)
            {
                new KeyValuePair<string,string>("oauth_consumer_key", consumerKey),
                new KeyValuePair<string,string>("oauth_nonce", random.Next().ToString() ),
                new KeyValuePair<string,string>("oauth_timestamp", DateTime.UtcNow.ToUnixTime().ToString() ),
                new KeyValuePair<string,string>("oauth_signature_method", "HMAC-SHA1" ),
                new KeyValuePair<string,string>("oauth_version", "1.0" )
            };
            if (token != null)
            {
                parameters.Add(new KeyValuePair<string, string>("oauth_token", token.Key));
            }
            if (optionalParameters == null)
            {
                optionalParameters = Enumerable.Empty<KeyValuePair<string, string>>();
            }

            var signature = GenerateSignature(
                consumerSecret,
                new Uri(url),
                method,
                token,
                parameters.Concat(optionalParameters));

            parameters.Add(new KeyValuePair<string, string>("oauth_signature", signature));

            return parameters;
        }

        static string GenerateSignature(
            string consumerSecret,
            Uri uri,
            HttpMethod method,
            Token? token,
            IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var hmacKeyBase = consumerSecret.UrlEncode() +
                "&" +
                ((token == null) ? "" : token.Secret).UrlEncode();

            // escaped => unescaped[]
            var queryParams = Utility.ParseQueryString(uri.GetComponents(UriComponents.Query | UriComponents.KeepDelimiter, UriFormat.UriEscaped));

            var stringParameter = parameters
                .Where(x => x.Key.ToLower() != "realm")
                .Concat(queryParams)
                .Select(p => new { Key = p.Key.UrlEncode(), Value = p.Value.UrlEncode() })
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .ThenBy(p => p.Value, StringComparer.Ordinal)
                .Select(p => p.Key + "=" + p.Value)
                .ToString("&");
            var signatureBase = method.ToString() +
                "&" + uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped).UrlEncode() +
                "&" + stringParameter.UrlEncode();

            var hash = ComputeHash(Encoding.UTF8.GetBytes(hmacKeyBase), Encoding.UTF8.GetBytes(signatureBase));
            return Convert.ToBase64String(hash).UrlEncode();
        }

        public static HttpClient CreateOAuthClient(string consumerKey, string consumerSecret, AccessToken accessToken, IEnumerable<KeyValuePair<string, string>>? optionalOAuthHeaderParameters = null) =>
            new HttpClient(new OAuthMessageHandler(consumerKey, consumerSecret, accessToken, optionalOAuthHeaderParameters));

        public static HttpClient CreateOAuthClient(HttpMessageHandler innerHandler, string consumerKey, string consumerSecret, AccessToken accessToken, IEnumerable<KeyValuePair<string, string>>? optionalOAuthHeaderParameters = null) =>
            new HttpClient(new OAuthMessageHandler(innerHandler, consumerKey, consumerSecret, accessToken, optionalOAuthHeaderParameters));
    }
}
