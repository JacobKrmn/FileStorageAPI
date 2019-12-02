using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace FileStorageAPI.Azure
{
    public class AuthenticationBuilder
    {
        public string httpMethod { get; set; }
        public string storageAccount { get; set; }
        public string keyType { get; set; }

        private string getCanonicalizedHeaders(HttpRequestMessage httpRequestMessage) {
            var headers = from kvp in httpRequestMessage.Headers
                          where kvp.Key.StartsWith("x-ms-", StringComparison.OrdinalIgnoreCase)
                          orderby kvp.Key
                          select new { Key = kvp.Key.ToLowerInvariant(), kvp.Value };
            StringBuilder hBuilder = new StringBuilder();
            foreach (var kvp in headers) {
                hBuilder.Append(kvp.Key);
                char separator = ':';

                foreach (string headerValue in kvp.Value) {
                    string trimmedValue = headerValue.TrimStart().Replace("\r\n", string.Empty);
                    hBuilder.Append(separator).Append(trimmedValue);
                    separator = ',';
                }
                hBuilder.Append("\n");
            }
            return hBuilder.ToString();
        }

        private string getCanonicalizedResource(Uri address, string storageAccountName) {
            StringBuilder sb = new StringBuilder("/").Append(storageAccountName).Append(address.AbsolutePath);
            NameValueCollection values = HttpUtility.ParseQueryString(address.Query);

            foreach (var item in values.AllKeys.OrderBy(k => k)) {
                sb.Append('\n').Append(item.ToLower()).Append(':').Append(values[item]);
            }

            return sb.ToString();
        }

        public AuthenticationHeaderValue GetAuthenticationHeader(string storageAccountName, string storageAccountKey,
            DateTime now, 
            HttpRequestMessage httpRequestMessage, string ifMatch = "", string md5="") {
            HttpMethod method = httpRequestMessage.Method;
            String MessageSignature = String.Format("{0}\n\n\n{1}\n{5}\n\n\n\n{2}\n\n\n\n{3}{4}",
                method.ToString(),
                (method == HttpMethod.Get || method == HttpMethod.Head) ? String.Empty
                : httpRequestMessage.Content.Headers.ContentLength.ToString(),
                ifMatch,
                getCanonicalizedHeaders(httpRequestMessage),
                getCanonicalizedResource(httpRequestMessage.RequestUri, storageAccountName), md5);

            byte[] SignatureBytes = Encoding.UTF8.GetBytes(MessageSignature);

            HMACSHA256 SHA256 = new HMACSHA256(Convert.FromBase64String(storageAccountKey));


            AuthenticationHeaderValue AuthHV = new AuthenticationHeaderValue("SharedKey",
                storageAccountName + ":" + Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes)));
            return AuthHV;
        }   
    }
}