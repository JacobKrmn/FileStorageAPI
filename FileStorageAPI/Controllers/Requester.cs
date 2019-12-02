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

        public void initializeClient()
        {
            //Call before making any request.
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.Proxy = null;
            clientHandler.UseProxy = false;

            client = new HttpClient(clientHandler);
        }

        public async Task<string> makeRequest(HttpMethod method, string content = null)
        {
            AuthenticationBuilder authBuilder = new AuthenticationBuilder();

            String uri = string.Format("http://{0}.file.core.windows.net?comp=list", storageAcc);

            Byte[] requestPayload = null;

            using (var httpRequestMessage = new HttpRequestMessage(method, uri)
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
                    List<string> list = new List<string>();
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        string xmlString = await httpResponseMessage.Content.ReadAsStringAsync();


                        return xmlString;
                    }
                    else
                    {
                        string res = String.Format("Received Code {0}, accompanied by message: {1}",
                            httpResponseMessage.StatusCode.ToString(),
                            httpResponseMessage.Content.ToString());
                        return res.ToString();
                    }
                }

            }
        }
    }
}