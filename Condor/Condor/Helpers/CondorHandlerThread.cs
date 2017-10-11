/*
    This file is part of Condor.
    Condor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    Condor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with Condor.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Condor.Models.API.CommandModels;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace Condor.Helpers
{
    class CondorHandlerThread
    {
        private Semaphore m_wh;
        private Queue<APICommand> m_cmdQ;
        private Thread m_thread;
        private string m_addr;
        private static readonly object m_mutex = new object();

        public CondorHandlerThread(string addr)
        {
            m_wh = new Semaphore(0, 5);
            m_cmdQ = new Queue<APICommand>();
            m_addr = addr;

            m_thread = new Thread(new ParameterizedThreadStart(delegate { this.Run(); }));
        }

        public void Start()
        {
            m_thread.Start();
        }
        public void Stop()
        {
            Enqueue(null);
        }

        public void Enqueue(APICommand cmd)
        {
            lock (m_mutex) m_cmdQ.Enqueue(new APICommand(cmd));
            m_wh.Release();
        }
        public void Run()
        {
            APICommand curCmd;
            while(true)
            {
                m_wh.WaitOne(); //wait for a message to be enqueued
                lock (m_mutex) curCmd = m_cmdQ.Dequeue();
                //if null then exit the thread
                if (curCmd == null)
                    return;

                //process the command
                JObject resp = new JObject();
                //create the message handler and tell it to not automatically process redirects
                //create a client to execute the request
                HttpClient httpClient = null;
                try
                {
                    httpClient = HttpClientFactory.Create(new Uri(m_addr));
                    httpClient.Timeout = new System.TimeSpan(0, 0, 5);
                    //create the request object
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, curCmd.APIEndpoint.str);
                    request.Headers.Add("Accept", "application/json");
                    request.Content = new StringContent(curCmd.JSON, Encoding.UTF8, "application/json");
                    //execute the request and get the response
                    HttpResponseMessage msgresp = httpClient.SendAsync(request).Result;
                    //check for and process redirects
                    if (msgresp.StatusCode == HttpStatusCode.MovedPermanently || msgresp.StatusCode == HttpStatusCode.Redirect)
                    {
                        httpClient = HttpClientFactory.Create(new Uri(msgresp.Headers.Location.Scheme + "://" + msgresp.Headers.Location.Host));
                        httpClient.Timeout = new System.TimeSpan(0, 0, 1);
                        request = new HttpRequestMessage(HttpMethod.Post, msgresp.Headers.Location.AbsolutePath);
                        request.Headers.Add("Accept", "application/json");
                        request.Content = new StringContent(curCmd.JSON, Encoding.UTF8, "application/json");
                        msgresp = httpClient.SendAsync(request).Result;
                    }
                    string jsonResp = msgresp.Content.ReadAsStringAsync().Result;

                    //send callback with JSON response data
                    try
                    {
                        curCmd.Response = JObject.Parse(jsonResp);
                    }
                    catch (JsonException e)
                    {
                        curCmd.Response = JObject.Parse("{'result': 'fail', 'msg': 'The server appears to be down'}");
                    }
                }
                catch (AggregateException e)
                {
                    curCmd.Response = JObject.Parse("{'result': 'fail', 'msg': 'Failed to create HTTP client'}");
                }
            }
        }
    }
}
