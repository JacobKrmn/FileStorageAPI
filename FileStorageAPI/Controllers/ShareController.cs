using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Xml.Linq;
using FileStorageAPI.Models;

namespace FileStorageAPI.Controllers
{
    public class ShareController : ApiController
    {
        string storageURL = "http://conexysfilestorage.file.core.windows.net";
        Requester requester = new Requester();

        public List<string> Get() {
            //TODO: Implement RBAC support
            string url = storageURL + "?comp=list";

            String xmlString = requester.makeRequest(HttpMethod.Get, storageURL, "?comp=list").Result;
            List<string> results = new List<string>();

            int gtIndex = xmlString.IndexOf('<');
            if (gtIndex > 0)
            {
                xmlString = xmlString.Remove(0, gtIndex);
            }

            XElement x = XElement.Parse(xmlString);

            foreach (XElement container in x.Element("Shares").Elements("Share"))
            {
                results.Add(container.Element("Name").Value);
            }

            return results;
        }

        public void Post(string shareName, string resType, string path = "") {
            string uri = String.Format("{0}/{1}{2}?restype={3}", storageURL, shareName, path, resType);
            string result = requester.makeRequest(HttpMethod.Put, uri).Result;
        }

        public void Delete(string shareName, string resType) {
            string uri = String.Format("{0}/{1}?resType={2}", storageURL, shareName, resType);
            string result = requester.makeRequest(HttpMethod.Delete, uri).Result;
        }

    }
}
