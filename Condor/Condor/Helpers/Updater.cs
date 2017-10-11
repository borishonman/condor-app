using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Condor.Helpers
{
    interface IUpdaterImpl
    {
        string GetFileType();
        int GetCurrentVersion();
    }
    class Updater
    {
        private static JArray ExecuteRequest(string addr)
        {
            Uri addrUri = new Uri(addr);
            Uri baseUri = new Uri(addrUri.Scheme + "://" + addrUri.Host);
            try
            {
                HttpClient httpClient = HttpClientFactory.Create(baseUri);
                httpClient.Timeout = new System.TimeSpan(0, 0, 5);
                //create the request object
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, addrUri.AbsolutePath);
                request.Headers.Add("User-Agent", "condor");
                //execute the request and get the response
                HttpResponseMessage msgresp = httpClient.SendAsync(request).Result;
                //check for and process redirects
                if (msgresp.StatusCode == HttpStatusCode.MovedPermanently || msgresp.StatusCode == HttpStatusCode.Redirect)
                {
                    httpClient = HttpClientFactory.Create(new Uri(msgresp.Headers.Location.Scheme + "://" + msgresp.Headers.Location.Host));
                    httpClient.Timeout = new System.TimeSpan(0, 0, 1);
                    request = new HttpRequestMessage(HttpMethod.Get, msgresp.Headers.Location.AbsolutePath);
                    request.Headers.Add("User-Agent", "condor");
                    msgresp = httpClient.SendAsync(request).Result;
                }
                string jsonResp = msgresp.Content.ReadAsStringAsync().Result;
                return JArray.Parse(jsonResp);
            }
            catch (AggregateException e)
            {
                return null;
            }
        }
        public static bool CheckForUpdates(out string addr, out string latestVersion, out string description)
        {
            IUpdaterImpl impl = DependencyService.Get<IUpdaterImpl>();

            string t = impl.GetFileType();
            int v = impl.GetCurrentVersion();

            JArray resp = ExecuteRequest("https://api.github.com/repos/borishonman/condor-app/releases");
            if (resp == null)
            {
                addr = null;
                latestVersion = "";
                description = "";
                return false;
            }

            //find the latest release with the binary type we want
            JObject latestRelease = null;
            JObject latestAsset = null;
            foreach (JToken tok in resp)
            {
                //get the asset based on the type of binary we want
                JObject asset = null;
                foreach (JToken tokA in (JArray)((JObject)tok).GetValue("assets"))
                {
                    JObject curAsset = tokA as JObject;
                    string file = (string)curAsset.GetValue("name");
                    if (file.EndsWith(t))
                    {
                        asset = curAsset;
                        break;
                    }
                }
                //if no asset was found, we know this release won't work
                if (asset == null)
                    continue;
                //check the release against the current app version
                string release = (string)((JObject)tok).GetValue("tag_name");
                release = release.Substring(1).Replace(".", "");
                if (int.Parse(release) > v)
                {
                    latestRelease = (JObject)tok;
                    latestAsset = asset;
                }
            }

            IConfig cfg = ConfigFactory.GetInstance();

            if (latestRelease != null && cfg.Address != null)
            {
                string releaseVer = (string)latestRelease.GetValue("tag_name");
                addr = latestRelease != null ? cfg.Address + "/app/" : null;
                latestVersion = releaseVer;
                description = (string)latestRelease.GetValue("body");
                return true;
            }

            addr = "";
            latestVersion = "";
            description = "";
            return false;
        }
    }
}
