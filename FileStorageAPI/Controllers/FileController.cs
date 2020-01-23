using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml.Linq;
using Microsoft.Azure;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.File;

namespace FileStorageAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class FileController : ApiController
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

                foreach (XElement container in x.Element("Entries").Elements("File"))
                {
                    results.Add(container.Element("Name").Value);
                }
            }
            catch (Exception ex) {
                results.Add(ex.Message);
            }
            return results;
        }

        public string Get(string shareName, string path, string fileName) {
            CloudStorageAccount storageAcc = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudFileClient cloudFileClient = storageAcc.CreateCloudFileClient();
            CloudFileShare share = cloudFileClient.GetShareReference(shareName);
            try
            {
                if (share.Exists())
                {
                    CloudFileDirectory rootDir = share.GetRootDirectoryReference();
                    CloudFileDirectory targetDir = rootDir.GetDirectoryReference(path);
                    if (targetDir.Exists())
                    {
                        CloudFile file = targetDir.GetFileReference(fileName);
                        if (file.Exists())
                        {
                            return file.DownloadTextAsync().Result;
                        }
                        else return "Invalid file name.";
                    }
                    else return "Invalid directory";
                }
                else return "Invalid share.";
            }
            catch (Exception ex) {
                return ex.Message;
            }

        }

        public HttpResponseMessage Post(string shareName, string path, string fileName) {
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));
                    CloudFileClient cloudFileClient = cloudStorageAccount.CreateCloudFileClient();
                    CloudFileShare share = cloudFileClient.GetShareReference(shareName);

                    try
                    {
                        if (share.Exists())
                        {
                            CloudFileDirectory rootDir = share.GetRootDirectoryReference();
                            CloudFileDirectory targetDir = rootDir.GetDirectoryReference(path);

                            CloudFile cloudFile = targetDir.GetFileReference(fileName);
                            Stream fileStream = postedFile.InputStream;

                            cloudFile.UploadFromStream(fileStream);
                            fileStream.Dispose();

                            return Request.CreateResponse(HttpStatusCode.Created);
                        }
                        else {
                            return Request.CreateResponse(HttpStatusCode.BadRequest);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                    }
                }
            }
            else {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        public string Delete(string shareName, string path, string fileName) {
            string uri = String.Format("{0}/{1}{2}/{3}", storageUrl, shareName, path, fileName);
            return requester.makeRequest(HttpMethod.Delete, uri).Result;

        }

    }
}

