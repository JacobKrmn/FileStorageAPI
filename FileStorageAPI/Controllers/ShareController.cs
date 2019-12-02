using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FileStorageAPI.Models;

namespace FileStorageAPI.Controllers
{
    public class ShareController : ApiController
    {
        string storageURL = "https://www.conexysfilestorage.file.core.windows.net";
        Requester requester = new Requester();

        public string Get() {
            //TODO: Implement RBAC support
            string url = storageURL + "?comp=list";
            requester.initializeClient();
            return requester.makeRequest(HttpMethod.Get).Result;
        }
    }
}
