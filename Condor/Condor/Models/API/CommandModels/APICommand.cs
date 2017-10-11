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
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace Condor.Models.API.CommandModels
{
    public class APICommand
    {
        public class Endpoint
        {
            private String m_str;

            public static Endpoint PROJECT = new Endpoint("/api/project/");
            public static Endpoint MEMBER = new Endpoint("/api/member/");
            public static Endpoint PERMISSION = new Endpoint("/api/permission/");
            public static Endpoint AUTH = new Endpoint("/api/auth/");

            private Endpoint(String str)
            {
                m_str = str;
            }

            public String str
            {
                get
                {
                    return m_str;
                }
            }
        }

        protected Endpoint m_ep;
        protected Dictionary<String, String> m_data;

        private JObject m_resp;

        public delegate void ResponseReceivedHandler(JObject response);
        public event ResponseReceivedHandler OnResponseReceived;

        protected APICommand()
        {
            m_data = new Dictionary<string, string>();
        }

        public APICommand(APICommand from)
        {
            m_ep = from.m_ep;
            m_data = from.m_data;
            m_resp = from.m_resp;
            OnResponseReceived = from.OnResponseReceived;
        }

        public Endpoint APIEndpoint
        {
            get
            {
                return m_ep;
            }
        }

        public String Token
        {
            get
            {
                return m_data["token"];
            }
            set
            {
                m_data["token"] = value;
            }
        }

        public String JSON
        {
            get
            {
                string res = "{";
                int i = 0;
                foreach (KeyValuePair<String,String> kvp in m_data)
                {
                    res += "\"" + kvp.Key + "\": \"" + kvp.Value + "\"";
                    if (i < m_data.Count - 1)
                        res += ",";
                    i++;
                }
                return res + "}";
            }
        }

        public JObject Response
        {
            get
            {
                return m_resp;
            }
            set
            {
                m_resp = value;
                Device.BeginInvokeOnMainThread(() => { this.OnResponseReceived(m_resp); });
            }
        }
    }
}
