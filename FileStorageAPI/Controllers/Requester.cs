using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using FileStorageAPI.Azure;

namespace FileStorageAPI.Controllers
{
    public class Requester
    {
        string accesskey = "d960uVY1DQtFpW9mojxA2YBr0NufuyMUfp0FTB7xo5g+dIIcOegjS9nqFQ9HMQrjmmPkDhPgbVA7v02sXxhA/w==";
        string storageAcc = "conexysfilestorage";

        private HttpClient client;

        public async Task<string> makeRequest(HttpMethod method, string uri, string content = null)
        {
            AuthenticationBuilder authBuilder = new AuthenticationBuilder();

            StringBuilder sb = new StringBuilder(uri);
            sb.Append(content);
            string requestUri = sb.ToString();

            Byte[] requestPayload = null;

            using (var httpRequestMessage = new HttpRequestMessage(method, requestUri)
            {
                Content = (requestPayload == null) ? null : new ByteArrayContent(requestPayload)
            }) {
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", "2017-07-29");
                httpRequestMessage.Headers.Authorization = authBuilder.GetAuthenticationHeader(storageAcc, accesskey,
                    now, httpRequestMessage);

                using (HttpResponseMessage httpResponseMessage =
                    await new HttpClient().SendAsync(httpRequestMessage).ConfigureAwait(false))
                {
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        String xmlString = await httpResponseMessage.Content.ReadAsStringAsync();
                        return xmlString;
                    }
                    else
                    {
                       string result = String.Format("Received Code {0}, accompanied by message: {1}",
                            httpResponseMessage.StatusCode.ToString(),
                            httpResponseMessage.Content.ToString());
                        return result;
                    }
                }

            }
        }
    }
}