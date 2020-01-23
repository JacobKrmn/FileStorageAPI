using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml.Linq;
using Microsoft.Azure;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.File;

namespace FileStorageAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class FolderController : ApiController
    {
        string storageUrl = "http://conexysfilestorage.file.core.windows.net";
        Requester requester = new Requester();

        public List<string> Get(string shareName, string path) {
            string uri = String.Format("{0}/{1}{2}?resType=directory&comp=list", storageUrl, shareName, path);
            string xmlString = requester.makeRequest(HttpMethod.Get, uri).Result;

            List<string> results = new List<string>();

            try
            {

                int gtIndex = xmlString.IndexOf('<');
                if (gtIndex > 0)
                {
                    xmlString = xmlString.Remove(0, gtIndex);
                }

                XElement x = XElement.Parse(xmlString);
                foreach (XElement container in x.Element("Entries").Elements("Directory"))
                {
                    results.Add(container.Element("Name").Value);
                }
            }
            catch (Exception ex) {
                results.Add(ex.Message);
            }
                return results;
        }

        public string Put(string shareName, string path, string dirName) {
            string uri = String.Format("{0}/{1}/{2}/{3}?restype=directory", storageUrl, shareName, path, dirName);
            return requester.makeRequest(HttpMethod.Put, uri).Result;
        }

        public string Delete(string shareName, string path, string dirName) {
            string uri = String.Format("{0}/{1}/{2}/{3}?restype=directory", storageUrl, shareName, path, dirName);
            return requester.makeRequest(HttpMethod.Delete, uri).Result;
        }

    }
}
